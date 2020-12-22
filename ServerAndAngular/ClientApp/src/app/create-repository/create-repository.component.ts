import { Component, OnInit, Inject, Pipe, PipeTransform, Output, EventEmitter } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { RepositoryDto } from '../model/repository';
import { RepositoryType } from '../model/repositoryType';
import { RepositoriesService } from '../service/repositories.service';
import { CredentialDto } from '../model/credential';
import { FormControl, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'create-repository-dialog',
  templateUrl: 'create-repository-dialog.html',
})
export class CreateRepositoryDialog implements OnInit {
  repositoryTypes = Object.keys(RepositoryType).filter(e => !isNaN(+e)).map(o => ({ index: +o, name: RepositoryType[o] }));
  selected: { index: number, name: string };
  availableCredentials: CredentialDto[];
  registredForms: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<CreateRepositoryDialog>,
    @Inject(MAT_DIALOG_DATA) public data: RepositoryDto,
    private toastr: ToastrService,
    private repositoryService: RepositoriesService,
    private credentialService: CredentialService) { }

  /**
   * The user click to cancel
   * */
  public onNoClick(): void {
    this.dialogRef.close();
  }

  /**
   *The user click on OK
   * */
  public onOkClick(): void {
    this.data.repositoryType = this.selected.index;
    this.repositoryService.AddRepository(this.data)
      .subscribe(res => {
        this.dialogRef.close(res);
        this.toastr.success('Le dépot "' + res.name + '" a été créé', 'Dépot');
      },
        error => this.HandleError(error));
  }

  public ngOnInit(): void {
    // Retrieve all available credentials
    this.credentialService.GetCredentials()
      .subscribe(res => this.availableCredentials = res
        , error => console.error(error));
  }

  /**
  * Display validation error to user
  * @param error the error to display
  */
  HandleError(error: HttpErrorResponse) {
    let errorTitle = "";
    if (error.error.detail != undefined) {
      errorTitle = error.error.detail;
    }
    else {
      errorTitle = error.error.title;
    }
    this.toastr.error(errorTitle, 'Dépot');
  }
}

@Component({
  selector: 'app-create-repository',
  templateUrl: './create-repository.component.html',
  styleUrls: ['./create-repository.component.css']
})
export class CreateRepositoryComponent implements OnInit {

  data: RepositoryDto;
  @Output() repositoryCreated: EventEmitter<RepositoryDto> = new EventEmitter<RepositoryDto>();

  constructor(public dialog: MatDialog) { }

  openDialog(): void {
    this.data = new RepositoryDto();
    const dialogRef = this.dialog.open(CreateRepositoryDialog, {
      data: this.data,
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result !== undefined) {
        this.data = result;
        this.repositoryCreated.emit(this.data);
      }
    });
  }

  ngOnInit() {
  }
}
