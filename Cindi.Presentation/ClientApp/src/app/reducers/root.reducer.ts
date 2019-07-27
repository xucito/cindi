import { createReducer, on } from "@ngrx/store";
import { setCurrentUser } from "./root.actions";

export const currentUserReducer = createReducer(
  undefined,
  on(setCurrentUser, (state, action) => action.user)
);
