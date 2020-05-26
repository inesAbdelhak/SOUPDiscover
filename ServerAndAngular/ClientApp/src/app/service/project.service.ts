import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ProjectDto } from '../model/project';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  

  constructor(private httpClient: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) { }

  /**
   * Update the project to server
   * @param project The new state of the project
   */
  UpdateProject(project: ProjectDto): Observable<ProjectDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.put<ProjectDto>(this.baseUrl + 'api/Projects/' + project.name, JSON.stringify(project), { headers: headerOptions })
  }

  /**
   * Launch the project analysis
   * @param name the name of the project to launch
   */
  LaunchProject(name: string) {
    return this.httpClient.post<ProjectDto>(this.baseUrl + "api/Projects/Start/" + name, null);
  }

  /**
   * Add a project on database
   * @param project the project to save to database
   */
  AddProject(project: ProjectDto): Observable<ProjectDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.post<ProjectDto>(this.baseUrl + 'api/Projects', JSON.stringify(project), { headers: headerOptions });
  }

  /**
   * Return all project of the database
   * */
  GetProjects(): Observable<ProjectDto[]> {
    return this.httpClient.get<ProjectDto[]>(this.baseUrl + 'api/Projects');
  }

  /**
   * Return the project, with all packages detected
   * @param name the name of the project to retrieve
   */
  GetProject(name: string): Observable<ProjectDto>{
    return this.httpClient.get<ProjectDto>(this.baseUrl + 'api/Projects/' + name);
  }

  /**
   * Remove a project from its name
   * @param currentProjectId
   */
  DeleteProject(currentProjectId: string): Observable<ProjectDto> {
    return this.httpClient.delete<ProjectDto>(this.baseUrl + 'api/Projects/' + currentProjectId);
  }

  /**
   * Return all project consumers (csproj)
   * @param projectName
   */
  public GetAllPackageConsummer(projectName: string): Observable<string[]> {
    return this.httpClient.get<string[]>(this.baseUrl + "api/Projects/projectConsumers/" + projectName);

  }
}
