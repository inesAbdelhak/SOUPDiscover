import { Component, OnInit, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';

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

  name: string;
  key: string;

  constructor(public dialog: MatDialog, public credentalService: CredentialService) { }

  openDialog(): void {
    const dialogRef = this.dialog.open(DialogOverviewExampleDialog, {
      //width: '250px',
      //height: '400px',
      data: { name: '', key: ''}
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      this.name = result.name; 
      this.key = result.key;
      this.credentalService.AddCredential(result);
    });
  }

  ngOnInit() {
  }
}

@Component({
  selector: 'dialog-overview-example-dialog',
  templateUrl: 'dialog-overview-example-dialog.html',
})
export class DialogOverviewExampleDialog {

  constructor(
    public dialogRef: MatDialogRef<DialogOverviewExampleDialog>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

}
