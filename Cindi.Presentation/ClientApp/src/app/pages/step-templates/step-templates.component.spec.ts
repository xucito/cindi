import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StepTemplatesComponent } from './step-templates.component';

describe('StepTemplatesComponent', () => {
  let component: StepTemplatesComponent;
  let fixture: ComponentFixture<StepTemplatesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StepTemplatesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StepTemplatesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
