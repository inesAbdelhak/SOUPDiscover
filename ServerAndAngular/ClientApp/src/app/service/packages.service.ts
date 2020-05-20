import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PackageDto } from '../model/package';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PackagesService {

  constructor(private httpClient: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) { }

  /**
   * Return all package of the project defined
   * @param projectName
   */
  public GetPackageFromProjectName(projectName: string): Observable<PackageDto[]> {
    return this.httpClient.get<PackageDto[]>(this.baseUrl + "api/Packages/fromprojectName/" + projectName);
  }

  /**
 * Return the url that permit to download csv file of the project
 * */
  GetCsvUrl(projectId: string): string {
    return this.baseUrl + "api/Packages/exporttocsv/" + projectId;
  }
}
