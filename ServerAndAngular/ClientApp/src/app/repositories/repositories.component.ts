import { Component, OnInit } from '@angular/core';
import { RepositoriesService } from '../service/repositories.service';
import { RepositoryDto } from '../model/repository';

@Component({
  selector: 'app-repositories',
  templateUrl: './repositories.component.html',
  styleUrls: ['./repositories.component.css']
})
export class RepositoriesComponent implements OnInit {
  /* All repositories */
  repositories: RepositoryDto[];

  constructor(public repositoriesService: RepositoriesService) { }

  /**
 * To update repositories list, when a project is added or deleted
 */
  repositoryListUpdate = function (repository: RepositoryDto): void {
    this.repositoriesService.GetRepositories()
      .subscribe(result => this.repositories = result);
  }

  ngOnInit() {
    /* Read all repositories */
    this.repositoriesService.GetRepositories()
      .subscribe(res => this.repositories = res,
        error => console.error(error));
  }
}
