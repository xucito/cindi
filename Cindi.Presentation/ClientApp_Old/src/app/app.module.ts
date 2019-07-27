import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { RouterModule } from "@angular/router";

import { AppComponent } from "./app.component";
import { NavMenuComponent } from "./nav-menu/nav-menu.component";
import { HomeComponent } from "./home/home.component";
import { WorkflowsComponent } from "./workflows/workflows.component";
import { NodeDataService } from "./services/node-data.service";
import { StepsComponent } from "./steps/steps.component";
import { StepTemplatesComponent } from "./step-templates/step-templates.component";
import { SharedModule } from "./shared/shared.module";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { WorkflowTemplatesComponent } from "./workflow-templates/workflow-templates.component";
import { WorkflowTemplateComponent } from "./workflow-template/workflow-template.component";
import { AppStateService } from "./services/app-state.service";
import { WorkflowComponent } from "./workflow/workflow.component";
import { StepComponent } from "./step/step.component";
import { StatusCardComponent } from "./home/components/status-card/status-card.component";
import { LoadingBarService } from "./services/loading-bar.service";
import { LoginComponent } from "./login/login.component";
import { AuthGuard } from "./auth/auth.guard";
import { BasicAuthInterceptor } from "./auth/basic-auth.interceptor";
import { ErrorInterceptor } from "./auth/error.interceptor";
import { SecretModalComponent } from "./shared/components/modals/secret-modal/secret-modal.component";
import { MatDialogRef, MatDialogModule } from "@angular/material";
import { UsersComponent } from "./users/users.component";
import { AddUsersModalComponent } from "./shared/components/modals/add-users-modal/add-users-modal.component";
import { GlobalValuesComponent } from './global-values/global-values.component';
import { AddGlobalValueModalComponent } from "./shared/components/modals/add-global-value-modal/add-global-value-modal.component";
import { EnvServiceProvider } from "./services/env.service.provider";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    WorkflowsComponent,
    StepsComponent,
    StepTemplatesComponent,
    WorkflowTemplatesComponent,
    WorkflowTemplateComponent,
    WorkflowComponent,
    StepComponent,
    StatusCardComponent,
    LoginComponent,
    UsersComponent,
    GlobalValuesComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    HttpClientModule,
    FormsModule,
    SharedModule,
    MatDialogModule,
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
        path: "workflows",
        children: [
          { path: "", component: WorkflowsComponent },
          { path: ":id", component: WorkflowComponent }
        ],
        canActivate: [AuthGuard]
      },
      {
        path: "workflow-templates",
        children: [
          { path: "", component: WorkflowTemplatesComponent },
          { path: ":id", component: WorkflowTemplateComponent }
        ],
        canActivate: [AuthGuard]
      },
      {
        path: "users",
        component: UsersComponent
      },
      {
        path: "global-values",
        component: GlobalValuesComponent
      }
    ]),
    BrowserAnimationsModule
  ],
  providers: [
    EnvServiceProvider,
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
    },
    { provide: MatDialogRef, useValue: {} }
  ],
  bootstrap: [AppComponent],
  entryComponents: [SecretModalComponent, AddUsersModalComponent, AddGlobalValueModalComponent]
})
export class AppModule {}
