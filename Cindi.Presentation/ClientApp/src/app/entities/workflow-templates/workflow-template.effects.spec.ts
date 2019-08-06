import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable } from 'rxjs';

import { WorkflowTemplateEffects } from './workflow-template.effects';

describe('WorkflowTemplateEffects', () => {
  let actions$: Observable<any>;
  let effects: WorkflowTemplateEffects;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        WorkflowTemplateEffects,
        provideMockActions(() => actions$)
      ]
    });

    effects = TestBed.get<WorkflowTemplateEffects>(WorkflowTemplateEffects);
  });

  it('should be created', () => {
    expect(effects).toBeTruthy();
  });
});
