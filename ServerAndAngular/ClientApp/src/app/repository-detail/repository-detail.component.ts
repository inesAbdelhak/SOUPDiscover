import { Component, OnInit } from '@angular/core';
import { RepositoryDto } from '../model/repository';
import { RepositoriesService } from '../service/repositories.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CredentialDto } from '../model/credential';
import { CredentialService } from '../service/credential.service';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { RepositoryType } from '../model/repositoryType';

@Component({
  selector: 'app-repository-detail',
  templateUrl: './repository-detail.component.html',
  styleUrls: ['./repository-detail.component.css']
})
export class RepositoryDetailComponent implements OnInit {

  repositoryId: string;
  repository: RepositoryDto;
  availableCredentials: CredentialDto[];
  edit = false;
  repositoryType = RepositoryType;

  constructor(private repositoriesService: RepositoriesService,
    private toastr: ToastrService,
    private route: ActivatedRoute,
    private router: Router,
    private credentialService: CredentialService) { }

  ngOnInit() {
    // Load the repository data
    this.route.paramMap.subscribe(params => {
      this.repositoryId = params.get('id');
      console.log(this.repositoryId);
      this.repositoriesService.GetRepository(this.repositoryId)
        .subscribe(res => {
          this.repository = res;
        });
    });
    // Retrieve all available credentials
    this.credentialService.GetCredentials()
      .subscribe(res => this.availableCredentials = res);
  }

  /**
   * Update the server with the current state of the repository configuration
   * */
  UpdateRepository(): void {
    this.repositoriesService.UpdateRepository(this.repository)
      .subscribe(_ => {
        this.edit = false;
        this.toastr.success('Le dépot "' + this.repositoryId + '" a bien été mis à jour', 'Dépot');
      },
        error => this.HandleError(error));
  }

  /**
   * Start editing the repository configuration
   * */
  EditRepository(): void {
    this.edit = true;
  }

  /**
   * Try to delete the repository
   * */
  DeleteRepository(): void {
    this.repositoriesService.DeleteRepository(this.repositoryId)
      .subscribe(_ => {
        this.toastr.success('Le dépot "' + this.repositoryId + '" a bien été supprimé', 'Dépot');
        // Navigate to the repository list
        this.router.navigate(['/repositories']);
      },
        error => this.HandleError(error));
  }

  /**
   * Cancel editing repository configuration
   * */
  CancelUpdateRepository(): void {
    this.edit = false;
    this.repositoriesService.GetRepository(this.repositoryId)
      .subscribe(resultat => {
        this.repository = resultat;
      });
  }

  /**
   * Display error to user
   * @param error the error to display
   */
  HandleError(error: HttpErrorResponse): void {
    this.toastr.error(error.error.detail, 'Dépot');
  }
}
