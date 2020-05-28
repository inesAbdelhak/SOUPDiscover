import { Component, OnInit } from '@angular/core';
import { ProjectDto } from '../model/project';
import { ProjectService } from '../service/project.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-project',
  templateUrl: './project.component.html',
  styleUrls: ['./project.component.css']
})
export class ProjectComponent implements OnInit {

  /**
   * List of projects displayed
   */
  projects: ProjectDto[];

  constructor(public projectService: ProjectService) { }

  /**
   * To update project list, when a project is added or deleted
   */
  projectListUpdate = function (project: ProjectDto): void {
    this.RefreshProjects();
  }
  /**
   * Launch analysis on the project
   * @param project
   */
  LaunchAnalyse(projectName: string): void {
    this.projectService.LaunchProject(projectName)
      .subscribe(_ => { },
        error => this.HandelError(error));
  }

  /**
   * Request list of projects to update the client side
   * */
  RefreshProjects() {
    this.projectService.GetProjects()
      .subscribe(result => this.projects = result,
        error => console.error(error));
  }

  /**
  * Display error to client
  * @param error the error to display
  */
  HandelError(error: HttpErrorResponse): void {
    window.alert(error.error.detail);
  }

  ngOnInit() {
    this.projectService.GetProjects()
      .subscribe(result => this.projects = result
        , error => console.error(error));
  }
}
