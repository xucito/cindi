import { createAction, props } from "@ngrx/store";
import { User } from '../entities/user.model';

export const setCurrentUser = createAction(
  "[login/API] Set Current User",
  props<{ user: User }>()
);
