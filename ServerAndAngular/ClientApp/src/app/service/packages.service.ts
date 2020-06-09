import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PackageDto } from '../model/package';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PackagesService {

  packagesApiEndpoint: string = this.baseUrl + 'api/Packages/';

  constructor(private httpClient: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  /**
   * Return all package of the project defined
   * @param projectName
   */
  public GetPackageFromProjectName(projectName: string, csproj: string = null): Observable<PackageDto[]> {
    if (projectName == null) {
      throw new Error('projectName must be not null!');
    }
    let request = this.packagesApiEndpoint + 'filter?projectName=' + projectName;
    if (csproj != null) {
      request += '&csproj=' + csproj;
    }
    return this.httpClient.get<PackageDto[]>(request);
  }

  /**
 * Return the url that permit to download csv file of the project
 * */
  GetCsvUrl(projectId: string): string {
    return this.packagesApiEndpoint + 'exporttocsv/' + projectId;
  }
}
