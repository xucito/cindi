import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkflowDesignerComponent } from './workflow-designer.component';

describe('WorkflowDesignerComponent', () => {
  let component: WorkflowDesignerComponent;
  let fixture: ComponentFixture<WorkflowDesignerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WorkflowDesignerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkflowDesignerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
