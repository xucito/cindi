/**
 * @license
 * Copyright Akveo. All Rights Reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 */
import { Component, OnInit } from "@angular/core";
import { AnalyticsService } from "./@core/utils/analytics.service";
import { Store } from "@ngrx/store";
import { State } from "./reducers";
import { loadStepTemplates } from "./entities/step-templates/step-template.actions";
import { Router } from "@angular/router";
import { AuthenticationService } from "./auth/services/authentication.service";

@Component({
  selector: "ngx-app",
  template: "<router-outlet></router-outlet>"
})
export class AppComponent implements OnInit {
  constructor(
    private store: Store<State>,
    private router: Router,
    private auth: AuthenticationService
  ) {
    let test = localStorage.getItem("authToken");
    if (localStorage.getItem("authToken") != null) {
      auth.tryLoginWithLocalStorage().subscribe(
        result => {},
        err => {
          if (err.status == 401) {
            localStorage.removeItem("authToken");
            router.navigate(["./auth/login"]);
          }
        }
      );
    } else if (store.select(s => s.currentUser == undefined)) {
      router.navigate(["./auth/login"]);
    }
  }

  ngOnInit() {}
}
