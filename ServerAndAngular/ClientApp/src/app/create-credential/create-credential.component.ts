import { Component, OnInit, Inject, Output, EventEmitter } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { CredentialDto } from '../model/credential';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';

export interface DialogData {
  name: string;
  key: string;
}

@Component({
  selector: 'create-credential-dialog',
  templateUrl: 'create-credential-dialog.html',
})
export class CreateCredentialDialog {
  constructor(
    public dialogRef: MatDialogRef<CreateCredentialDialog>,
    public credentalService: CredentialService,
    private toastr: ToastrService,
    @Inject(MAT_DIALOG_DATA) public data: CredentialDto) { }

  /**
   * The user cancel editing
   * */
  onNoClick(): void {
    this.dialogRef.close();
  }

  /**
   * Validate modifications
   * */
  OnOkClick(): void {
    this.credentalService.AddCredential(this.data)
      .subscribe(res => {
        this.dialogRef.close(res);
        this.toastr.success('La clé ssh "' + res.name + '" a été créée.', 'Authentification');
      },
        error => this.HandleError(error));
  }

  /**
  * Display validation error to user
  * @param error the error to display
  */
  HandleError(error: HttpErrorResponse) {
    this.toastr.error(error.error.detail, 'Authentification');
  }
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
      if (result == null) {
        return;
      }
      this.data = result;
      this.credentialCreated.emit(result);
    },
      error => console.error(error)
    );
  }

  ngOnInit() {
  }
}
