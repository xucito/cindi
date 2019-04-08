import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SequenceTemplatesComponent } from './sequence-templates.component';

describe('SequenceTemplatesComponent', () => {
  let component: SequenceTemplatesComponent;
  let fixture: ComponentFixture<SequenceTemplatesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SequenceTemplatesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SequenceTemplatesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
