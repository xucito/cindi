import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConditionsGroupVisualizerComponent } from './conditions-group-visualizer.component';

describe('ConditionsGroupVisualizerComponent', () => {
  let component: ConditionsGroupVisualizerComponent;
  let fixture: ComponentFixture<ConditionsGroupVisualizerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConditionsGroupVisualizerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConditionsGroupVisualizerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
