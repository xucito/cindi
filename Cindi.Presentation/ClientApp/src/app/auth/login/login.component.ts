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
import { AuthenticationService } from '../services/authentication.service';

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
    public authentication: AuthenticationService
  ) {
    super(service, {}, cd, router);
  }

  login() {
    this.login$ = this.authentication
      .login(this.user.username, this.user.password)
      .subscribe(result => {
        this.router.navigate(["/"]);
      });
  }
}
