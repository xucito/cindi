import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  MatMenuModule,
  MatSidenavModule,
  MatButtonModule,
  MatTableModule,
  MatIconModule,
  MatProgressBarModule,
  MatListModule,
  MatFormFieldModule,
  MatProgressSpinnerModule,
  MatDialogModule,
  MatBottomSheetModule
} from "@angular/material";
import { MatCardModule } from "@angular/material/card";
import { WorkflowVisualizationComponent } from "./components/workflow-visualization/workflow-visualization.component";
import { WorkflowTemplateVisualizationComponent } from "./components/workflow-template-visualization/workflow-template-visualization.component";
import { NgxGraphModule } from "../../../linked_projects/ngx-graph/src";
import { NgxChartsModule } from "@swimlane/ngx-charts";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { OutletComponent } from "./components/outlet/outlet.component";
import { FormsComponent } from "./components/form/form.component";
import { InputComponent } from "./components/form/input/input.component";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

import { CodemirrorModule } from "@ctrl/ngx-codemirror";
import "codemirror/mode/shell/shell";
import "./modes/console";
import { StepProgressBarComponent } from "./components/step-progress-bar/step-progress-bar.component";
import { UpdateViewerComponent } from "./components/update-viewer/update-viewer.component";
import { SecretModalComponent } from "./components/modals/secret-modal/secret-modal.component";
import { AddUsersModalComponent } from './components/modals/add-users-modal/add-users-modal.component';
import { AddGlobalValueModalComponent } from './components/modals/add-global-value-modal/add-global-value-modal.component';

@NgModule({
  imports: [
    CommonModule,
    MatMenuModule,
    MatButtonModule,
    MatTableModule,
    BrowserAnimationsModule,
    NgxChartsModule,
    NgxGraphModule,
    MatIconModule,
    MatCardModule,
    FormsModule,
    MatProgressBarModule,
    ReactiveFormsModule,
    CodemirrorModule,
    MatListModule,
    MatBottomSheetModule,
    MatDialogModule
  ],
  exports: [
    WorkflowTemplateVisualizationComponent,
    WorkflowVisualizationComponent,
    MatButtonModule,
    MatMenuModule,
    MatTableModule,
    MatIconModule,
    FormsComponent,
    MatCardModule,
    MatProgressBarModule,
    MatListModule,
    CodemirrorModule,
    StepProgressBarComponent,
    UpdateViewerComponent,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatBottomSheetModule,
    SecretModalComponent,
    MatDialogModule
    //MatSidenavModule
  ],
  declarations: [
    WorkflowVisualizationComponent,
    WorkflowTemplateVisualizationComponent,
    OutletComponent,
    FormsComponent,
    InputComponent,
    StepProgressBarComponent,
    UpdateViewerComponent,
    SecretModalComponent,
    AddUsersModalComponent,
    AddGlobalValueModalComponent
  ],
  entryComponents: []
})
export class SharedModule {}
