import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CredentialService } from '../service/credential.service';
import { CredentialDto } from '../model/credential';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-credentials',
  templateUrl: './credentials.component.html',
  styleUrls: ['./credentials.component.css']
})
export class CredentialsComponent implements OnInit {

  public credentials: CredentialDto[];

  constructor(private credentialService: CredentialService,
    private toastr: ToastrService) {
  }

  /**
  * To update credential list, when a credential is added or deleted
  */
  credentialListUpdate = function (credential: CredentialDto): void {
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
        this.credentials.splice(index);// Remove the credential of the credential list
        this.toastr.success('La clée ssh ' + credentialDto.name + ' a été supprimée');
      },
        error => this.HandleError(error));
  }

  HandleError(error: HttpErrorResponse): void {
    this.toastr.error(error.error.detail, "Authentification");
  }
   
  ngOnInit() {
    this.credentialService.GetCredentials().subscribe(result => {
      this.credentials = result;
    }, error => console.error(error));
  }
}
