import { Component, OnInit, Inject, Pipe, PipeTransform, Output, EventEmitter } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { RepositoryDto, RepositoryType } from '../model/repository';
import { RepositoriesService } from '../service/repositories.service';
import { CredentialDto } from '../model/credential';
import { FormControl, FormBuilder, Validators, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-create-repository',
  templateUrl: './create-repository.component.html',
  styleUrls: ['./create-repository.component.css']
})
export class CreateRepositoryComponent implements OnInit {

  data: RepositoryDto;
  @Output() repositoryCreated: EventEmitter<RepositoryDto> = new EventEmitter<RepositoryDto>();

  constructor(public dialog: MatDialog,
    private credentialService: CredentialService,
    private repositoryService: RepositoriesService) { }

  openDialog(): void {
    this.data = {};
    const dialogRef = this.dialog.open(CreateRepositoryDialog, {
      // width: '250px',
      // height: '400px',
      data: this.data
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      // result.repositoryType = RepositoryType.Git;
      this.data = result;
      this.repositoryService.AddRepository(this.data)
        .subscribe(res => this.repositoryCreated.emit(res));
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
  registredForms: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<CreateRepositoryDialog>,
    @Inject(MAT_DIALOG_DATA) public data: RepositoryDto,
    private credentialService: CredentialService,
    private repositoryService: RepositoriesService) { }

  public onNoClick(): void {
    this.dialogRef.close();
  }

  public onOkClick(): void {
    this.data.repositoryType = this.selected.index;
    this.repositoryService.AddRepository(this.data)
      .subscribe(res => this.dialogRef.close());
  }

  public ngOnInit(): void {
    //this.registredForms = this.formBuilder.group({
    //  url: ['', Validators.required],
    //});
    // Retrieve all available credentials
    this.credentialService.GetCredentials()
      .subscribe(res => this.availableCredentials = res);
  }
}
