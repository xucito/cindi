import { Component } from "@angular/core";
import { NodeDataService } from "./services/node-data.service";
import { AppStateService } from "./services/app-state.service";
import { LoadingBarService } from "./services/loading-bar.service";
import { Subscription } from "rxjs";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"]
})
export class AppComponent {
  title = "app";
  color = "primary";
  mode = "indeterminate";
  value = 50;
  bufferValue = 75;
  isLoading = false;
  loadingBar$: Subscription;
  user$: Subscription;
  user: any;

  constructor(
    private _appState: AppStateService,
    private loadingBar: LoadingBarService
  ) {
    //_appState.refreshStepTemplateData().subscribe(result => {});
    this.loadingBar$ = loadingBar.IsLoading.subscribe(
      (result) => {
        this.isLoading = result;
      }
    )
    this.user$ = _appState.currentUser.subscribe(
      (user) => {
        this.user = user;
      }
    )
  }
}
