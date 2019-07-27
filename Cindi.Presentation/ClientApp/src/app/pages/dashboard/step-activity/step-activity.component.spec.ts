import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StepActivityComponent } from './step-activity.component';

describe('StepActivityComponent', () => {
  let component: StepActivityComponent;
  let fixture: ComponentFixture<StepActivityComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StepActivityComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StepActivityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
