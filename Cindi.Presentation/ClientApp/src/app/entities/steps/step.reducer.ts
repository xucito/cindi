import { filter } from "rxjs/operators";
import { Action, createReducer, on, createSelector } from "@ngrx/store";
import { EntityState, EntityAdapter, createEntityAdapter } from "@ngrx/entity";
import { Step } from "./step.model";
import * as StepActions from "./step.actions";

export interface State extends EntityState<Step> {
  // additional entities state properties
}

export const adapter: EntityAdapter<Step> = createEntityAdapter<Step>({
  sortComparer: (a, b) => sortByDate(a, b)
});

export function sortByDate(a: Step, b: Step): number {
  if (a.createdOn > b.createdOn) {
    return 1;
  } else {
    return 0;
  }
}

export const initialState: State = adapter.getInitialState({
  // additional entity state properties
});

const stepReducer = createReducer(
  initialState,
  on(StepActions.addStep, (state, action) =>
    adapter.addOne(action.step, state)
  ),
  on(StepActions.upsertStep, (state, action) =>
    adapter.upsertOne(action.step, state)
  ),
  on(StepActions.addSteps, (state, action) =>
    adapter.addMany(action.steps, state)
  ),
  on(StepActions.upsertSteps, (state, action) =>
    adapter.upsertMany(action.steps, state)
  ),
  on(StepActions.updateStep, (state, action) =>
    adapter.updateOne(action.step, state)
  ),
  on(StepActions.updateSteps, (state, action) =>
    adapter.updateMany(action.steps, state)
  ),
  on(StepActions.deleteStep, (state, action) =>
    adapter.removeOne(action.id, state)
  ),
  on(StepActions.deleteSteps, (state, action) =>
    adapter.removeMany(action.ids, state)
  ),
  on(StepActions.loadStepsSuccess, (state, action) =>
    adapter.addAll(action.steps, state)
  ),
  on(StepActions.loadSteps),
  on(StepActions.clearSteps, state => adapter.removeAll(state))
);

export function reducer(state: State | undefined, action: Action) {
  return stepReducer(state, action);
}

export const {
  selectIds,
  selectEntities,
  selectAll,
  selectTotal
} = adapter.getSelectors((state: any) => state.steps);

export const getMostRecentSteps = createSelector(
  selectAll,
  (steps, props) => {
    if (props.validStatuses) {
      steps = steps.filter(step => {
        return props.validStatuses.includes(step.status);
      });
    }
    return steps.slice(0, props.hits);
  }
);

export const getStep = createSelector(
  selectAll,
  (steps, props) => {
    for (let i = 0; i < steps.length; i++) {
      if (steps[i].id == props.id) {
        return steps[i];
      }
    }
  }
);

export const getStepCreationMetrics = createSelector(
  selectAll,
  (steps, props) => {
    const startDate = new Date();
    startDate.setMinutes(startDate.getMinutes() - 15);
    steps = steps.filter(s => new Date(s.createdOn) > startDate);

    const metrics = [];

    for (let i = 0; i < 15; i++) {
      const endDate: Date = new Date(startDate);
      endDate.setMinutes(startDate.getMinutes() + 1);
      const filteredSteps = steps.filter(s => {
        return (
          new Date(s.createdOn) >= startDate && new Date(s.createdOn) < endDate
        );
      });

      metrics.push({
        startDate: new Date(startDate),
        endDate: new Date(endDate),
        numberOfSteps: filteredSteps.length
      });
      startDate.setMinutes(startDate.getMinutes() + 1);
    }
    return metrics;
  }
);
