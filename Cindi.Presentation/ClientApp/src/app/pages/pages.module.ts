import { NgModule } from '@angular/core';
import { NbMenuModule } from '@nebular/theme';

import { ThemeModule } from '../@theme/theme.module';
import { PagesComponent } from './pages.component';
import { DashboardModule } from './dashboard/dashboard.module';
import { PagesRoutingModule } from './pages-routing.module';
import { StepTemplatesComponent } from './step-templates/step-templates.component';
import { SharedModule } from '../shared/shared.module';
import { StepTemplateComponent } from './step-template/step-template.component';
import { WorkflowComponent } from './workflow/workflow.component';
import { WorkflowsComponent } from './workflows/workflows.component';
import { WorkflowTemplatesComponent } from './workflow-templates/workflow-templates.component';
import { WorkflowTemplateComponent } from './workflow-template/workflow-template.component';
import { GlobalValuesComponent } from './global-values/global-values.component';
import { WorkflowDesignerComponent } from './workflow-designer/workflow-designer.component';
import { BotsComponent } from './bots/bots.component';
import { MonitoringComponent } from './monitoring/monitoring.component';
import { StepComponent } from './step/step.component';
import { StepsComponent } from './steps/steps.component';

@NgModule({
  imports: [
    PagesRoutingModule,
    ThemeModule,
    NbMenuModule,
    DashboardModule,
    SharedModule
  ],
  declarations: [
    PagesComponent,
    StepTemplatesComponent,
    StepTemplateComponent,
    WorkflowComponent,
    WorkflowsComponent,
    WorkflowTemplatesComponent,
    WorkflowTemplateComponent,
    GlobalValuesComponent,
    WorkflowDesignerComponent,
    BotsComponent,
    MonitoringComponent,
    StepComponent,
    StepsComponent
  ],
})
export class PagesModule {
}
