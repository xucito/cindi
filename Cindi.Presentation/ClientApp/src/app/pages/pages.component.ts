import { loadGlobalValues } from "./../entities/global-values/global-value.actions";
import { loadWorkflowTemplates } from "./../entities/workflow-templates/workflow-template.actions";
import { loadWorkflows } from "./../entities/workflows/workflow.actions";
import { Component } from "@angular/core";

import { MENU_ITEMS } from "./pages-menu";
import { State } from "../reducers";
import { Store } from "@ngrx/store";
import { loadStepTemplates } from "../entities/step-templates/step-template.actions";
import { loadSteps } from "../entities/steps/step.actions";
import { Observable } from "rxjs";

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
  autoSaveInterval;
  autoLoadStepsInterval;
  constructor(private store: Store<State>) {
    this.store.dispatch(loadSteps({ status: undefined }));
    this.store.dispatch(loadStepTemplates());
    this.store.dispatch(loadWorkflows({ status: undefined }));
    this.store.dispatch(loadWorkflowTemplates());
    this.store.dispatch(loadGlobalValues());
    this.autoSaveInterval = setInterval(() =>  {
      console.log("loaded");
      this.loadAll();
    }, 30000);

      this.autoLoadStepsInterval = setInterval(() =>  {
        console.log("loaded");
        this.store.dispatch(loadSteps({ status: undefined }));
      }, 10000);
    // this.loadAll();
  }

  loadAll() {
    this.store.dispatch(loadStepTemplates());
    this.store.dispatch(loadWorkflows({ status: undefined }));
    this.store.dispatch(loadWorkflowTemplates());
    this.store.dispatch(loadGlobalValues());
  }
}
