import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ExecutionSchedulesComponent } from './execution-schedules.component';

describe('ExecutionSchedulesComponent', () => {
  let component: ExecutionSchedulesComponent;
  let fixture: ComponentFixture<ExecutionSchedulesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ExecutionSchedulesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExecutionSchedulesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
