import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StepProgressBarComponent } from './step-progress-bar.component';

describe('StepProgressBarComponent', () => {
  let component: StepProgressBarComponent;
  let fixture: ComponentFixture<StepProgressBarComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StepProgressBarComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StepProgressBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
