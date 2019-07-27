import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkflowVisualizationComponent } from './workflow-visualization.component';

describe('WorkflowVisualizationComponent', () => {
  let component: WorkflowVisualizationComponent;
  let fixture: ComponentFixture<WorkflowVisualizationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WorkflowVisualizationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkflowVisualizationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
