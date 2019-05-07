import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AddGlobalValueModalComponent } from './add-global-value-modal.component';

describe('AddGlobalValueModalComponent', () => {
  let component: AddGlobalValueModalComponent;
  let fixture: ComponentFixture<AddGlobalValueModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AddGlobalValueModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddGlobalValueModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
