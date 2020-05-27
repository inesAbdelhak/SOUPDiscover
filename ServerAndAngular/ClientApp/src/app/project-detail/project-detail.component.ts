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
import { startWith, map } from 'rxjs/operators';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';

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

  // The selected csproj
  packageConsumerSelected: string;

  // all available csproj
  packageConsumers: string[] = [''];

  filteredConsumers: Observable<string[]>;

  // all repositories
  repositories: RepositoryDto[];

  // object to filter package from their name
  packagesTableSource: MatTableDataSource<PackageDto>;

  // package filtered with csproj file name
  packages: PackageDto[];

  // the columns to display
  displayedColumns: string[] = ['packageId', 'version', 'licence'];

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

      // Get details of the project
      this.projectService.GetProject(this.currentProjectId).subscribe(res => {
        this.project = res;
      })

      // Get all packages of the project
      this.packageService.GetPackageFromProjectName(this.currentProjectId)
        .subscribe(res => {
          this.packages = res;
          this.packagesTableSource = new MatTableDataSource<PackageDto>(this.packages);
          this.packagesTableSource.paginator = this.paginator;
        });

      // Get packages consumer of the project (csproj file names)
      this.projectService.GetAllPackageConsummer(this.currentProjectId)
        .subscribe(res => {
          this.packageConsumers = res;
          // Filter
          this.filteredConsumers = this.selectProjectControl.valueChanges.pipe(
            startWith(''),
            map(value => this.filterPackageConsumer(value))
          );
        });
    });

    this.repositoriesService.GetRepositories()
      .subscribe(res => {
        this.repositories = res;
      });

    // Filter
    this.filteredConsumers = this.selectProjectControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterPackageConsumer(value))
    );
  }

  /**
   * filter package consumer client side
   * @param value
   */
  private filterPackageConsumer(value: string): string[] {
    const filterValue = this._normalizeValue(value);
    return this.packageConsumers.filter(p => this._normalizeValue(p).includes(filterValue));
  }

  private _normalizeValue(value: string): string {
    return value.toLowerCase().replace(/\s/g, '');
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
   * Cancel editing the configuration
   * */
  CancelUpdateProject(): void {
    this.edit = false;
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
   * Filter list of packages
   * @param event the event that contains the filter request
   */
  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.packagesTableSource.filter = filterValue.trim().toLowerCase();
  }

  applyCsProjFilter(event: MatAutocompleteSelectedEvent) {
    this.packageConsumerSelected = event.option.viewValue;
    // Update package List
    this.packageService.GetPackageFromProjectName(this.currentProjectId, this.packageConsumerSelected)
      .subscribe(res => {
        this.packages = res;
        this.packagesTableSource.data = res;
      });
  }

  /**
   * Launch the analysis of the project
   * */
  Analyse(): void {
    this.projectService.LaunchProject(this.currentProjectId)
      .subscribe(res => { });
  }
}
