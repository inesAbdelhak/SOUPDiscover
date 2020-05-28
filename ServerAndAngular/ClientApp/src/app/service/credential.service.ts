import { Injectable, Inject } from '@angular/core';
import { CredentialDto } from '../model/credential';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { catchError } from 'rxjs/operators';

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
    return this.httpClient.post<CredentialDto>(this.baseUrl + 'api/Credentials', JSON.stringify(credential), { headers: headerOptions });
  }

  /**
   * Delete the credential
   * @param crednetialId Id of the credential to delete
   */
  DeleteCredential(crednetialId: string): Observable<object> {
    return this.httpClient.delete(this.baseUrl + 'api/Credentials/' + crednetialId);
  }

  /**
   * Return all credentials of the database
   * */
  GetCredentials(): Observable<CredentialDto[]> {
    return this.httpClient.get<CredentialDto[]>(this.baseUrl + 'api/Credentials');
  }
}
