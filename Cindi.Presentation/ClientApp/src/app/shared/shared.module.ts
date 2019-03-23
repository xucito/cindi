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
  MatFormFieldModule
} from "@angular/material";
import {MatCardModule} from '@angular/material/card';
import { SequenceVisualizationComponent } from "./components/sequence-visualization/sequence-visualization.component";
import { SequenceTemplateVisualizationComponent } from "./components/sequence-template-visualization/sequence-template-visualization.component";
import { NgxGraphModule } from "../../../linked_projects/ngx-graph/src";
import { NgxChartsModule } from "@swimlane/ngx-charts";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { OutletComponent } from './components/outlet/outlet.component';
import { FormsComponent } from './components/form/form.component';
import { InputComponent } from './components/form/input/input.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { CodemirrorModule } from '@ctrl/ngx-codemirror';
import 'codemirror/mode/shell/shell';
import './modes/console';
import { StepProgressBarComponent } from './components/step-progress-bar/step-progress-bar.component';
import { UpdateViewerComponent } from './components/update-viewer/update-viewer.component';

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
    MatListModule
  ],
  exports: [
    SequenceTemplateVisualizationComponent,
    SequenceVisualizationComponent,
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
    UpdateViewerComponent
    //MatSidenavModule
  ],
  declarations: [
    SequenceVisualizationComponent,
    SequenceTemplateVisualizationComponent,
    OutletComponent,
    FormsComponent,
    InputComponent,
    StepProgressBarComponent,
    UpdateViewerComponent
  ]
})
export class SharedModule {}
