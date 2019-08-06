import { CindiClientService } from './../../services/cindi-client.service';
import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import * as fromWorkflowsActions from './workflow.actions';
import { catchError, map, switchMap } from "rxjs/operators";
import { of } from 'rxjs';

@Injectable()
export class WorkflowEffects {

  constructor(private actions$: Actions,
    private cindiData: CindiClientService) {}

  loadWorkflows$ = createEffect(() =>
    this.actions$.pipe(
      ofType(fromWorkflowsActions.loadWorkflows),
      switchMap(payload =>
        this.cindiData.GetWorkflows(payload.status).pipe(
          map(result => {
            const workflows = result.result;
            return fromWorkflowsActions.loadWorkflowsSuccess({ workflows });
          }),
          catchError(errors => of(fromWorkflowsActions.loadWorkflowsFail({ errors })))
        )
      )
    )
  );
}
