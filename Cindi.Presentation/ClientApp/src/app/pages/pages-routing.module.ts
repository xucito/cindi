import { WorkflowsComponent } from './workflows/workflows.component';
import { MonitoringComponent } from './monitoring/monitoring.component';
import { BotsComponent } from './bots/bots.component';
import { GlobalValuesComponent } from './global-values/global-values.component';
import { WorkflowTemplate } from './../entities/workflow-templates/workflow-template.model';
import { RouterModule, Routes } from "@angular/router";
import { NgModule } from "@angular/core";

import { PagesComponent } from "./pages.component";
import { DashboardComponent } from "./dashboard/dashboard.component";
import { StepTemplatesComponent } from "./step-templates/step-templates.component";
import { StepTemplateComponent } from "./step-template/step-template.component";
import { WorkflowTemplatesComponent } from "./workflow-templates/workflow-templates.component";
import { WorkflowTemplateComponent } from './workflow-template/workflow-template.component';
import {WorkflowDesignerComponent} from './workflow-designer/workflow-designer.component';
import { StepsComponent } from './steps/steps.component';

const routes: Routes = [
  {
    path: "",
    component: PagesComponent,
    children: [
      {
        path: "steps",
        component: StepsComponent
      },
      {
        path: "workflows",
        component: WorkflowsComponent
      },
      {
        path: "dashboard",
        component: DashboardComponent
      },
      {
        path: "step-templates",
        children: [
          {
            path: "",
            component: StepTemplatesComponent,
            pathMatch: "full"
          },
          {
            path: ":id",
            component: StepTemplateComponent
          }
        ]
      },
      {
        path: "workflow-templates",
        children: [
          {
            path: "",
            component: WorkflowTemplatesComponent,
            pathMatch: "full"
          },
          {
            path: ":id",
            component: WorkflowTemplateComponent
          }
        ]
      },
      {
        path: "bots",
        children: [
          {
            path: "",
            component: BotsComponent,
            pathMatch: "full"
          }
        ]
      },
      {
        path: "workflow-designer",
        children: [
          {
            path: "",
            component: WorkflowDesignerComponent,
            pathMatch: "full"
          }
        ]
      },
      {
        path: "",
        redirectTo: "dashboard",
        pathMatch: "full"
      },
      {
        path: "global-values",
        children: [
          {
            path: "",
            component: GlobalValuesComponent,
            pathMatch: "full"
          }
        ]
      },
      {
        path: "monitoring",
        children: [
          {
            path: "",
            component: MonitoringComponent,
            pathMatch: "full"
          }
        ]
      },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PagesRoutingModule {}
