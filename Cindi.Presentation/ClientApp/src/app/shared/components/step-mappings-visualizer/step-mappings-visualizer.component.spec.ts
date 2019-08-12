import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StepMappingsVisualizerComponent } from './step-mappings-visualizer.component';

describe('StepMappingsVisualizerComponent', () => {
  let component: StepMappingsVisualizerComponent;
  let fixture: ComponentFixture<StepMappingsVisualizerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StepMappingsVisualizerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StepMappingsVisualizerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
