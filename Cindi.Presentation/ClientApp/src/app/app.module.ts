import { GlobalValueEffects } from './entities/global-values/global-value.effects';
import { WorkflowTemplateEffects } from "./entities/workflow-templates/workflow-template.effects";
/**
 * @license
 * Copyright Akveo. All Rights Reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 */
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { NgModule } from "@angular/core";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { CoreModule } from "./@core/core.module";
import { ThemeModule } from "./@theme/theme.module";
import { AppComponent } from "./app.component";
import { AppRoutingModule } from "./app-routing.module";
import {
  NbChatModule,
  NbDatepickerModule,
  NbDialogModule,
  NbMenuModule,
  NbSidebarModule,
  NbToastrModule,
  NbWindowModule,
  NbOverlayContainerComponent,
  NbWindowService,
  NbWindowComponent
} from "@nebular/theme";
import { NgxAuthModule } from "./auth/auth.module";
import { EnvServiceProvider } from "./services/env.service.provider";
import { CindiClientService } from "./services/cindi-client.service";
import { StoreModule } from "@ngrx/store";
import { metaReducers, ROOT_REDUCERS } from "./reducers";
import { StoreDevtoolsModule } from "@ngrx/store-devtools";
import { environment } from "../environments/environment";
import { EffectsModule } from "@ngrx/effects";
import { BasicAuthInterceptor } from "./auth/basic-auth.interceptor";
import { StepTemplateEffects } from "./entities/step-templates/step-template.effects";
import { StepEffects } from "./entities/steps/step.effects";
import { AuthenticationService } from "./auth/services/authentication.service";
import { SharedModule } from "./shared/shared.module";
import { WorkflowEffects } from "./entities/workflows/workflow.effects";

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    NgxAuthModule,
    NbSidebarModule.forRoot(),
    NbMenuModule.forRoot(),
    NbDatepickerModule.forRoot(),
    NbDialogModule.forRoot(),
    NbWindowModule.forRoot(),
    NbToastrModule.forRoot(),
    NbChatModule.forRoot({
      messageGoogleMapKey: "AIzaSyA_wNuCzia92MAmdLRzmqitRGvCF7wCZPY"
    }),
    CoreModule.forRoot(),
    StoreModule.forRoot(ROOT_REDUCERS, {
      metaReducers,
      runtimeChecks: {
        strictStateImmutability: true,
        strictActionImmutability: true
      }
    }),
    !environment.production ? StoreDevtoolsModule.instrument() : [],
    EffectsModule.forRoot([
      StepTemplateEffects,
      StepEffects,
      WorkflowEffects,
      WorkflowTemplateEffects,
      GlobalValueEffects
    ]),
    ThemeModule.forRoot(),
    SharedModule,
    NbToastrModule.forRoot()
  ],
  bootstrap: [AppComponent],
  providers: [
    EnvServiceProvider,
    CindiClientService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: BasicAuthInterceptor,
      multi: true
    },
    AuthenticationService
  ]
})
export class AppModule {}
