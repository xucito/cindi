import { NbWindowService } from "@nebular/theme";
import {
  Component,
  OnInit,
  EventEmitter,
  Output,
  Input,
  OnChanges
} from "@angular/core";
import { Graph, Node, Edge, Layout } from "@swimlane/ngx-graph";
import * as shape from "d3-shape";
import { Subscription, Subject } from "rxjs";
import { id, colorSets } from "@swimlane/ngx-charts/release/utils";
import { Store, select } from "@ngrx/store";
import { State } from "../../entities/workflow-templates/workflow-template.reducer";
import { WorkflowTemplate } from "../../entities/workflow-templates/workflow-template.model";
import { selectAll } from "../../entities/step-templates/step-template.reducer";

@Component({
  selector: "workflow-designer",
  templateUrl: "./workflow-designer.component.html",
  styleUrls: ["./workflow-designer.component.css"]
})
export class WorkflowDesignerComponent implements OnInit, OnChanges {
  ngOnChanges(changes: import("@angular/core").SimpleChanges): void {
    throw new Error("Method not implemented.");
  }
  ngOnInit(): void {
    throw new Error("Method not implemented.");
  }
}
