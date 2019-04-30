import { Injectable } from "@angular/core";
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from "@angular/common/http";
import { Observable } from "rxjs";

@Injectable()
export class BasicAuthInterceptor implements HttpInterceptor {
  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    // add authorization header with basic auth credentials if available
    if (request.headers.get("Authorization") == undefined) {
      //let currentUser = JSON.parse(localStorage.getItem("currentUser"));
      let authToken = localStorage.getItem("authToken");
      if (authToken) {
        request = request.clone({
          setHeaders: {
            Authorization: `Basic ${authToken}`
          }
        });
      }
    }

    return next.handle(request);
  }
}
