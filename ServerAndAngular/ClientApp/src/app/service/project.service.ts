import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ProjectDto, ProjectWithDetailsDto } from '../Model/Project';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  /**
   * Update the project to server
   * @param project The new state of the project
   */
  UpdateProject(project: ProjectDto): Observable<ProjectDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    let request = this.httpClient.put<ProjectDto>(this.baseUrl + 'api/Projects/' + project.name, JSON.stringify(project), { headers: headerOptions })
    //request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }

  /**
   * Return the url that permit to download csv file of the project
   * */
  GetCsvUrl(projectId: string) : string {
    return this.baseUrl + "api/Projects/exporttocsv/" + projectId;
  }

  /**
   * Launch the project analysis
   * @param name the name of the project to launch
   */
  LaunchProject(name: string) {
    let request = this.httpClient.post<ProjectDto>(this.baseUrl + "api/Projects/Start/" + name, null);
    request.subscribe(res => console.log(res), error => console.error(error));
  }

  /**
   * Add a project on database
   * @param project the project to save to database
   */
  AddProject(project: ProjectDto): Observable<ProjectDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    let request = this.httpClient.post<ProjectDto>(this.baseUrl + 'api/Projects', JSON.stringify(project), { headers: headerOptions });
    //request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }

  /**
   * Return all project of the database
   * */
  GetProjects(): Observable<ProjectDto[]> {
    let request = this.httpClient.get<ProjectDto[]>(this.baseUrl + 'api/Projects');
    //request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }

  /**
   * Return the project, with all packages detected
   * @param name the name of the project to retrieve
   */
  GetProject(name: string): Observable<ProjectWithDetailsDto>{
    let request = this.httpClient.get<ProjectWithDetailsDto>(this.baseUrl + 'api/Projects/' + name);
    //request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }
}
