import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LogicBlockVisualizerComponent } from './logic-block-visualizer.component';

describe('LogicBlockVisualizerComponent', () => {
  let component: LogicBlockVisualizerComponent;
  let fixture: ComponentFixture<LogicBlockVisualizerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LogicBlockVisualizerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LogicBlockVisualizerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
