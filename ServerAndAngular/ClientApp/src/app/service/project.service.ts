import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ProjectDto } from '../Model/Project';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  /**
   * Launch the project analysis
   * @param name the name of the project to launch
   */
  LaunchProject(name: string) {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    let request = this.httpClient.post<ProjectDto>(this.baseUrl + "api/Projects/Start", JSON.stringify(name), { headers: headerOptions });
    request.subscribe(res => console.log(res), error => console.error(error));
  }

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  /**
   * Add a project on database
   * @param project the project to save to database
   */
  AddProject(project: ProjectDto): Observable<ProjectDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    let request = this.httpClient.post<ProjectDto>(this.baseUrl + 'api/Projects', JSON.stringify(project), { headers: headerOptions });
    request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }

  /**
   * Return all project of the database
   * */
  GetProjects(): Observable<ProjectDto[]> {
    return this.httpClient.get<ProjectDto[]>(this.baseUrl + 'api/Projects');
  }
}
