import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SecretModalComponent } from './secret-modal.component';

describe('SecretModalComponent', () => {
  let component: SecretModalComponent;
  let fixture: ComponentFixture<SecretModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SecretModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SecretModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
