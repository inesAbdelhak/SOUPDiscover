import { Component } from '@angular/core';
import { name, version } from '../../package.json';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  public version: string = version;
  public title: string = name;
}
