import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectService } from '../service/project.service';
import { ProjectDto } from '../model/project';
import { RepositoryDto } from '../model/repository';
import { RepositoriesService } from '../service/repositories.service';
import { PackageDto, PackageType } from '../model/package';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { PackagesService } from '../service/packages.service';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { startWith, map, delay } from 'rxjs/operators';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-project-detail',
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.css']
})
export class ProjectDetailComponent implements OnInit {

  /** The id of the project to view details */
  currentProjectId: string;
  /** The project to display */
  project: ProjectDto;

  selectProjectControl : FormControl = new FormControl();

  // all repositories
  repositories: RepositoryDto[];

  // indicate if the user wants to edit project configurations
  edit: boolean = false;

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;

  constructor(private projectService: ProjectService,
    private repositoriesService: RepositoriesService,
    private packageService: PackagesService,
    private route: ActivatedRoute,
    private router: Router) { }

  /**
   * Retrieve id of the project to display
   * and retrieve all details of the project
   * */
  ngOnInit() {
    this.route.paramMap.subscribe(params => {

      this.currentProjectId = params.get('id');
      console.log(params.get('id'));

      this.Refresh();
    });
  }

  /**
   * Refresh
   * */
  Refresh(): void {
    // Get details of the project
    this.projectService.GetProject(this.currentProjectId).subscribe(res => {
      this.project = res;
    },
      error => console.error(error));

    this.repositoriesService.GetRepositories()
      .subscribe(res => {
        this.repositories = res;
      },
        error => console.error(error));
  }

  /**
   * Delete the project
   * */
  DeleteProject(): void {
    this.projectService.DeleteProject(this.currentProjectId)
      .subscribe(_ => this.router.navigate(['..'], { relativeTo: this.route })
        ,error => console.error(error));
  }

  /**
   * Update changes to server
   * */
  UpdateProject(): void {
    this.projectService.UpdateProject(this.project)
      .subscribe(_ => this.edit = false,
        error => console.error(error));
  }

  /**
   * Cancel editing the configuration
   * */
  CancelUpdateProject(): void {
    this.edit = false;
    this.projectService.GetProject(this.currentProjectId).subscribe(res => {
      this.project = res;
    },
      error => console.error(error));
  }

  /**
   * Start editing project properties
   * */
  EditProject(): void {
    this.edit = true;
  }

  /**
   * Export all detected soup to a csv file
   * */
  GetCsvUrl(): string {
    return this.packageService.GetCsvUrl(this.currentProjectId);
  }

  /**
   * Launch the analysis of the project
   * */
  Analyse(): void {
    this.projectService.LaunchProject(this.currentProjectId)
      .subscribe(_ => { this.Refresh(); },
        error => {
          this.HandelError(error);
        });
  }

  /**
  * Display error to client
  * @param error the error to display
  */
  HandelError(error: HttpErrorResponse): void {
    window.alert(error.error.detail);
  }

  /**
   * Stop the executing project
   * */
  Stop(): void {
    this.projectService.StopProject(this.currentProjectId)
      .subscribe(_ => { },
        error => {
          this.HandelError(error);
        });
  }
}
