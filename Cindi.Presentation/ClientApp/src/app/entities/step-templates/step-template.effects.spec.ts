import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable } from 'rxjs';

import { StepTemplateEffects } from './step-template.effects';

describe('StepTemplateEffects', () => {
  let actions$: Observable<any>;
  let effects: StepTemplateEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        StepTemplateEffects,
        provideMockActions(() => actions$)
      ]
    });

    effects = TestBed.get<StepTemplateEffects>(StepTemplateEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
