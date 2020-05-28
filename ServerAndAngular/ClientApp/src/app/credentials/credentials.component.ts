import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CredentialService } from '../service/credential.service';
import { CredentialDto } from '../model/credential';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-credentials',
  templateUrl: './credentials.component.html',
  styleUrls: ['./credentials.component.css']
})
export class CredentialsComponent implements OnInit {
  public credentials: CredentialDto[];
  constructor(private credentialService: CredentialService) {
  }

  /**
  * To update credential list, when a credential is added or deleted
  */
  projectListUpdate = function (credential: CredentialDto): void {
    this.credentialService.GetCredentials().subscribe(result => {
      this.credentials = result;
    }, error => console.error(error));
  }

  /**
   * Delete the credential
   * */
  DeleteCredential(credentialDto: CredentialDto): void {
    this.credentialService.DeleteCredential(credentialDto.name)
      .subscribe(_ => {
        let index = this.credentials.indexOf(credentialDto);
        this.credentials.splice(index);
      },
        error => this.HandelError(error));
  }

  HandelError(error: HttpErrorResponse): void {
    window.alert(error.error.detail);
  }
   
  ngOnInit() {
    this.credentialService.GetCredentials().subscribe(result => {
      this.credentials = result;
    }, error => console.error(error));
  }
}
