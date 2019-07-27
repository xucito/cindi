import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkflowTemplatesComponent } from './workflow-templates.component';

describe('WorkflowTemplatesComponent', () => {
  let component: WorkflowTemplatesComponent;
  let fixture: ComponentFixture<WorkflowTemplatesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WorkflowTemplatesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkflowTemplatesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
