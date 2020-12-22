import { Component, OnInit, Inject, Output, EventEmitter } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { CredentialDto, CredentialType } from '../model/credential';
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

  hide = true;
  selected: { index: number, name: string };
  credentialTypes = Object.keys(CredentialType).filter(e => !isNaN(+e)).map(o => ({ index: +o, name: CredentialType[o] }));

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
    if (this.selected.name == 'ssh')
      this.data.credentialType = CredentialType.ssh;
    else if (this.selected.name == 'password') {
      this.data.credentialType = CredentialType.password;
    }
    this.credentalService.AddCredential(this.data)
      .subscribe(res => {
        this.dialogRef.close(res);
        this.toastr.success('L\'authentification "' + res.name + '" a été créée.', 'Authentification');
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
    this.data = CredentialDto.CreateEmptyCredential();//{ name: '', key: '', login: '', password: '', credentialType: null };
    const dialogRef = this.dialog.open(CreateCredentialDialog, {
      data: this.data,
      disableClose: true,
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
