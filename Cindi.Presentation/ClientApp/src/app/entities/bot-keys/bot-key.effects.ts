import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { switchMap, catchError, map } from 'rxjs/operators';
import { CindiClientService } from '../../services/cindi-client.service';
import * as fromBotKeysActions from './bot-key.actions';
import { of } from 'rxjs';

@Injectable()
export class BotKeyEffects {
  constructor(private actions$: Actions,
    private cindiClient: CindiClientService) {}

  loadBotKeys$ = createEffect(() =>
    this.actions$.pipe(
      ofType(fromBotKeysActions.loadBotKeys),
      switchMap(() =>
        this.cindiClient.GetBotKeys().pipe(
          map(result => {
            let botKeys = result.result;
            return fromBotKeysActions.loadBotKeysSuccess({
              botKeys: botKeys
            });
          }),
          catchError(errors =>
            of(fromBotKeysActions.loadBotKeysFail({ errors }))
          )
        )
      )
    )
  );
}
