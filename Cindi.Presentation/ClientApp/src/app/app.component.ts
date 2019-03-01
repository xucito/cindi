import { Component } from "@angular/core";
import { NodeDataService } from "./services/node-data.service";
import { AppStateService } from "./services/app-state.service";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"]
})
export class AppComponent {
  title = "app";
  constructor(private _appState: AppStateService) {
    _appState.refreshStepTemplateData().subscribe(
      (result) => {}
    );
  }
}
