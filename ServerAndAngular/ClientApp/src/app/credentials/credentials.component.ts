import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CredentialService } from '../service/credential.service';
import { CredentialDto } from '../Model/credential';

@Component({
  selector: 'app-credentials',
  templateUrl: './credentials.component.html',
  styleUrls: ['./credentials.component.css']
})
export class CredentialsComponent implements OnInit {
  public credentials: CredentialDto[];
  constructor(private credentialService: CredentialService) {
  }
   
  ngOnInit() {
    this.credentialService.GetCredentials().subscribe(result => {
      this.credentials = result;
    }, error => console.error(error));
  }
}
