import { createAction, props } from '@ngrx/store';
import { Update } from '@ngrx/entity';

import { StepTemplate } from './step-template.model';


export const loadStepTemplates = createAction(
  '[StepTemplate/API] Load StepTemplates'
);

export const loadStepTemplatesSuccess = createAction(
  '[StepTemplate/API] Load StepTemplates Success', 
  props<{ stepTemplates: StepTemplate[] }>()
);
export const loadStepTemplatesFail = createAction(
  '[StepTemplate/API] Load StepTemplates Fail', 
  props<{ errors: any }>()
); 
export const addStepTemplate = createAction(
  '[StepTemplate/API] Add StepTemplate',
  props<{ stepTemplate: StepTemplate }>()
);

export const upsertStepTemplate = createAction(
  '[StepTemplate/API] Upsert StepTemplate',
  props<{ stepTemplate: StepTemplate }>()
);

export const addStepTemplates = createAction(
  '[StepTemplate/API] Add StepTemplates',
  props<{ stepTemplates: StepTemplate[] }>()
);

export const upsertStepTemplates = createAction(
  '[StepTemplate/API] Upsert StepTemplates',
  props<{ stepTemplates: StepTemplate[] }>()
);

export const updateStepTemplate = createAction(
  '[StepTemplate/API] Update StepTemplate',
  props<{ stepTemplate: Update<StepTemplate> }>()
);

export const updateStepTemplates = createAction(
  '[StepTemplate/API] Update StepTemplates',
  props<{ stepTemplates: Update<StepTemplate>[] }>()
);

export const deleteStepTemplate = createAction(
  '[StepTemplate/API] Delete StepTemplate',
  props<{ id: string }>()
);

export const deleteStepTemplates = createAction(
  '[StepTemplate/API] Delete StepTemplates',
  props<{ ids: string[] }>()
);

export const clearStepTemplates = createAction(
  '[StepTemplate/API] Clear StepTemplates'
);
