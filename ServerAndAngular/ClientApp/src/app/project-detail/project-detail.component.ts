import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProjectService } from '../service/project.service';
import { ProjectWithDetailsDto } from '../Model/Project';
import { RepositoryDto } from '../Model/repository';
import { RepositoriesService } from '../service/repositories.service';
import { PackageDto, PackageType } from '../Model/package';

@Component({
  selector: 'app-project-detail',
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.css']
})
export class ProjectDetailComponent implements OnInit {

  /** The id of the project to view details */
  currentProjectId: string;
  /** The project to display */
  project: ProjectWithDetailsDto;

  repositories: RepositoryDto[];
  packages: PackageDto[];
  displayedColumns: string[] = ['packageId', 'version'];
  edit: boolean = false;

  constructor(private projectService: ProjectService, private repositoriesService: RepositoriesService, private route: ActivatedRoute) { }

  /**
   * Retrieve id of the project to display
   * and retrieve all details of the project
   * */
  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.currentProjectId = params.get('id');
      console.log(params.get('id'));
      this.projectService.GetProject(this.currentProjectId).subscribe(res => this.project = res);
    });
    this.repositoriesService.GetRepositories().subscribe(res => this.repositories = res);
  }

  /**
   * Update changes to server
   * */
  UpdateProject(): void {
    this.projectService.UpdateProject(this.project);
  }

  /**
   * Export all detected soup to a csv file
   * */
  GetCsvUrl(): string {
    return this.projectService.GetCsvUrl(this.currentProjectId);
  }
}
