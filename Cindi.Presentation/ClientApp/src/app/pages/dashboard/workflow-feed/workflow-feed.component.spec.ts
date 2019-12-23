import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkflowFeedComponent } from './workflow-feed.component';

describe('WorkflowFeedComponent', () => {
  let component: WorkflowFeedComponent;
  let fixture: ComponentFixture<WorkflowFeedComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WorkflowFeedComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkflowFeedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
