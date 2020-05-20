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

  repositories: RepositoryDto[];
  packagesTableSource: MatTableDataSource<PackageDto>;
  packages: PackageDto[];
  displayedColumns: string[] = ['packageId', 'version'];
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
      this.projectService.GetProject(this.currentProjectId).subscribe(res => {
        this.project = res;
      })
      this.packageService.GetPackageFromProjectName(this.currentProjectId)
        .subscribe(res => {
          this.packages = res;
          this.packagesTableSource = new MatTableDataSource<PackageDto>(this.packages);
          this.packagesTableSource.paginator = this.paginator;
        });
    });
    this.repositoriesService.GetRepositories()
      .subscribe(res => this.repositories = res);
  }

  /**
   * Delete the project
   * */
  DeleteProject(): void {
    this.projectService.DeleteProject(this.currentProjectId)
      .subscribe(res => this.router.navigate(['..'], { relativeTo: this.route }));
  }

  /**
   * Update changes to server
   * */
  UpdateProject(): void {
    this.projectService.UpdateProject(this.project)
      .subscribe(res => this.edit = false);
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
    return this.projectService.GetCsvUrl(this.currentProjectId);
  }

  /**
   * Filter list of packages
   * @param event the event that contains the filter request
   */
  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.packagesTableSource.filter = filterValue.trim().toLowerCase();
  }

  /**
   * Launch the analysis of the project
   * */
  Analyse(): void {
    this.projectService.LaunchProject(this.currentProjectId)
      .subscribe(res => { });
  }
}
