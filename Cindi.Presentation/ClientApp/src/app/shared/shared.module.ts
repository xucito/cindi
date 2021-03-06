import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { InputComponent } from "./components/dynamic-form/input/input.component";
import { DynamicFormComponent } from "./components/dynamic-form/dynamic-form.component";
import { StepViewComponent } from "./components/step-view/step-view.component";
import { NgxChartsModule } from '@swimlane/ngx-charts';
import {
  NbLayoutModule,
  NbCardModule,
  NbListModule,
  NbIconModule,
  NbActionsModule,
  NbStepperModule,
  NbButtonModule,
  NbSelectModule,
  NbInputModule,
  NbAccordionModule,
  NbBadgeModule,
  NbAlertModule
} from "@nebular/theme";
import { CodemirrorModule } from "@ctrl/ngx-codemirror";
import { TemplateViewComponent } from "./components/template-view/template-view.component";
import { WorkflowVisualizerComponent } from "./components/workflow-visualizer/workflow-visualizer.component";
import { NgxGraphModule } from "@swimlane/ngx-graph";
import { WorkflowStepComponent } from './components/workflow-step/workflow-step.component';
import { ConditionsGroupVisualizerComponent } from './components/conditions-group-visualizer/conditions-group-visualizer.component';
import { ConditionSelectorComponent } from './components/condition-selector/condition-selector.component';
import { LogicBlockVisualizerComponent } from './components/logic-block-visualizer/logic-block-visualizer.component';
import { StepMappingsVisualizerComponent } from './components/step-mappings-visualizer/step-mappings-visualizer.component';
import { WorkflowInputsComponent } from './components/workflow-inputs/workflow-inputs.component';
import { MappingSelectorComponent } from './components/mapping-selector/mapping-selector.component';
import { SelectStepTemplateComponent } from './components/select-step-template/select-step-template.component';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { DataTableComponent } from './components/data-table/data-table.component';

@NgModule({
  declarations: [
    DynamicFormComponent,
    InputComponent,
    StepViewComponent,
    TemplateViewComponent,
    WorkflowVisualizerComponent,
    WorkflowStepComponent,
    ConditionsGroupVisualizerComponent,
    ConditionSelectorComponent,
    LogicBlockVisualizerComponent,
    StepMappingsVisualizerComponent,
    WorkflowInputsComponent,
    MappingSelectorComponent,
    SelectStepTemplateComponent,
    DataTableComponent
  ],
  exports: [
    NgxDatatableModule,
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
    ConditionsGroupVisualizerComponent,
    LogicBlockVisualizerComponent,
    NgxChartsModule,
    DataTableComponent,
    CodemirrorModule,
    FormsModule,
    NbBadgeModule,
    NbAlertModule
  ],
  imports: [
    NgxDatatableModule,
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
    NbAccordionModule,
    NbActionsModule,
    NgxGraphModule,
    NbButtonModule,
    NbStepperModule,
    NgxChartsModule,
    NbBadgeModule,
    NbAlertModule
  ]
})
export class SharedModule {}
