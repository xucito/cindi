import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { CindiClientService } from "../../services/cindi-client.service";
import * as fromStepTemplateActions from "./step-template.actions";
import { catchError, map, mergeMap, switchMap } from "rxjs/operators";
import { defer, of } from "rxjs";

@Injectable()
export class StepTemplateEffects {
  constructor(
    private actions$: Actions,
    private cindiData: CindiClientService
  ) {}

  loadStepTemplates$ = createEffect(() =>
    this.actions$.pipe(
      ofType(fromStepTemplateActions.loadStepTemplates),
      switchMap(() =>
        this.cindiData.GetStepTemplates().pipe(
          map(result => {
            let stepTemplates = result.result;
            return fromStepTemplateActions.loadStepTemplatesSuccess({ stepTemplates });
          }),
          catchError(errors =>
            of(fromStepTemplateActions.loadStepTemplatesFail({ errors }))
          )
        )
      )
    )
  );
}
