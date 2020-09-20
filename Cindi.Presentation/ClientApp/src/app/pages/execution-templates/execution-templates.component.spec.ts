import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ExecutionTemplatesComponent } from './execution-templates.component';

describe('ExecutionTemplatesComponent', () => {
  let component: ExecutionTemplatesComponent;
  let fixture: ComponentFixture<ExecutionTemplatesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ExecutionTemplatesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExecutionTemplatesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
