import { CindiClientService } from "./../../services/cindi-client.service";
import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import * as fromWorkflowTemplates from "./workflow-template.actions";
import { catchError, map, switchMap } from "rxjs/operators";
import { of } from "rxjs";

@Injectable()
export class WorkflowTemplateEffects {
  constructor(
    private actions$: Actions,
    private cindiData: CindiClientService
  ) {}

  loadWorkflowTemplates$ = createEffect(() =>
    this.actions$.pipe(
      ofType(fromWorkflowTemplates.loadWorkflowTemplates),
      switchMap(payload =>
        this.cindiData.GetWorkflowTemplates().pipe(
          map(result => {
            const workflowTemplates = result.result;
            return fromWorkflowTemplates.loadWorkflowTemplatesSuccess({
              workflowTemplates
            });
          }),
          catchError(errors =>
            of(fromWorkflowTemplates.loadWorkflowTemplatesFail({ errors }))
          )
        )
      )
    )
  );
}
