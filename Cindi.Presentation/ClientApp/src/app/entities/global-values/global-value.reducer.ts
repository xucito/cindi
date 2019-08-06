import { Action, createReducer, on } from "@ngrx/store";
import { EntityState, EntityAdapter, createEntityAdapter } from "@ngrx/entity";
import { GlobalValue } from "./global-value.model";
import * as GlobalValueActions from "./global-value.actions";

export interface State extends EntityState<GlobalValue> {
  // additional entities state properties
}

export const adapter: EntityAdapter<GlobalValue> = createEntityAdapter<
  GlobalValue
>({
  selectId: a => a.name
});

export const initialState: State = adapter.getInitialState({
  // additional entity state properties
});

const globalValueReducer = createReducer(
  initialState,
  on(GlobalValueActions.addGlobalValue, (state, action) =>
    adapter.addOne(action.globalValue, state)
  ),
  on(GlobalValueActions.upsertGlobalValue, (state, action) =>
    adapter.upsertOne(action.globalValue, state)
  ),
  on(GlobalValueActions.addGlobalValues, (state, action) =>
    adapter.addMany(action.globalValues, state)
  ),
  on(GlobalValueActions.upsertGlobalValues, (state, action) =>
    adapter.upsertMany(action.globalValues, state)
  ),
  on(GlobalValueActions.updateGlobalValue, (state, action) =>
    adapter.updateOne(action.globalValue, state)
  ),
  on(GlobalValueActions.updateGlobalValues, (state, action) =>
    adapter.updateMany(action.globalValues, state)
  ),
  on(GlobalValueActions.deleteGlobalValue, (state, action) =>
    adapter.removeOne(action.id, state)
  ),
  on(GlobalValueActions.deleteGlobalValues, (state, action) =>
    adapter.removeMany(action.ids, state)
  ),
  on(GlobalValueActions.loadGlobalValues),
  on(GlobalValueActions.loadGlobalValuesSuccess, (state, action) =>
    adapter.addAll(action.globalValues, state)
  ),
  on(GlobalValueActions.clearGlobalValues, state => adapter.removeAll(state))
);

export function reducer(state: State | undefined, action: Action) {
  return globalValueReducer(state, action);
}

export const {
  selectIds,
  selectEntities,
  selectAll,
  selectTotal
} = adapter.getSelectors((state: any) => state.globalValues);
