import { createAction, props } from "@ngrx/store";

export const setCurrentUser = createAction(
  "[login/API] Set Current User",
  props<{ user: any }>()
);
