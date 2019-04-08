import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SequenceTemplateVisualizationComponent } from './sequence-template-visualization.component';

describe('SequenceTemplateVisualizationComponent', () => {
  let component: SequenceTemplateVisualizationComponent;
  let fixture: ComponentFixture<SequenceTemplateVisualizationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SequenceTemplateVisualizationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SequenceTemplateVisualizationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
