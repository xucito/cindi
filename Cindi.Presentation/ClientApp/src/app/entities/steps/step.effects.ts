import * as fromStepsAction from "./step.actions";
import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { CindiClientService } from "../../services/cindi-client.service";
import { catchError, map, mergeMap, switchMap } from "rxjs/operators";
import { defer, of } from "rxjs";

@Injectable()
export class StepEffects {
  constructor(
    private actions$: Actions,
    private cindiData: CindiClientService
  ) {}

  loadSteps$ = createEffect(() =>
    this.actions$.pipe(
      ofType(fromStepsAction.loadSteps),
      switchMap(payload =>
        this.cindiData.GetSteps(payload.status).pipe(
          map(result => {
            let steps = result.result;
            return fromStepsAction.loadStepsSuccess({ steps });
          }),
          catchError(errors => of(fromStepsAction.loadStepsFail({ errors })))
        )
      )
    )
  );
}
