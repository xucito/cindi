import { Injectable } from "@angular/core";
import { Actions, Effect, createEffect, ofType } from "@ngrx/effects";
import { CindiClientService } from "../services/cindi-client.service";
import { catchError, map, switchMap } from "rxjs/operators";
import { defer, of } from "rxjs";
import { User } from "./user.model";
@Injectable()
export class UserEffects {
  constructor(
    private actions$: Actions,
    private cindiClient: CindiClientService
  ) {}
}
