import {
  ActionReducer,
  ActionReducerMap,
  createFeatureSelector,
  createSelector,
  MetaReducer,
  Action,
  combineReducers
} from "@ngrx/store";
import { environment } from "../../environments/environment";
import * as fromStepTemplates from "../entities/step-templates/step-template.reducer";
import { InjectionToken } from "@angular/core";
import { currentUserReducer } from "./root.reducer";
import * as fromStep from "../entities/steps/step.reducer";
import * as fromWorkflows from "../entities/workflows/workflow.reducer";
import * as fromWorkflowTemplates from "../entities/workflow-templates/workflow-template.reducer";
import * as fromGlobalValues from "../entities/global-values/global-value.reducer";
import * as fromBotKeys from "../entities/bot-keys/bot-key.reducer";

export interface State {
  stepTemplates: fromStepTemplates.State;
  steps: fromStep.State;
  workflows: fromWorkflows.State,
  workflowTemplates: fromWorkflowTemplates.State,
  globalValues: fromGlobalValues.State,
  botKeys: fromBotKeys.State,
  currentUser: any;
}

export const ROOT_REDUCERS = new InjectionToken<
  ActionReducerMap<State, Action>
>("Root reducers token", {
  factory: () => ({
    stepTemplates: fromStepTemplates.reducer,
    currentUser: currentUserReducer,
    steps: fromStep.reducer,
    workflows: fromWorkflows.reducer,
    workflowTemplates: fromWorkflowTemplates.reducer,
    globalValues: fromGlobalValues.reducer,
    botKeys: fromBotKeys.reducer
  })
});

export function reducers(state: State | undefined, action: Action) {
  return combineReducers({
    stepTemplates: fromStepTemplates.reducer,
    currentUser: currentUserReducer,
    steps: fromStep.reducer,
    workflows: fromWorkflows.reducer,
    botKeys: fromBotKeys.reducer
  });
}



export const metaReducers: MetaReducer<State>[] = !environment.production
  ? []
  : [];
