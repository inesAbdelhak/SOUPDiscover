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

  // Element contains at least one element
  positivefilter: string[] = [];

  // Element doesn't contains one element
  negativefilter: string[] = [];

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
        this.packagesTableSource.filterPredicate = (a, b) => { return this.filterElement(a, b) };
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

  // Update fields positivefilter and negativefilter form current filter
  updateFilter(filter: string) {
    var list = filter.split(' ');
    this.positivefilter = [];
    this.negativefilter = [];
    list.forEach(value => {
      value = value.trim();
      if (value.startsWith('+') && value.length > 1) {
        this.positivefilter.push(value.substring(1));
      }
      else if (value.startsWith('-') && value.length > 1) {
        this.negativefilter.push(value.substring(1));
      }
      else {
        this.positivefilter.push(value);
      }
    }); 
  }

  /**
  * Filter list of packages
  * @param event the event that contains the filter request
  */
  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.updateFilter(filterValue);
    this.packagesTableSource.filter = filterValue.trim().toLowerCase();
  }

  filterElement(data: PackageDto, filter: string) : boolean {
    var isok: boolean = true;
    if (this.positivefilter.length > 0) {
      isok = isok && this.positivefilter.every(function f(value: string, index: number, array: string[]) {
        return data.packageId.lastIndexOf(value) != -1;
      });
    }

    if (this.negativefilter.length > 0) {
      isok = isok && this.negativefilter.every(function f(value: string, index: number, array: string[]) {
        return data.packageId.lastIndexOf(value) == -1;
      });
    }
    return isok;
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
