import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
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
    StepComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    HttpClientModule,
    FormsModule,
    SharedModule,
    RouterModule.forRoot([
      { path: "", component: HomeComponent, pathMatch: "full" },
      {
        path: "steps",
        children: [
          { path: "", component: StepsComponent },
          { path: ":id", component: StepComponent },
        ]
      },
      { path: "step-templates", component: StepTemplatesComponent },
      {
        path: "sequences",
        children: [
          { path: "", component: SequencesComponent },
          { path: ":id", component: SequenceComponent }
        ]
      },
      {
        path: "sequence-templates",
        children: [
          { path: "", component: SequenceTemplatesComponent },
          { path: ":id", component: SequenceTemplateComponent }
        ]
      }
    ]),
    BrowserAnimationsModule
  ],
  providers: [NodeDataService, AppStateService],
  bootstrap: [AppComponent]
})
export class AppModule {}
