import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MappingSelectorComponent } from './mapping-selector.component';

describe('MappingSelectorComponent', () => {
  let component: MappingSelectorComponent;
  let fixture: ComponentFixture<MappingSelectorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MappingSelectorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MappingSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
