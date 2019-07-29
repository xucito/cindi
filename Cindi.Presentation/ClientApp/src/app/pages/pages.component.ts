import { Component } from "@angular/core";

import { MENU_ITEMS } from "./pages-menu";
import { State } from "../reducers";
import { Store } from "@ngrx/store";
import { loadStepTemplates } from "../entities/step-templates/step-template.actions";
import { loadSteps } from "../entities/steps/step.actions";

@Component({
  selector: "ngx-pages",
  styleUrls: ["pages.component.scss"],
  template: `
    <ngx-one-column-layout>
      <nb-menu [items]="menu"></nb-menu>
      <router-outlet></router-outlet>
    </ngx-one-column-layout>
  `
})
export class PagesComponent {
  menu = MENU_ITEMS;
  constructor(private store: Store<State>) {
    store.dispatch(loadStepTemplates());
    store.dispatch(loadSteps({ status: undefined }));
  }
}
