import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { RouterModule } from "@angular/router";

import { AppComponent } from "./app.component";
import { NavMenuComponent } from "./nav-menu/nav-menu.component";
import { HomeComponent } from "./home/home.component";
import { SequencesComponent } from "./sequences/sequences.component";
import { NodeDataService } from "./services/node-data.service";
import { StepsComponent } from "./steps/steps.component";
import { StepTemplatesComponent } from "./step-templates/step-templates.component";
import { SharedModule } from "./shared/shared.module";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { SequenceTemplatesComponent } from "./sequence-templates/sequence-templates.component";
import { SequenceTemplateComponent } from "./sequence-template/sequence-template.component";
import { AppStateService } from "./services/app-state.service";
import { SequenceComponent } from "./sequence/sequence.component";
import { StepComponent } from "./step/step.component";
import { StatusCardComponent } from "./home/components/status-card/status-card.component";
import { LoadingBarService } from "./services/loading-bar.service";
import { LoginComponent } from "./login/login.component";
import { AuthGuard } from "./auth/auth.guard";
import { BasicAuthInterceptor } from "./auth/basic-auth.interceptor";
import { ErrorInterceptor } from "./auth/error.interceptor";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    SequencesComponent,
    StepsComponent,
    StepTemplatesComponent,
    SequenceTemplatesComponent,
    SequenceTemplateComponent,
    SequenceComponent,
    StepComponent,
    StatusCardComponent,
    LoginComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    HttpClientModule,
    FormsModule,
    SharedModule,
    RouterModule.forRoot([
      {
        path: "",
        component: HomeComponent,
        pathMatch: "full",
        canActivate: [AuthGuard]
      },
      {
        path: "login",
        component: LoginComponent,
        pathMatch: "full"
      },
      {
        path: "steps",
        children: [
          { path: "", component: StepsComponent },
          { path: ":id", component: StepComponent }
        ],
        canActivate: [AuthGuard]
      },
      {
        path: "step-templates",
        component: StepTemplatesComponent,
        canActivate: [AuthGuard]
      },
      {
        path: "sequences",
        children: [
          { path: "", component: SequencesComponent },
          { path: ":id", component: SequenceComponent }
        ],
        canActivate: [AuthGuard]
      },
      {
        path: "sequence-templates",
        children: [
          { path: "", component: SequenceTemplatesComponent },
          { path: ":id", component: SequenceTemplateComponent }
        ],
        canActivate: [AuthGuard]
      }
    ]),
    BrowserAnimationsModule
  ],
  providers: [
    NodeDataService,
    AppStateService,
    LoadingBarService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: BasicAuthInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
