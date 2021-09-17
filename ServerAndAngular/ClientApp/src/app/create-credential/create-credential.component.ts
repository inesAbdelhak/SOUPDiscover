import { Component, OnInit, Inject, Output, EventEmitter } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CredentialService } from '../service/credential.service';
import { CredentialDto, CredentialType } from '../model/credential';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormControl, FormGroup, FormGroupDirective, NgForm, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { CommonErrorStateMatcher } from '../common/common.errorStateMatcher';

export interface DialogData {
  name: string;
  key: string;
}

@Component({
  selector: 'create-credential-dialog',
  templateUrl: 'create-credential-dialog.html',
})
export class CreateCredentialDialog implements OnInit {
  constructor(
    public dialogRef: MatDialogRef<CreateCredentialDialog>,
    public credentalService: CredentialService,
    private toastr: ToastrService,
    @Inject(MAT_DIALOG_DATA) public data: CredentialDto) { }

  private allFieldsForm: FormGroup;

  /* Fields to validate when submit ssh credential */
  private sshForms: FormGroup;

  /* Fields to validate when submit login/password credential */
  private passwordForms: FormGroup;

  /* Fields to validate when submit a token credential */
  private tokenForms: FormGroup;
  selectedFormGroup: FormGroup;

  /* To hide or not the password when typing it */
  hide = true;

  /* The selected credential type selected */
  private _selected: { index: number, name: string };

  /* All available credential types */
  credentialTypes = Object.keys(CredentialType).filter(e => !isNaN(+e)).map(o => ({ index: +o, name: CredentialType[o] }));

  /* The FormControl to typing the name */
  name: AbstractControl;

  /* The FormControl to typing the ssh key */
  key: AbstractControl;

  /* The FormControl to typing the login */
  login: AbstractControl;

  /* The FormControl to typing the password */
  password: AbstractControl;

  /* The FormControl to typing the token */
  token: AbstractControl;

  credentialType: AbstractControl;

  /* The object that define how manage errors */
  matcher = new CommonErrorStateMatcher();

  get selected(): any {
    return this._selected;
  }

  /**
   * Setter of selected credential type
   * When setting it, the list of FormControl to check change.
   */
  set selected(newType: any) {
    this._selected = newType;
    if (!newType) {
      this.selectedFormGroup = this.allFieldsForm;
      console.log("switch to allFieldsForm group");
      return;
    }
    /* Switch to the FormGroup to validate*/
    switch (newType.name) {
      case 'ssh':
        this.selectedFormGroup = this.sshForms;
        console.log("switch to sshForms group");
        break;

      case 'password':
        this.selectedFormGroup = this.passwordForms;
        console.log("switch to passwordForms group");
        break;

      case 'token':
        this.selectedFormGroup = this.tokenForms;
        console.log("switch to tokenForms group");
        break;

      default:
        this.selectedFormGroup = this.allFieldsForm;
        console.log("switch to allFieldsForm group");
    }
  }

  /**
   * The user cancel editing
   * */
  onNoClick(): void {
    this.dialogRef.close();
  }

  /**
   * Create FormControl
   */
  ngOnInit(): void {
    this.name = new FormControl(this.data.name, [
      Validators.required,
      Validators.minLength(3),
    ]);
    this.name.valueChanges.subscribe(res => this.data.name = res);
    this.login = new FormControl(this.data.login, [
      Validators.required,
      Validators.minLength(3),
    ]);
    this.login.valueChanges.subscribe(res => this.data.login = res);
    this.password = new FormControl(this.data.password, [
      Validators.required,
    ]);
    this.password.valueChanges.subscribe(res => this.data.password = res);
    this.token = new FormControl(this.data.token, [
      Validators.required
    ]);
    this.token.valueChanges.subscribe(res => this.data.token = res);
    this.key = new FormControl(this.data.key, [
      Validators.required,
    ]);
    this.key.valueChanges.subscribe(res => this.data.key = res);

    this.credentialType = new FormControl(this.selected, [
      Validators.required,
    ]);
    this.credentialType.valueChanges.subscribe(res => this.selected = res);
    this.allFieldsForm = new FormGroup({
      name: this.name,
      key: this.key,
      login: this.login,
      password: this.password,
      token: this.token,
    });
    this.selectedFormGroup = this.allFieldsForm;
    this.sshForms = new FormGroup({
      name: this.name,
      key: this.key,
    });
    this.passwordForms = new FormGroup({
      name: this.name,
      login: this.login,
      password: this.password,
    });
    this.tokenForms = new FormGroup({
      name: this.name,
      token: this.token,
    });
    this.markAsTouched();
  }

  /**
   * Check validation of all fields
   * */
  markAsTouched(formGroupToUpdate?: FormGroup): void {
    if (!formGroupToUpdate) {
      formGroupToUpdate = this.allFieldsForm;
    }
    Object.keys(formGroupToUpdate.controls).forEach(field => {
      const control = formGroupToUpdate.get(field);
      control.markAsTouched({ onlySelf: true });
    });
  }

  /**
   * Validate modifications
   * */
  OnOkClick(): void {
    if (!this.selected) {
      throw 'Credential type is not selected!';
    }
    switch (this.selected.name) {
      case 'ssh':
        this.data.credentialType = CredentialType.ssh;
        break;
      case 'password':
        this.data.credentialType = CredentialType.password;
        break;
      case 'token':
        this.data.credentialType = CredentialType.token;
        break;
      default:
        throw 'The credential type ' + this.selected.name + ' is not supported';
    }
    this.credentalService.AddCredential(this.data)
      .subscribe(res => {
        this.dialogRef.close(res);
        this.toastr.success('L\'authentification "' + res.name + '" a été créée.', 'Authentification');
      },
        error => this.HandleError(error));
  }

  /*
   * Indicate, if all FormControl of the selected FormGroup are valid.
   */
  public get selectedvalid(): boolean {
    return Object.keys(this.selectedFormGroup.controls).every(field => {
      const control = this.selectedFormGroup.get(field);
      return control.valid;
    });
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
