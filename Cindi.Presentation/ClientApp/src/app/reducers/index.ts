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
import * as fromUser from "../entities/user.reducer";
import { InjectionToken } from "@angular/core";
import { currentUserReducer } from "./root.reducer";

export interface State {
  user: fromUser.State;
  currentUser: any;
}

export const ROOT_REDUCERS = new InjectionToken<
  ActionReducerMap<State, Action>
>("Root reducers token", {
  factory: () => ({
    user: fromUser.reducer,
    currentUser: currentUserReducer
  })
});

export function reducers(state: State | undefined, action: Action) {
  return combineReducers({
    user: fromUser.reducer,
    currentUser: currentUserReducer
  });
}

export const metaReducers: MetaReducer<State>[] = !environment.production
  ? []
  : [];
