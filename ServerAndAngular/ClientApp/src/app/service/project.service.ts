import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ProjectDto } from '../model/project';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  projectApiEndpoint: string = this.baseUrl + 'api/Projects/';

  constructor(private httpClient: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) { }

  /**
   * Add a project on database
   * @param project the project to save to database
   */
  AddProject(project: ProjectDto): Observable<ProjectDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.post<ProjectDto>(this.projectApiEndpoint, JSON.stringify(project), { headers: headerOptions });
  }

  /**
   * Launch the project analysis
   * @param projectName the name of the project to launch
   */
  LaunchProject(projectName: string) {
    return this.httpClient.post<boolean>(this.projectApiEndpoint + 'Start/' + projectName, null);
  }

  /**
   * Stop the executing project
   * @param projectName the name of the project to Stop
   */
  public StopProject(projectName: string): Observable<boolean> {
    return this.httpClient.post<boolean>(this.projectApiEndpoint + 'Stop/' + projectName, null);
  }

  /**
   * Return all project of the database
   * */
  GetProjects(): Observable<ProjectDto[]> {
    return this.httpClient.get<ProjectDto[]>(this.projectApiEndpoint);
  }

  /**
   * Return the project, with all packages detected
   * @param name the name of the project to retrieve
   */
  GetProject(name: string): Observable<ProjectDto> {
    return this.httpClient.get<ProjectDto>(this.projectApiEndpoint + name);
  }

  /**
   * Return all project consumers (csproj)
   * @param projectName
   */
  public GetAllPackageConsummer(projectName: string): Observable<string[]> {
    return this.httpClient.get<string[]>(this.projectApiEndpoint + 'projectConsumers/' + projectName);
  }

  /**
   * Update the project to server
   * @param project The new state of the project
   */
  UpdateProject(project: ProjectDto): Observable<ProjectDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.put<ProjectDto>(this.projectApiEndpoint + project.name, JSON.stringify(project), { headers: headerOptions });
  }

  /**
   * Remove a project from its name
   * @param currentProjectId
   */
  DeleteProject(currentProjectId: string): Observable<ProjectDto> {
    return this.httpClient.delete<ProjectDto>(this.projectApiEndpoint + currentProjectId);
  }
}
