import { Injectable, Inject } from '@angular/core';
import { CredentialDto } from '../Model/credential';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { RepositoryDto } from '../Model/repository';

@Injectable({
  providedIn: 'root'
})
export class CredentialService {
  
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  /**
   * Add a credential on database
   * @param credential the credential to save to database
   */
  AddCredential(credential: CredentialDto): Observable<CredentialDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    let request = this.httpClient.post<CredentialDto>(this.baseUrl + 'api/Credentials', JSON.stringify(credential), { headers: headerOptions });
    // request.subscribe(res => console.log(res), error => console.error(error));
    return request;
  }

  /**
   * Return all credentials of the database
   * */
  GetCredentials(): Observable<CredentialDto[]> {
    return this.httpClient.get<CredentialDto[]>(this.baseUrl + 'api/Credentials');
  }
}
