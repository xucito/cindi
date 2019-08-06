import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StepTemplateViewComponent } from './step-template-view.component';

describe('StepTemplateViewComponent', () => {
  let component: StepTemplateViewComponent;
  let fixture: ComponentFixture<StepTemplateViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StepTemplateViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StepTemplateViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
