import { createAction, props } from '@ngrx/store';
import { Update } from '@ngrx/entity';

import { GlobalValue } from './global-value.model';

export const loadGlobalValues = createAction(
  '[GlobalValue/API] Load GlobalValues'
);

export const loadGlobalValuesSuccess = createAction(
  '[GlobalValue/API] Load GlobalValues Success',
  props<{ globalValues: GlobalValue[] }>()
);
export const loadGlobalValuesFail = createAction(
  '[GlobalValue/API] Load GlobalValues Fail',
  props<{ errors: any }>()
);

export const addGlobalValue = createAction(
  '[GlobalValue/API] Add GlobalValue',
  props<{ globalValue: GlobalValue }>()
);

export const upsertGlobalValue = createAction(
  '[GlobalValue/API] Upsert GlobalValue',
  props<{ globalValue: GlobalValue }>()
);

export const addGlobalValues = createAction(
  '[GlobalValue/API] Add GlobalValues',
  props<{ globalValues: GlobalValue[] }>()
);

export const upsertGlobalValues = createAction(
  '[GlobalValue/API] Upsert GlobalValues',
  props<{ globalValues: GlobalValue[] }>()
);

export const updateGlobalValue = createAction(
  '[GlobalValue/API] Update GlobalValue',
  props<{ globalValue: Update<GlobalValue> }>()
);

export const updateGlobalValues = createAction(
  '[GlobalValue/API] Update GlobalValues',
  props<{ globalValues: Update<GlobalValue>[] }>()
);

export const deleteGlobalValue = createAction(
  '[GlobalValue/API] Delete GlobalValue',
  props<{ id: string }>()
);

export const deleteGlobalValues = createAction(
  '[GlobalValue/API] Delete GlobalValues',
  props<{ ids: string[] }>()
);

export const clearGlobalValues = createAction(
  '[GlobalValue/API] Clear GlobalValues'
);
