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
  NbActionsModule,
  NbStepperModule,
  NbButtonModule,
  NbSelectModule,
  NbInputModule
} from "@nebular/theme";
import { CodemirrorModule } from "@ctrl/ngx-codemirror";
import { TemplateViewComponent } from "./components/template-view/template-view.component";
import { WorkflowVisualizerComponent } from "./components/workflow-visualizer/workflow-visualizer.component";
import { NgxGraphModule } from "@swimlane/ngx-graph";
import { WorkflowStepComponent } from './components/workflow-step/workflow-step.component';
import { ConditionsGroupVisualizerComponent } from './components/conditions-group-visualizer/conditions-group-visualizer.component';
import { ConditionSelectorComponent } from './components/condition-selector/condition-selector.component';

@NgModule({
  declarations: [
    DynamicFormComponent,
    InputComponent,
    StepViewComponent,
    TemplateViewComponent,
    WorkflowVisualizerComponent,
    WorkflowStepComponent,
    ConditionsGroupVisualizerComponent,
    ConditionSelectorComponent
  ],
  exports: [
    DynamicFormComponent,
    StepViewComponent,
    NbListModule,
    NbIconModule,
    NbCardModule,
    NbStepperModule,
    NbActionsModule,
    NbButtonModule,
    NbSelectModule,
    NbInputModule,
    TemplateViewComponent,
    WorkflowVisualizerComponent,
    NgxGraphModule,
    ConditionsGroupVisualizerComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    NbSelectModule,
    ReactiveFormsModule,
    NbLayoutModule,
    NbCardModule,
    CodemirrorModule,
    NbListModule,
    NbIconModule,
    NbInputModule,
    NbCardModule,
    NbActionsModule,
    NgxGraphModule,
    NbButtonModule,
    NbStepperModule
  ]
})
export class SharedModule {}
