import { NgModule } from "@angular/core";
import {
  NbCardModule,
  NbTabsetModule,
  NbListModule,
  NbIconModule,
  NbButtonModule,
  NbActionsModule
} from "@nebular/theme";

import { ThemeModule } from "../../@theme/theme.module";
import { DashboardComponent } from "./dashboard.component";
import { StepActivityComponent } from "./step-activity/step-activity.component";
import { StepCardComponent } from "./step-card/step-card.component";
import { StepsFeedComponent } from "./steps-feed/steps-feed.component";
import { StoreModule } from "@ngrx/store";

@NgModule({
  imports: [
    NbCardModule,
    ThemeModule,
    NbTabsetModule,
    NbListModule,
    NbIconModule,
    NbButtonModule,
    NbActionsModule
  ],
  declarations: [
    DashboardComponent,
    StepActivityComponent,
    StepCardComponent,
    StepsFeedComponent
  ]
})
export class DashboardModule {}
