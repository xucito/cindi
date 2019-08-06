import { createAction, props } from '@ngrx/store';
import { Update } from '@ngrx/entity';

import { WorkflowTemplate } from './workflow-template.model';

export const loadWorkflowTemplates = createAction(
  "[WorkflowTemplate/API] Load WorkflowTemplates"
);

export const loadWorkflowTemplatesSuccess = createAction(
  "[WorkflowTemplate/API] Load WorkflowTemplates Success",
  props<{ workflowTemplates: WorkflowTemplate[] }>()
);

export const loadWorkflowTemplatesFail = createAction(
  "[WorkflowTemplate/API] Load WorkflowTemplates Fail",
  props<{ errors: any }>()
);

export const addWorkflowTemplate = createAction(
  '[WorkflowTemplate/API] Add WorkflowTemplate',
  props<{ workflowTemplate: WorkflowTemplate }>()
);

export const upsertWorkflowTemplate = createAction(
  '[WorkflowTemplate/API] Upsert WorkflowTemplate',
  props<{ workflowTemplate: WorkflowTemplate }>()
);

export const addWorkflowTemplates = createAction(
  '[WorkflowTemplate/API] Add WorkflowTemplates',
  props<{ workflowTemplates: WorkflowTemplate[] }>()
);

export const upsertWorkflowTemplates = createAction(
  '[WorkflowTemplate/API] Upsert WorkflowTemplates',
  props<{ workflowTemplates: WorkflowTemplate[] }>()
);

export const updateWorkflowTemplate = createAction(
  '[WorkflowTemplate/API] Update WorkflowTemplate',
  props<{ workflowTemplate: Update<WorkflowTemplate> }>()
);

export const updateWorkflowTemplates = createAction(
  '[WorkflowTemplate/API] Update WorkflowTemplates',
  props<{ workflowTemplates: Update<WorkflowTemplate>[] }>()
);

export const deleteWorkflowTemplate = createAction(
  '[WorkflowTemplate/API] Delete WorkflowTemplate',
  props<{ id: string }>()
);

export const deleteWorkflowTemplates = createAction(
  '[WorkflowTemplate/API] Delete WorkflowTemplates',
  props<{ ids: string[] }>()
);

export const clearWorkflowTemplates = createAction(
  '[WorkflowTemplate/API] Clear WorkflowTemplates'
);
