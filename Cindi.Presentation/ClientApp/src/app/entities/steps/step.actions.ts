import { createAction, props } from "@ngrx/store";
import { Update } from "@ngrx/entity";

import { Step } from "./step.model";

export const loadSteps = createAction(
  "[Step/API] Load Steps",
  props<{ status: string }>()
);

export const loadStepsSuccess = createAction(
  "[Step/API] Load Steps Success",
  props<{ steps: Step[] }>()
);

export const loadStepsFail = createAction(
  "[Step/API] Load Steps Fail",
  props<{ errors: any }>()
);

export const addStep = createAction(
  "[Step/API] Add Step",
  props<{ step: Step }>()
);

export const upsertStep = createAction(
  "[Step/API] Upsert Step",
  props<{ step: Step }>()
);

export const addSteps = createAction(
  "[Step/API] Add Steps",
  props<{ steps: Step[] }>()
);

export const upsertSteps = createAction(
  "[Step/API] Upsert Steps",
  props<{ steps: Step[] }>()
);

export const updateStep = createAction(
  "[Step/API] Update Step",
  props<{ step: Update<Step> }>()
);

export const updateSteps = createAction(
  "[Step/API] Update Steps",
  props<{ steps: Update<Step>[] }>()
);

export const deleteStep = createAction(
  "[Step/API] Delete Step",
  props<{ id: string }>()
);

export const deleteSteps = createAction(
  "[Step/API] Delete Steps",
  props<{ ids: string[] }>()
);

export const clearSteps = createAction("[Step/API] Clear Steps");
