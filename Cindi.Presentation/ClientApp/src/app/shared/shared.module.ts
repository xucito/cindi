import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { InputComponent } from "./components/dynamic-form/input/input.component";
import { DynamicFormComponent } from "./components/dynamic-form/dynamic-form.component";
import { StepViewComponent } from "./components/step-view/step-view.component";
import {
  NbLayoutModule,
  NbCardModule,
  NbListModule,
  NbIconModule,
  NbActionsModule
} from "@nebular/theme";
import { CodemirrorModule } from "@ctrl/ngx-codemirror";
import { TemplateViewComponent } from "./components/template-view/template-view.component";
import { WorkflowVisualizerComponent } from "./components/workflow-visualizer/workflow-visualizer.component";
import { NgxGraphModule } from "@swimlane/ngx-graph";
import { WorkflowStepComponent } from './components/workflow-step/workflow-step.component';

@NgModule({
  declarations: [
    DynamicFormComponent,
    InputComponent,
    StepViewComponent,
    TemplateViewComponent,
    WorkflowVisualizerComponent,
    WorkflowStepComponent
  ],
  exports: [
    DynamicFormComponent,
    StepViewComponent,
    NbListModule,
    NbIconModule,
    NbCardModule,
    NbActionsModule,
    TemplateViewComponent,
    WorkflowVisualizerComponent,
    NgxGraphModule
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NbLayoutModule,
    NbCardModule,
    CodemirrorModule,
    NbListModule,
    NbIconModule,
    NbCardModule,
    NbActionsModule,
    NgxGraphModule
  ]
})
export class SharedModule {}
