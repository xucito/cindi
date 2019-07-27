import { Component, OnInit, ChangeDetectorRef } from "@angular/core";
import { NbLoginComponent, NbAuthService } from "@nebular/auth";
import { HttpHeaders, HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { EnvService } from "../../services/env.service";
import { map } from "rxjs/operators";
import { Subscription } from "rxjs";
import { Store } from "@ngrx/store";
import { State } from "../../reducers";
import { setCurrentUser } from "../../reducers/root.actions";

@Component({
  selector: "login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.scss"]
})
export class LoginComponent extends NbLoginComponent {
  showMessages: any = {};
  login$: Subscription;

  constructor(
    public service: NbAuthService,
    public cd: ChangeDetectorRef,
    public router: Router,
    private http: HttpClient,
    private env: EnvService,
    private store: Store<State>
  ) {
    super(service, {}, cd, router);
  }

  login() {
    console.log(this.user);
    let headerDict = {
      Authorization:
        "basic " + window.btoa(this.user.username + ":" + this.user.password)
    };
    let requestOptions = {
      headers: new HttpHeaders(headerDict)
    };

    this.login$ = this.http
      .get<any>(this.env.apiUrl + "/api/users/me", requestOptions)
      .pipe(
        map(result => {
          let user = result.result;
          // login successful if there's a user in the response
          if (user) {
            // store user details and basic auth credentials in local storage
            // to keep user logged in between page refreshes
            let AuthData = window.btoa(
              this.user.username + ":" + this.user.password
            );
            localStorage.setItem("currentUser", JSON.stringify(user));
            localStorage.setItem("authToken", AuthData);
            this.store.dispatch(setCurrentUser({ user: user }));
          }
        })
      )
      .subscribe(result => {});
  }
}
