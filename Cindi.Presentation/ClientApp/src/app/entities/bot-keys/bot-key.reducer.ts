import { Action, createReducer, on } from "@ngrx/store";
import { EntityState, EntityAdapter, createEntityAdapter } from "@ngrx/entity";
import { BotKey } from "./bot-key.model";
import * as BotKeyActions from "./bot-key.actions";

export interface State extends EntityState<BotKey> {
  // additional entities state properties
}

export const adapter: EntityAdapter<BotKey> = createEntityAdapter<
  BotKey
>({
  selectId: a => a.id
});

export const initialState: State = adapter.getInitialState({
  // additional entity state properties
});

const botKeyReducer = createReducer(
  initialState,
  on(BotKeyActions.addBotKey, (state, action) =>
    adapter.addOne(action.botKey, state)
  ),
  on(BotKeyActions.upsertBotKey, (state, action) =>
    adapter.upsertOne(action.botKey, state)
  ),
  on(BotKeyActions.addBotKeys, (state, action) =>
    adapter.addMany(action.botKeys, state)
  ),
  on(BotKeyActions.upsertBotKeys, (state, action) =>
    adapter.upsertMany(action.botKeys, state)
  ),
  on(BotKeyActions.updateBotKey, (state, action) =>
    adapter.updateOne(action.botKey, state)
  ),
  on(BotKeyActions.updateBotKeys, (state, action) =>
    adapter.updateMany(action.botKeys, state)
  ),
  on(BotKeyActions.deleteBotKey, (state, action) =>
    adapter.removeOne(action.id, state)
  ),
  on(BotKeyActions.deleteBotKeys, (state, action) =>
    adapter.removeMany(action.ids, state)
  ),
  on(BotKeyActions.loadBotKeys),
  on(BotKeyActions.loadBotKeysSuccess, (state, action) =>
    adapter.addAll(action.botKeys, state)
  ),
  on(BotKeyActions.clearBotKeys, state => adapter.removeAll(state))
);

export function reducer(state: State | undefined, action: Action) {
  return botKeyReducer(state, action);
}

export const {
  selectIds,
  selectEntities,
  selectAll,
  selectTotal
} = adapter.getSelectors((state: any) => state.botKeys);
