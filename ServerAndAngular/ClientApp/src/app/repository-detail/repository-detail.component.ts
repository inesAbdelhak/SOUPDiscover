import { Component, OnInit } from '@angular/core';
import { RepositoryDto } from '../model/repository';
import { RepositoriesService } from '../service/repositories.service';
import { ActivatedRoute } from '@angular/router';
import { CredentialDto } from '../model/credential';
import { CredentialService } from '../service/credential.service';

@Component({
  selector: 'app-repository-detail',
  templateUrl: './repository-detail.component.html',
  styleUrls: ['./repository-detail.component.css']
})
export class RepositoryDetailComponent implements OnInit {

  repositoryId: string;
  repository: RepositoryDto;
  availableCredentials: CredentialDto[];
  edit: boolean = false;

  constructor(private repositoriesService: RepositoriesService,
    private route: ActivatedRoute,
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
    })
    // Retrieve all available credentials
    this.credentialService.GetCredentials()
      .subscribe(res => this.availableCredentials = res);
  }

  /**
   * Update the server with the current state of the repository configuration
   * */
  UpdateRepository(): void {
    this.repositoriesService.UpdateRepository(this.repository).subscribe(res => this.edit = false);
  }

  /**
   * Start editing the repository configuration
   * */
  EditRepository(): void {
    this.edit = true;
  }

  /**
   * Cancel editing repository configuration
   * */
  CancelUpdateRepository(): void {
    this.edit = false;
    this.repositoriesService.GetRepository(this.repositoryId)
      .subscribe(res => {
        this.repository = res;
      });
  }
}
