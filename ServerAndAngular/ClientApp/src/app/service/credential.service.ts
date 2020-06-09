import { Injectable, Inject } from '@angular/core';
import { CredentialDto } from '../model/credential';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root'
})
export class CredentialService {

  credentialApiEndpoint: string = this.baseUrl + 'api/Credentials/';

  constructor(private httpClient: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  /**
  * Add a credential on database
  * @param credential the credential to save to database
  */
  AddCredential(credential: CredentialDto): Observable<CredentialDto> {
    const headerOptions = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.httpClient.post<CredentialDto>(this.credentialApiEndpoint, JSON.stringify(credential), { headers: headerOptions });
  }

  /**
   * Return all credentials of the database
   * */
  GetCredentials(): Observable<CredentialDto[]> {
    return this.httpClient.get<CredentialDto[]>(this.credentialApiEndpoint);
  }

  /**
   * Delete a credential
   * @param credentialId Id of the credential to delete
   */
  DeleteCredential(credentialId: string): Observable<object> {
    return this.httpClient.delete(this.credentialApiEndpoint + credentialId);
  }
}
