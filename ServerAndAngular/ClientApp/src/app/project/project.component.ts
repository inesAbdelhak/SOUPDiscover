import { Component, OnInit } from '@angular/core';
import { ProjectDto } from '../model/project';
import { ProjectService } from '../service/project.service';

@Component({
  selector: 'app-project',
  templateUrl: './project.component.html',
  styleUrls: ['./project.component.css']
})
export class ProjectComponent implements OnInit {

  projects: ProjectDto[];

  /**
   * To update project list, when a project is added or deleted
   */
  projectListUpdate = function (project: ProjectDto): void {
    this.projectService.GetProjects()
      .subscribe(result => this.projects = result);
  }

  constructor(public projectService: ProjectService) { }
  /**
   * Launch analysis on the project
   * @param project
   */
  LaunchAnalyse(projectName: string): void {
    this.projectService.LaunchProject(projectName);
  }

  ngOnInit() {
    this.projectService.GetProjects()
      .subscribe(result => this.projects = result);
  }

}
