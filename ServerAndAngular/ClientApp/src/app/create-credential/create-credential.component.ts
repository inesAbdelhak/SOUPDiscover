import { Component, OnInit, Inject, Output, EventEmitter } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { CredentialDto } from '../model/credential';
import { HttpErrorResponse } from '@angular/common/http';

export interface DialogData {
  name: string;
  key: string;
}

@Component({
  selector: 'app-create-credential',
  templateUrl: './create-credential.component.html',
  styleUrls: ['./create-credential.component.css']
})
export class CreateCredentialComponent implements OnInit {

  data: CredentialDto;

  @Output() credentialCreated: EventEmitter<CredentialDto> = new EventEmitter<CredentialDto>();

  constructor(public dialog: MatDialog, public credentalService: CredentialService) { }

  /**
   * Open the dialog box to create a new credential
  */
  openDialog(): void {
    this.data = { name: '', key: '' };
    const dialogRef = this.dialog.open(CreateCredentialDialog, {
      data: this.data
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result == null)
        return;
      this.data = result;
      this.credentalService.AddCredential(result)
        .subscribe(_ => this.credentialCreated.emit(result),
          error => this.HandelError(error));
    });
  }

  /**
   * Display error to client
   * @param error the error to display
   */
  HandelError(error: HttpErrorResponse): void {
    window.alert(error.error.detail);
  }

  ngOnInit() {
  }
}

@Component({
  selector: 'create-credential-dialog',
  templateUrl: 'create-credential-dialog.html',
})
export class CreateCredentialDialog  {
  constructor(
    public dialogRef: MatDialogRef<CreateCredentialDialog>,
    @Inject(MAT_DIALOG_DATA) public data: CredentialDto) { }

  onNoClick(): void {
    this.dialogRef.close();
  }
}
