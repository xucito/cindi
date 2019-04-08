import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SequenceTemplateComponent } from './sequence-template.component';

describe('SequenceTemplateComponent', () => {
  let component: SequenceTemplateComponent;
  let fixture: ComponentFixture<SequenceTemplateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SequenceTemplateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SequenceTemplateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
