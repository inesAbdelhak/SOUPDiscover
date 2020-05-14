import { Component, OnInit, Inject, Pipe, PipeTransform } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { RepositoryDto, RepositoryType } from '../Model/repository';
import { RepositoriesService } from '../service/repositories.service';
import { CredentialDto } from '../Model/credential';

@Component({
  selector: 'app-create-repository',
  templateUrl: './create-repository.component.html',
  styleUrls: ['./create-repository.component.css']
})
export class CreateRepositoryComponent implements OnInit {

  public data: RepositoryDto;

  constructor(public dialog: MatDialog,
    public credentialService: CredentialService,
    public repositoryService: RepositoriesService) { }

  openDialog(): void {
    this.data = {};
    const dialogRef = this.dialog.open(CreateRepositoryDialog, {
      //width: '250px',
      //height: '400px',
      data: this.data
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      // result.repositoryType = RepositoryType.Git;
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
  repositoryTypes = Object.keys(RepositoryType).filter(e => !isNaN(+e)).map(o => { return { index: +o, name: RepositoryType[o] } });
  selected: { index: number, name: string };

  availableCredentials: CredentialDto[];
  constructor(
    public dialogRef: MatDialogRef<CreateRepositoryDialog>,
    @Inject(MAT_DIALOG_DATA) public data: RepositoryDto, public credentialService : CredentialService) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

  onOkClick(): void {
    this.data.repositoryType = this.selected.index;
  }

  ngOnInit(): void {
    // Retrieve all available credentials
    this.credentialService.GetCredentials()
      .subscribe(res => this.availableCredentials = res);
  }

}
