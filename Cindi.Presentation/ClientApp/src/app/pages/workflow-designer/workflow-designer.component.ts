import { CindiClientService } from "./../../services/cindi-client.service";
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
import { WorkflowTemplate } from "../../entities/workflow-templates/workflow-template.model";
import {
  selectAll,
  State
} from "../../entities/step-templates/step-template.reducer";

@Component({
  selector: "workflow-designer",
  templateUrl: "./workflow-designer.component.html",
  styleUrls: ["./workflow-designer.component.css"]
})
export class WorkflowDesignerComponent implements OnInit, OnChanges {
  allStepTemplates$: Subscription;
  allStepTemplates: any[];

  ngOnChanges(changes: import("@angular/core").SimpleChanges): void {
    throw new Error("Method not implemented.");
  }
  ngOnInit(): void {
    throw new Error("Method not implemented.");
  }

  constructor(
    private store: Store<State>,
    private cindiClient: CindiClientService
  ) {
    this.allStepTemplates$ = store.pipe(select(selectAll)).subscribe(result => {
      this.allStepTemplates = result;
    });
  }

  save$: Subscription;

  saveWorkflow(workflow) {
    console.log(workflow);
    this.save$ = this.cindiClient
      .PostWorkflowTemplate(workflow)
      .subscribe(result => {
        console.log("Save successful");
      });
  }
}
