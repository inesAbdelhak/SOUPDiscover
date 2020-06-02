import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PackagespaginatorComponent } from './packagespaginator.component';

describe('PackagespaginatorComponent', () => {
  let component: PackagespaginatorComponent;
  let fixture: ComponentFixture<PackagespaginatorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PackagespaginatorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PackagespaginatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
