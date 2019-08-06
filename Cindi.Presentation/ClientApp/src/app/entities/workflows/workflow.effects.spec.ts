import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable } from 'rxjs';

import { WorkflowEffects } from './workflow.effects';

describe('WorkflowEffects', () => {
  let actions$: Observable<any>;
  let effects: WorkflowEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        WorkflowEffects,
        provideMockActions(() => actions$)
      ]
    });

    effects = TestBed.get<WorkflowEffects>(WorkflowEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
