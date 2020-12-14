import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CreateRepositoryComponent } from './create-repository.component';

describe('CreateRepositoryComponent', () => {
  let component: CreateRepositoryComponent;
  let fixture: ComponentFixture<CreateRepositoryComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateRepositoryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateRepositoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
