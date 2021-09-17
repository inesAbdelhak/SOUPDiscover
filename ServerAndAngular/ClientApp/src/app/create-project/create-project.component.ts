import { Component, OnInit, Inject, EventEmitter, Output } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ProjectService } from '../service/project.service';
import { ProjectDto } from '../model/project';
import { RepositoryDto } from '../model/repository';
import { RepositoryType } from '../model/repositoryType';
import { RepositoriesService } from '../service/repositories.service';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { ProcessStatus } from '../model/processStatus';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { CommonErrorStateMatcher } from '../common/common.errorStateMatcher';

@Component({
  selector: 'create-project-dialog',
  templateUrl: 'create-project-dialog.html',
})
export class CreateProjectDialog implements OnInit {
  repositories: RepositoryDto[];
  constructor(
    public dialogRef: MatDialogRef<CreateProjectDialog>,
    public repositoriesService: RepositoriesService,
    public projectService: ProjectService,
    private toastr: ToastrService,
    @Inject(MAT_DIALOG_DATA) public data: ProjectDto) { }

  /* The FormControl to typing the name of the repository */
  name: AbstractControl;
  repositoryId: AbstractControl;
  commandLinesBeforeParse: AbstractControl;
  nugetServerUrl: AbstractControl;
  private allFieldsForm: FormGroup;
  /* The object that define how manage errors */
  matcher = new CommonErrorStateMatcher();

  ngOnInit(): void {
    this.repositoriesService.GetRepositories().
      subscribe(result => this.repositories = result);

    this.name = new FormControl(this.data.name, [
      Validators.required]);
    this.name.valueChanges.subscribe(res => this.data.name = res);
    this.repositoryId = new FormControl(this.data.repositoryId, [
      Validators.required]);
    this.repositoryId.valueChanges.subscribe(res => this.data.repositoryId = res);
    this.commandLinesBeforeParse = new FormControl(this.data.commandLinesBeforeParse, [
      Validators.required]);
    this.commandLinesBeforeParse.valueChanges.subscribe(res => this.data.commandLinesBeforeParse = res);
    this.nugetServerUrl = new FormControl(this.data.nugetServerUrl, [
      Validators.required]);
    this.nugetServerUrl.valueChanges.subscribe(res => this.data.nugetServerUrl = res);
    this.allFieldsForm = new FormGroup({
      name: this.name,
      repositoryId: this.repositoryId,
      commandLinesBeforeParse: this.commandLinesBeforeParse,
      nugetServerUrl: this.nugetServerUrl,
    });
    this.markAsTouched();
  }

  /**
   * The user cancel the editing
   * */
  onNoClick(): void {
    this.dialogRef.close();
  }

  /**
   * The user wants to save data
   * */
  onOkClick(): void {
    this.projectService.AddProject(this.data)
      .subscribe(res => {
        this.dialogRef.close(res);
        this.toastr.success('Le projet "' + res.name + '" a été créé', 'Projet');
      },
        error => this.HandleError(error));
  }

  /**
  * Display validation error to user
  * @param error the error to display
  */
  HandleError(error: HttpErrorResponse) {
    this.toastr.error(error.error.detail, 'Projet');
  }

  markAsTouched(formGroupToUpdate?: FormGroup): void {
    if (!formGroupToUpdate) {
      formGroupToUpdate = this.allFieldsForm;
    }
    Object.keys(formGroupToUpdate.controls).forEach(field => {
      const control = formGroupToUpdate.get(field);
      control.markAsTouched({ onlySelf: true });
    });
  }

  /*
 * Indicate, if all FormControl of the selected FormGroup are valid.
 */
  public get selectedvalid(): boolean {
    return Object.keys(this.allFieldsForm.controls).every(field => {
      const control = this.allFieldsForm.get(field);
      return control.valid;
    });
  }
}

@Component({
  selector: 'app-create-project',
  templateUrl: './create-project.component.html',
  styleUrls: ['./create-project.component.css']
})
export class CreateProjectComponent implements OnInit {

  data: ProjectDto;
  @Output() projectCreated: EventEmitter<ProjectDto> = new EventEmitter<ProjectDto>();
  constructor(public dialog: MatDialog) { }

  openDialog(): void {
    this.data = { name: '', commandLinesBeforeParse: '', repositoryId: '', nugetServerUrl: '' };
    const dialogRef = this.dialog.open(CreateProjectDialog, {
      data: this.data,
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      if (result !== undefined) {
        this.data = result;
        this.projectCreated.emit(result);
      }
    });
  }

  ngOnInit() {
  }
}
