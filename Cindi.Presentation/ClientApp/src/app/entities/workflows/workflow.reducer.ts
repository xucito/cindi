import { Action, createReducer, on } from "@ngrx/store";
import { EntityState, EntityAdapter, createEntityAdapter } from "@ngrx/entity";
import { Workflow } from "./workflow.model";
import * as WorkflowActions from "./workflow.actions";

export interface State extends EntityState<Workflow> {
  // additional entities state properties
}

export const adapter: EntityAdapter<Workflow> = createEntityAdapter<Workflow>();

export const initialState: State = adapter.getInitialState({
  // additional entity state properties
});

const workflowReducer = createReducer(
  initialState,
  on(WorkflowActions.addWorkflow, (state, action) =>
    adapter.addOne(action.workflow, state)
  ),
  on(WorkflowActions.upsertWorkflow, (state, action) =>
    adapter.upsertOne(action.workflow, state)
  ),
  on(WorkflowActions.addWorkflows, (state, action) =>
    adapter.addMany(action.workflows, state)
  ),
  on(WorkflowActions.upsertWorkflows, (state, action) =>
    adapter.upsertMany(action.workflows, state)
  ),
  on(WorkflowActions.updateWorkflow, (state, action) =>
    adapter.updateOne(action.workflow, state)
  ),
  on(WorkflowActions.updateWorkflows, (state, action) =>
    adapter.updateMany(action.workflows, state)
  ),
  on(WorkflowActions.deleteWorkflow, (state, action) =>
    adapter.removeOne(action.id, state)
  ),
  on(WorkflowActions.deleteWorkflows, (state, action) =>
    adapter.removeMany(action.ids, state)
  ),
  on(WorkflowActions.loadWorkflows),
  on(WorkflowActions.loadWorkflowsSuccess, (state, action) =>
    adapter.addAll(action.workflows, state)
  ),
  on(WorkflowActions.clearWorkflows, state => adapter.removeAll(state))
);

export function reducer(state: State | undefined, action: Action) {
  return workflowReducer(state, action);
}

export const {
  selectIds,
  selectEntities,
  selectAll,
  selectTotal
} = adapter.getSelectors();
