import { Component, OnInit, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { RepositoryDto, RepositoryType } from '../Model/repository';
import { RepositoriesService } from '../service/repositories.service';

@Component({
  selector: 'app-create-repository',
  templateUrl: './create-repository.component.html',
  styleUrls: ['./create-repository.component.css']
})
export class CreateRepositoryComponent implements OnInit {

  public data: RepositoryDto;

  constructor(public dialog: MatDialog, public credentalService: CredentialService, public repositoryService: RepositoriesService) { }

  openDialog(): void {
    this.data = { repositoryType: RepositoryType.None,  branch: '', sshKeyName: '', tokenName: '', url: '', name: '' };
    const dialogRef = this.dialog.open(CreateRepositoryDialog, {
      //width: '250px',
      //height: '400px',
      data: { data: this.data }
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      this.data = result;
      this.repositoryService.AddRepository(this.data);
    });
  }

  ngOnInit() {
  }
}

@Component({
  selector: 'create-repository-dialog',
  templateUrl: 'create-repository-dialog.html',
})
export class CreateRepositoryDialog implements OnInit {
  repositoryTypes: string[];

  constructor(
    public dialogRef: MatDialogRef<CreateRepositoryDialog>,
    @Inject(MAT_DIALOG_DATA) public data: RepositoryDto) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

  ngOnInit(): void {
    this.repositoryTypes = Object.keys(RepositoryType);
  }

}
