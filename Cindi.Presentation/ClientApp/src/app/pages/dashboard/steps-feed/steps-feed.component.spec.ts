import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StepsFeedComponent } from './steps-feed.component';

describe('StepsFeedComponent', () => {
  let component: StepsFeedComponent;
  let fixture: ComponentFixture<StepsFeedComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StepsFeedComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StepsFeedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
