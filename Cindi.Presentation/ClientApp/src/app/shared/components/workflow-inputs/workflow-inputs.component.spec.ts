import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkflowInputsComponent } from './workflow-inputs.component';

describe('WorkflowInputsComponent', () => {
  let component: WorkflowInputsComponent;
  let fixture: ComponentFixture<WorkflowInputsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WorkflowInputsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkflowInputsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
