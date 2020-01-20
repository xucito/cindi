import { SharedModule } from './../../shared/shared.module';
import { NgModule } from "@angular/core";
import {
  NbCardModule,
  NbTabsetModule,
  NbListModule,
  NbIconModule,
  NbButtonModule,
  NbActionsModule,
  NbToastrModule
} from "@nebular/theme";

import { ThemeModule } from "../../@theme/theme.module";
import { DashboardComponent } from "./dashboard.component";
import { StepActivityComponent } from "./step-activity/step-activity.component";
import { StepCardComponent } from "./step-card/step-card.component";
import { StepsFeedComponent } from "./steps-feed/steps-feed.component";
import { StoreModule } from "@ngrx/store";
import { StepTimeLineComponent } from './step-time-line/step-time-line.component';
import { WorkflowFeedComponent } from './workflow-feed/workflow-feed.component';

@NgModule({
  imports: [
    NbCardModule,
    ThemeModule,
    NbTabsetModule,
    NbListModule,
    NbIconModule,
    NbButtonModule,
    NbActionsModule,
    SharedModule,
    NbToastrModule
  ],
  declarations: [
    DashboardComponent,
    StepActivityComponent,
    StepCardComponent,
    StepsFeedComponent,
    StepTimeLineComponent,
    WorkflowFeedComponent
  ]
})
export class DashboardModule {}
