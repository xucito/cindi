import { AuthenticationService } from './authentication-service';
import { Injectable, ChangeDetectorRef } from "@angular/core";
import { HttpHeaders, HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { NbAuthService } from "@nebular/auth";
import { Router } from "@angular/router";
import { EnvService } from "../../services/env.service";
import { Store } from "@ngrx/store";
import { State } from "../../reducers";
import { map } from "rxjs/operators";
import { setCurrentUser } from "../../reducers/root.actions";

@Injectable({
  providedIn: "root"
})
export class AuthenticationService {
  constructor(
    private http: HttpClient,
    private env: EnvService,
    private store: Store<State>
  ) {}

  login(username: string, password: string): Observable<any> {
    let headerDict = {
      Authorization: "basic " + window.btoa(username + ":" + password)
    };

    let requestOptions = {
      headers: new HttpHeaders(headerDict)
    };

    return this.http
      .get<any>(this.env.apiUrl + "/api/users/me", requestOptions)
      .pipe(
        map(result => {
          let user = result.result;
          // login successful if there's a user in the response
          if (user) {
            // store user details and basic auth credentials in local storage
            // to keep user logged in between page refreshes
            let AuthData = window.btoa(username + ":" + password);
            localStorage.setItem("authToken", AuthData);
            this.store.dispatch(setCurrentUser({ user: user }));
          }
        })
      );
  }

  tryLoginWithLocalStorage(): Observable<any> {
    let headerDict = {
      Authorization: "basic " + localStorage.getItem("authToken")
    };

    let requestOptions = {
      headers: new HttpHeaders(headerDict)
    };

    return this.http
      .get<any>(this.env.apiUrl + "/api/users/me", requestOptions)
      .pipe(
        map(result => {
          let user = result.result;
          // login successful if there's a user in the response
          if (user) {
            this.store.dispatch(setCurrentUser({ user: user }));
          }
        })
      );
  }
}
