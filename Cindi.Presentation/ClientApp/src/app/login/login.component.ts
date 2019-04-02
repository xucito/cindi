import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { AuthenticationService } from "../auth/authentication.service";
import { Subscription } from "rxjs";
import { AppStateService } from "../services/app-state.service";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent implements OnInit {
  params$: Subscription;

  constructor(
    private router: Router,
    private auth: AuthenticationService,
    private route: ActivatedRoute,
    private appState: AppStateService
  ) {
    this.params$ = route.queryParams.subscribe(
      (params) => {
        this.redirectUrl = params['returnUrl'];
      }
    )
  }
  username: string;
  password: string;
  redirectUrl = "";
  ngOnInit() {}
  login$: Subscription;
  login(): void {
    this.login$ = this.auth.login(this.username, this.password).subscribe(
      result => {
        this.router.navigate([this.redirectUrl])
      },
      err => {
        alert("Invalid credentials");
      }
    );
  }
}
