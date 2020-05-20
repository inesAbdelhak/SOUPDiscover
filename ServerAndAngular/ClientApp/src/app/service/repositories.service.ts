import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RepositoryDto } from '../model/repository';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
/**
* Manage repositories on server
*/
export class RepositoriesService {

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  /**
   * Delete a repository
   * @param currentProjectId
   */
  DeleteRepository(repositoryId: string): Observable<RepositoryDto> {
    return this.httpClient.delete("api/repositories/" + repositoryId);
  }

  /**
   * Create a new repository
   * @param repository the new repository to create
   */
  AddRepository(repository: RepositoryDto): Observable<RepositoryDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.post<RepositoryDto>(this.baseUrl + 'api/repositories', JSON.stringify(repository), { headers: headerOptions });
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
    return this.httpClient.get<RepositoryDto>(this.baseUrl + 'api/repositories/' + id);
  }

  /**
   * Update the repository
   * @param repository the repository to update
   */
  UpdateRepository(repository: RepositoryDto) {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.put<RepositoryDto>(this.baseUrl + "api/repositories/" + repository.name, JSON.stringify(repository), { headers: headerOptions });
  }
}
