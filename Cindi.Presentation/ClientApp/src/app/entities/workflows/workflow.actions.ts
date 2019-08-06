import { createAction, props } from '@ngrx/store';
import { Update } from '@ngrx/entity';

import { Workflow } from './workflow.model';

export const loadWorkflows = createAction(
  "[Workflow/API] Load Workflows",
  props<{ status: string }>()
);

export const loadWorkflowsSuccess = createAction(
  "[Workflow/API] Load Workflows Success",
  props<{ workflows: Workflow[] }>()
);

export const loadWorkflowsFail = createAction(
  "[Workflow/API] Load Workflows Fail",
  props<{ errors: any }>()
);

export const addWorkflow = createAction(
  '[Workflow/API] Add Workflow',
  props<{ workflow: Workflow }>()
);

export const upsertWorkflow = createAction(
  '[Workflow/API] Upsert Workflow',
  props<{ workflow: Workflow }>()
);

export const addWorkflows = createAction(
  '[Workflow/API] Add Workflows',
  props<{ workflows: Workflow[] }>()
);

export const upsertWorkflows = createAction(
  '[Workflow/API] Upsert Workflows',
  props<{ workflows: Workflow[] }>()
);

export const updateWorkflow = createAction(
  '[Workflow/API] Update Workflow',
  props<{ workflow: Update<Workflow> }>()
);

export const updateWorkflows = createAction(
  '[Workflow/API] Update Workflows',
  props<{ workflows: Update<Workflow>[] }>()
);

export const deleteWorkflow = createAction(
  '[Workflow/API] Delete Workflow',
  props<{ id: string }>()
);

export const deleteWorkflows = createAction(
  '[Workflow/API] Delete Workflows',
  props<{ ids: string[] }>()
);

export const clearWorkflows = createAction(
  '[Workflow/API] Clear Workflows'
);
