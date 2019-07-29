import { Action, createReducer, on } from "@ngrx/store";
import { EntityState, EntityAdapter, createEntityAdapter } from "@ngrx/entity";
import { StepTemplate } from "./step-template.model";
import * as StepTemplateActions from "./step-template.actions";

export interface State extends EntityState<StepTemplate> {
  // additional entities state properties
}

export const adapter: EntityAdapter<StepTemplate> = createEntityAdapter<
  StepTemplate
>();

export const initialState: State = adapter.getInitialState({
  // additional entity state properties
});

const stepTemplateReducer = createReducer(
  initialState,
  on(StepTemplateActions.addStepTemplate, (state, action) =>
    adapter.addOne(action.stepTemplate, state)
  ),
  on(StepTemplateActions.upsertStepTemplate, (state, action) =>
    adapter.upsertOne(action.stepTemplate, state)
  ),
  on(StepTemplateActions.addStepTemplates, (state, action) =>
    adapter.addMany(action.stepTemplates, state)
  ),
  on(StepTemplateActions.upsertStepTemplates, (state, action) =>
    adapter.upsertMany(action.stepTemplates, state)
  ),
  on(StepTemplateActions.updateStepTemplate, (state, action) =>
    adapter.updateOne(action.stepTemplate, state)
  ),
  on(StepTemplateActions.updateStepTemplates, (state, action) =>
    adapter.updateMany(action.stepTemplates, state)
  ),
  on(StepTemplateActions.deleteStepTemplate, (state, action) =>
    adapter.removeOne(action.id, state)
  ),
  on(StepTemplateActions.deleteStepTemplates, (state, action) =>
    adapter.removeMany(action.ids, state)
  ),
  on(StepTemplateActions.loadStepTemplates),
  on(StepTemplateActions.loadStepTemplatesSuccess, (state, action) =>
    adapter.addAll(action.stepTemplates, state)
  ),
  on(StepTemplateActions.clearStepTemplates, state => adapter.removeAll(state))
);

export function reducer(state: State | undefined, action: Action) {
  return stepTemplateReducer(state, action);
}

export const {
  selectIds,
  selectEntities,
  selectAll,
  selectTotal
} = adapter.getSelectors();
