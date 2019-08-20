import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectStepTemplateComponent } from './select-step-template.component';

describe('SelectStepTemplateComponent', () => {
  let component: SelectStepTemplateComponent;
  let fixture: ComponentFixture<SelectStepTemplateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SelectStepTemplateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SelectStepTemplateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
