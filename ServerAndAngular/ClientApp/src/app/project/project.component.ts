import { Component, OnInit } from '@angular/core';
import { ProjectDto } from '../Model/Project';
import { ProjectService } from '../service/project.service';

@Component({
  selector: 'app-project',
  templateUrl: './project.component.html',
  styleUrls: ['./project.component.css']
})
export class ProjectComponent implements OnInit {

  projects: ProjectDto[];
  constructor(public projectService : ProjectService) { }

  ngOnInit() {
    this.projectService.GetProjects()
      .subscribe(result => this.projects = result);
  }

}
