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

  ngOnInit(): void {
    this.repositoriesService.GetRepositories().
      subscribe(result => this.repositories = result);
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
        this.toastr.success('Le projet ' + res.name + ' a été créé', 'Projet');
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
      data: this.data
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
