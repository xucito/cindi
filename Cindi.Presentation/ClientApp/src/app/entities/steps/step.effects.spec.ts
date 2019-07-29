import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable } from 'rxjs';

import { StepEffects } from './step.effects';

describe('StepEffects', () => {
  let actions$: Observable<any>;
  let effects: StepEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        StepEffects,
        provideMockActions(() => actions$)
      ]
    });

    effects = TestBed.get<StepEffects>(StepEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
