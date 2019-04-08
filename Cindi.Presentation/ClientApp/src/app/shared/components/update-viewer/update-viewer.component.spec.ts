import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateViewerComponent } from './update-viewer.component';

describe('UpdateViewerComponent', () => {
  let component: UpdateViewerComponent;
  let fixture: ComponentFixture<UpdateViewerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UpdateViewerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UpdateViewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
