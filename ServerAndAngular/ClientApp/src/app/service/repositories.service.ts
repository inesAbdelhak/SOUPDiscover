import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RepositoryDto } from '../Model/repository';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
/**
* Manage repositories on server
*/
export class RepositoriesService {
  /**
   * Delete a repository
   * @param currentProjectId
   */
  DeleteProject(currentProjectId: string): Observable<RepositoryDto> {
    return this.httpClient.delete("api/repositories/" + currentProjectId);
  }

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  /**
   * Create a new repository
   * @param repository the new repository to create
   */
  AddRepository(repository: RepositoryDto): Observable<RepositoryDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    let request = this.httpClient.post<RepositoryDto>(this.baseUrl + 'api/repositories', JSON.stringify(repository), { headers: headerOptions });
    request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }

  /**
   *  Return all repositories on database
   */
  GetRepositories(): Observable<RepositoryDto[]> {
    return this.httpClient.get<RepositoryDto[]>(this.baseUrl + 'api/repositories');
  }

  /**
   * Get a repository configuration
   * @param id
   */
  GetRepository(id: string): Observable<RepositoryDto>{
    let request = this.httpClient.get<RepositoryDto>(this.baseUrl + 'api/repositories/' + id);
    request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }

  /**
   * Update the repository
   * @param repository the repository to update
   */
  UpdateRepository(repository: RepositoryDto) {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    let request = this.httpClient.put<RepositoryDto>(this.baseUrl + "api/repositories/" + repository.name, JSON.stringify(repository), { headers: headerOptions });
    request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }
}
