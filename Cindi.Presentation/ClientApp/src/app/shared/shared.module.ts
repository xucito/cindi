import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  MatMenuModule,
  MatSidenavModule,
  MatButtonModule,
  MatTableModule,
  MatIconModule
} from "@angular/material";
import { SequenceVisualizationComponent } from "./components/sequence-visualization/sequence-visualization.component";
import { SequenceTemplateVisualizationComponent } from "./components/sequence-template-visualization/sequence-template-visualization.component";
import { NgxGraphModule } from "../../../linked_projects/ngx-graph/src";
import { NgxChartsModule } from "@swimlane/ngx-charts";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { OutletComponent } from './components/outlet/outlet.component';
import { FormsComponent } from './components/form/form.component';
import { InputComponent } from './components/form/input/input.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
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
    FormsModule,
    ReactiveFormsModule
  ],
  exports: [
    SequenceTemplateVisualizationComponent,
    SequenceVisualizationComponent,
    MatButtonModule,
    MatMenuModule,
    MatTableModule,
    MatIconModule,
    FormsComponent
    //MatSidenavModule
  ],
  declarations: [
    SequenceVisualizationComponent,
    SequenceTemplateVisualizationComponent,
    OutletComponent,
    FormsComponent,
    InputComponent
  ]
})
export class SharedModule {}
