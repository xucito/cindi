import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConditionSelectorComponent } from './condition-selector.component';

describe('ConditionSelectorComponent', () => {
  let component: ConditionSelectorComponent;
  let fixture: ComponentFixture<ConditionSelectorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConditionSelectorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConditionSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
