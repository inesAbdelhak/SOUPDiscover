import { Component, OnInit, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { CredentialDto } from '../model/credential';

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

  constructor(public dialog: MatDialog, public credentalService: CredentialService) { }

  openDialog(): void {
    this.data = { name: '', key: '' };
    const dialogRef = this.dialog.open(CreateCredentialDialog, {
      //width: '250px',
      //height: '400px',
      data: this.data
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      this.data = result;
      this.credentalService.AddCredential(result);
    });
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
