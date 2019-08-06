import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { switchMap, catchError, map } from 'rxjs/operators';
import { CindiClientService } from '../../services/cindi-client.service';
import * as fromGlobalValuesActions from './global-value.actions';
import { of } from 'rxjs';

@Injectable()
export class GlobalValueEffects {
  constructor(private actions$: Actions,
    private cindiClient: CindiClientService) {}

  loadGlobalValues$ = createEffect(() =>
    this.actions$.pipe(
      ofType(fromGlobalValuesActions.loadGlobalValues),
      switchMap(() =>
        this.cindiClient.GetGlobalValues().pipe(
          map(result => {
            let globalValues = result.result;
            return fromGlobalValuesActions.loadGlobalValuesSuccess({
              globalValues: globalValues
            });
          }),
          catchError(errors =>
            of(fromGlobalValuesActions.loadGlobalValuesFail({ errors }))
          )
        )
      )
    )
  );
}
