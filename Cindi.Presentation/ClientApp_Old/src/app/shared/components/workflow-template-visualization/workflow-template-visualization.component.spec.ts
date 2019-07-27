import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkflowTemplateVisualizationComponent } from './workflow-template-visualization.component';

describe('WorkflowTemplateVisualizationComponent', () => {
  let component: WorkflowTemplateVisualizationComponent;
  let fixture: ComponentFixture<WorkflowTemplateVisualizationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WorkflowTemplateVisualizationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkflowTemplateVisualizationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
