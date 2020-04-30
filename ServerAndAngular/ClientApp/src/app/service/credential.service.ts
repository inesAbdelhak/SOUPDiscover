import { Injectable, Inject } from '@angular/core';
import { CredentialDto } from '../Model/Credential';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root'
})
export class CredentialService {

  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  AddCredential(credential: CredentialDto) {
    return this.httpClient.post<CredentialDto>(this.baseUrl + 'api/credentials', credential);
  }

  GetCredentials(): Observable<CredentialDto[]> {
    return this.httpClient.get<CredentialDto[]>(this.baseUrl + 'api/credentials');
  }
}
