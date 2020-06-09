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
import { ToastrService } from 'ngx-toastr';
import { ProcessStatus } from '../model/processStatus';

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

  processStatus = ProcessStatus;

  selectProjectControl: FormControl = new FormControl();

  // all repositories
  repositories: RepositoryDto[];

  // indicate if the user wants to edit project configurations
  edit = false;

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;

  constructor(private projectService: ProjectService,
    private toastr: ToastrService,
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
   * Refresh data to display
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
      .subscribe(_ => {
        this.toastr.success('Le projet ' + this.currentProjectId + ' a bien été supprimé', 'Projet');
        this.router.navigate(['..'], { relativeTo: this.route });

      }
        , error => this.HandleError(error));
  }

  /**
   * Update changes to server
   * */
  UpdateProject(): void {
    this.projectService.UpdateProject(this.project)
      .subscribe(_ => {
        this.edit = false;
        this.toastr.success('Les modifications du projet ' + this.currentProjectId + 'on bien été appliquées.', 'Projet');
      },
        error => this.HandleError(error));
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
  Analyze(): void {
    this.projectService.LaunchProject(this.currentProjectId)
      .subscribe(_ => {
        this.Refresh();
        this.toastr.success('L\'analyse du projet ' + this.currentProjectId + ' a été stopée', 'Projet');
      },
        error => {
          this.HandleError(error);
        });
  }

  /**
  * Display error to client
  * @param error the error to display
  */
  HandleError(error: HttpErrorResponse): void {
    this.toastr.error(error.error.detail, 'Projet');
  }

  /**
   * Stop the executing project
   * */
  Stop(): void {
    this.projectService.StopProject(this.currentProjectId)
      .subscribe(_ => { },
        error => {
          this.HandleError(error);
        });
  }
}
