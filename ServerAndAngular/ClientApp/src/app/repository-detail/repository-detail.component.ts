import { Component, OnInit } from '@angular/core';
import { RepositoryDto } from '../Model/repository';
import { RepositoriesService } from '../service/repositories.service';
import { ActivatedRoute } from '@angular/router';
import { CredentialDto } from '../Model/credential';
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

  constructor(private repositoriesService: RepositoriesService,
    private route: ActivatedRoute,
    private credentialService: CredentialService) { }

  ngOnInit() {
    // Load the repository data
    this.route.paramMap.subscribe(params => {
      this.repositoryId = params.get('id');
      console.log(this.repositoryId);
      this.repositoriesService.GetRepository(this.repositoryId)
        .subscribe(res => this.repository = res);
    })
    // Retrieve all available credentials
    this.credentialService.GetCredentials()
      .subscribe(res => this.availableCredentials = res);
  }

  UpdateRepository(): void {
    this.repositoriesService.UpdateRepository(this.repository);
  }
}
