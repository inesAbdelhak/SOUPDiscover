import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { PackagespaginatorComponent } from './packagespaginator.component';

describe('PackagespaginatorComponent', () => {
  let component: PackagespaginatorComponent;
  let fixture: ComponentFixture<PackagespaginatorComponent>;

  beforeEach(waitForAsync(() => {
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
