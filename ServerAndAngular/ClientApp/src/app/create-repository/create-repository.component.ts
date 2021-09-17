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
import { CommonErrorStateMatcher } from '../common/common.errorStateMatcher';

@Component({
  selector: 'create-repository-dialog',
  templateUrl: 'create-repository-dialog.html',
})
export class CreateRepositoryDialog implements OnInit {
  repositoryTypes = Object.keys(RepositoryType).filter(e => !isNaN(+e)).map(o => ({ index: +o, name: RepositoryType[o] }));
  private _selected: { index: number, name: string };
  availableCredentials: CredentialDto[];
  selectedForm: FormControl;
  name: FormControl;
  url: FormControl;
  branch: FormControl;
  credentialId: FormControl;
  private allFieldsGroup: FormGroup;
  private gitFieldsGroup: FormGroup;
  selectedFieldsGroup: FormGroup;
  /* The object that define how manage errors */
  matcher = new CommonErrorStateMatcher();

  constructor(
    private dialogRef: MatDialogRef<CreateRepositoryDialog>,
    @Inject(MAT_DIALOG_DATA) public data: RepositoryDto,
    private toastr: ToastrService,
    private repositoryService: RepositoriesService,
    private credentialService: CredentialService) { }

  public get selected() {
    return this._selected;
  }

  public set selected(newType:  any) {
    this._selected = newType;
    if (!newType) {
      this.selectedFieldsGroup = this.allFieldsGroup;
    }
    switch (newType) {
      case 'git':
        this.selectedFieldsGroup = this.gitFieldsGroup;
        break;
      default:
        this.selectedFieldsGroup = this.allFieldsGroup;
    }
  }

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

    this.selectedForm = new FormControl(this.selected, [
      Validators.required,
    ]);
    this.selectedForm.valueChanges.subscribe(res => this.selected = res);
    this.name = new FormControl(this.data.name, [
      Validators.required]);
    this.name.valueChanges.subscribe(res => this.data.name = res);
    this.url = new FormControl(this.data.url, [
      Validators.required,
      Validators.pattern("^(https?:\/\/(www\.)?|git@)[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}([-a-zA-Z0-9()!@:%_\+.~#?&\/\/=]*)$")]);
    this.url.valueChanges.subscribe(res => this.data.url = res);
    this.branch = new FormControl(this.data.branch, [
      Validators.required]);
    this.branch.valueChanges.subscribe(res => this.data.branch = res);
    this.credentialId = new FormControl(this.data.credentialId, [
      Validators.required]);
    this.credentialId.valueChanges.subscribe(res => {
      console.log('credentialId' + res);
      this.data.credentialId = res;
    });
    this.allFieldsGroup = new FormGroup({
      selected: this.selectedForm,
      name: this.name,
      url: this.url,
      branch: this.branch,
      credentialId: this.credentialId,
    });
    this.gitFieldsGroup = new FormGroup({
      selected: this.selectedForm,
      name: this.name,
      url: this.url,
      branch: this.branch,
      credentialId: this.credentialId,
    });
    this.selectedFieldsGroup = this.allFieldsGroup;
    this.markAsTouched();
  }

  /*
   * Indicate, if all FormControl of the selected FormGroup are valid.
   */
  public get selectedvalid(): boolean {
    return Object.keys(this.selectedFieldsGroup.controls).every(field => {
      const control = this.selectedFieldsGroup.get(field);
      return control.valid;
    });
  }

  /**
 * Check validation of all fields
 * */
  markAsTouched(formGroupToUpdate?: FormGroup): void {
    if (!formGroupToUpdate) {
      formGroupToUpdate = this.allFieldsGroup;
    }
    Object.keys(formGroupToUpdate.controls).forEach(field => {
      const control = formGroupToUpdate.get(field);
      control.markAsTouched({ onlySelf: true });
    });
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
