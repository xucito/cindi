import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { environment } from "../../environments/environment";
import { map } from "rxjs/operators";
import { AppStateService } from "../services/app-state.service";
@Injectable({
  providedIn: "root"
})
export class AuthenticationService {
  constructor(private http: HttpClient, private appState: AppStateService) {}

  login(username: string, password: string) {
    let headerDict = {
      Authorization: "basic " + window.btoa(username + ":" + password)
    };
    let requestOptions = {
      headers: new HttpHeaders(headerDict)
    };

    return this.http
      .get<any>(environment.apiUrl + "/api/users", requestOptions)
      .pipe(
        map(result => {
          let user = result.result;
          // login successful if there's a user in the response
          if (user) {
            // store user details and basic auth credentials in local storage
            // to keep user logged in between page refreshes
            user.authdata = window.btoa(username + ":" + password);
            localStorage.setItem("currentUser", JSON.stringify(user));
            this.appState.setCurrentUser(user);
          }

          return user;
        })
      );
  }

  logout() {
    // remove user from local storage to log user out
    localStorage.removeItem("currentUser");
  }
}
