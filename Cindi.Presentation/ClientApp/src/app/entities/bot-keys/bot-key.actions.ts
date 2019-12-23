import { createAction, props } from '@ngrx/store';
import { Update } from '@ngrx/entity';
import { BotKey } from './bot-key.model';

export const loadBotKeys = createAction(
  '[BotKey/API] Load BotKeys'
);

export const loadBotKeysSuccess = createAction(
  '[BotKey/API] Load BotKeys Success',
  props<{ botKeys: BotKey[] }>()
);
export const loadBotKeysFail = createAction(
  '[BotKey/API] Load BotKeys Fail',
  props<{ errors: any }>()
);

export const addBotKey = createAction(
  '[BotKey/API] Add BotKey',
  props<{ botKey: BotKey }>()
);

export const upsertBotKey = createAction(
  '[BotKey/API] Upsert BotKey',
  props<{ botKey: BotKey }>()
);

export const addBotKeys = createAction(
  '[BotKey/API] Add BotKeys',
  props<{ botKeys: BotKey[] }>()
);

export const upsertBotKeys = createAction(
  '[BotKey/API] Upsert BotKeys',
  props<{ botKeys: BotKey[] }>()
);

export const updateBotKey = createAction(
  '[BotKey/API] Update BotKey',
  props<{ botKey: Update<BotKey> }>()
);

export const updateBotKeys = createAction(
  '[BotKey/API] Update BotKeys',
  props<{ botKeys: Update<BotKey>[] }>()
);

export const deleteBotKey = createAction(
  '[BotKey/API] Delete BotKey',
  props<{ id: string }>()
);

export const deleteBotKeys = createAction(
  '[BotKey/API] Delete BotKeys',
  props<{ ids: string[] }>()
);

export const clearBotKeys = createAction(
  '[BotKey/API] Clear BotKeys'
);
