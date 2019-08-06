import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StepTimeLineComponent } from './step-time-line.component';

describe('StepTimeLineComponent', () => {
  let component: StepTimeLineComponent;
  let fixture: ComponentFixture<StepTimeLineComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StepTimeLineComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StepTimeLineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
