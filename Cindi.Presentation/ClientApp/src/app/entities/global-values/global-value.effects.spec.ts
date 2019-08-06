import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable } from 'rxjs';

import { GlobalValueEffects } from './global-value.effects';

describe('GlobalValueEffects', () => {
  let actions$: Observable<any>;
  let effects: GlobalValueEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        GlobalValueEffects,
        provideMockActions(() => actions$)
      ]
    });

    effects = TestBed.get<GlobalValueEffects>(GlobalValueEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
