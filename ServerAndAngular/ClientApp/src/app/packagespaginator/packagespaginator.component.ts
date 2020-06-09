import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { Observable } from 'rxjs';
import { RepositoryDto } from '../model/repository';
import { MatTableDataSource } from '@angular/material/table';
import { PackageDto } from '../model/package';
import { PackagesService } from '../service/packages.service';
import { ProjectService } from '../service/project.service';
import { startWith, map } from 'rxjs/operators';
import { FormControl } from '@angular/forms';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';

@Component({
  selector: 'app-packagespaginator',
  templateUrl: './packagespaginator.component.html',
  styleUrls: ['./packagespaginator.component.css']
})
export class PackagespaginatorComponent implements OnInit {

  constructor(public projectService: ProjectService,
    public packageService: PackagesService) {
  }

  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;

  @Input() currentProjectId: string;

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
  displayedColumns: string[] = ['packageId', 'version', 'packageType', 'description', 'licence'];

  selectProjectControl: FormControl = new FormControl();

  /**
   * Refresh
   * */
  Refresh(): void {
    // Get all packages of the project
    this.packageService.GetPackageFromProjectName(this.currentProjectId)
      .subscribe(res => {
        this.packages = res;
        this.packagesTableSource = new MatTableDataSource<PackageDto>(this.packages);
        this.packagesTableSource.paginator = this.paginator;
      },
        error => console.error(error));

    // Get packages consumer of the project (csproj file names)
    this.projectService.GetAllPackageConsummer(this.currentProjectId)
      .subscribe(resultat => {
        this.packageConsumers = resultat;
        // Filter
        this.filteredConsumers = this.selectProjectControl.valueChanges.pipe(
          startWith(''),
          map(value => this.filterPackageConsumer(value))
        );
      },
        error => console.error(error));

    // Filter
    this.filteredConsumers = this.selectProjectControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterPackageConsumer(value))
    );
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
      .subscribe(resultat => {
        this.packages = resultat;
        this.packagesTableSource.data = resultat;
      },
        error => console.error(error));
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

  ngOnInit() {
    this.Refresh();
  }

}
