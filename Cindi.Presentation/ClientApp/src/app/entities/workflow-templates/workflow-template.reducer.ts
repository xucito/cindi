import { Action, createReducer, on, createSelector } from "@ngrx/store";
import {
  EntityState,
  EntityAdapter,
  createEntityAdapter,
  Dictionary
} from "@ngrx/entity";
import { WorkflowTemplate } from "./workflow-template.model";
import * as WorkflowTemplateActions from "./workflow-template.actions";

export interface State extends EntityState<WorkflowTemplate> {
  // additional entities state properties
}

export const adapter: EntityAdapter<WorkflowTemplate> = createEntityAdapter<
  WorkflowTemplate
>({
  selectId: a => a.referenceId
});

export const initialState: State = adapter.getInitialState({
  // additional entity state properties
});

const workflowTemplateReducer = createReducer(
  initialState,
  on(WorkflowTemplateActions.addWorkflowTemplate, (state, action) =>
    adapter.addOne(action.workflowTemplate, state)
  ),
  on(WorkflowTemplateActions.upsertWorkflowTemplate, (state, action) =>
    adapter.upsertOne(action.workflowTemplate, state)
  ),
  on(WorkflowTemplateActions.addWorkflowTemplates, (state, action) =>
    adapter.addMany(action.workflowTemplates, state)
  ),
  on(WorkflowTemplateActions.upsertWorkflowTemplates, (state, action) =>
    adapter.upsertMany(action.workflowTemplates, state)
  ),
  on(WorkflowTemplateActions.updateWorkflowTemplate, (state, action) =>
    adapter.updateOne(action.workflowTemplate, state)
  ),
  on(WorkflowTemplateActions.updateWorkflowTemplates, (state, action) =>
    adapter.updateMany(action.workflowTemplates, state)
  ),
  on(WorkflowTemplateActions.deleteWorkflowTemplate, (state, action) =>
    adapter.removeOne(action.id, state)
  ),
  on(WorkflowTemplateActions.deleteWorkflowTemplates, (state, action) =>
    adapter.removeMany(action.ids, state)
  ),
  on(WorkflowTemplateActions.loadWorkflowTemplates),
  on(WorkflowTemplateActions.loadWorkflowTemplatesSuccess, (state, action) =>
    adapter.addAll(action.workflowTemplates, state)
  ),
  on(WorkflowTemplateActions.clearWorkflowTemplates, state =>
    adapter.removeAll(state)
  )
);

export function reducer(state: State | undefined, action: Action) {
  return workflowTemplateReducer(state, action);
}

export const {
  selectIds,
  selectEntities,
  selectAll,
  selectTotal
} = adapter.getSelectors((state: any) => state.workflowTemplates);

export const getWorkflowTemplateEntityById = createSelector(
  selectEntities,
  (entities, props) => {
    return entities[props.referenceId];
  }
);
