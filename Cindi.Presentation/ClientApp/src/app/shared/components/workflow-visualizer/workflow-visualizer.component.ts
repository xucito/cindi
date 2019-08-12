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
import { Store } from "@ngrx/store";
import { State } from "../../../entities/workflow-templates/workflow-template.reducer";

@Component({
  selector: "workflow-visualizer",
  templateUrl: "./workflow-visualizer.component.html",
  styleUrls: ["./workflow-visualizer.component.scss"]
})
export class WorkflowVisualizerComponent implements OnInit, OnChanges {
  ngOnChanges(changes: import("@angular/core").SimpleChanges): void {
    this.hierarchialGraph = this.generateGraph();
    this.updateChart();
  }
  name = "Angular 5";
  hierarchialGraph: Graph;
  curve = shape.curveLinear; //shape.curveBundle.beta(1);
  view = undefined;
  stepTemplates$: Subscription;
  maxStringLength = 20;
  // curve = shape.curveLinear;
  colorScheme;
  public ngOnInit(): void {}
  update$: Subject<any> = new Subject();

  @Output() selectedStepChange = new EventEmitter();

  constructor(private stepTemplateStore: Store<State>) {
    this.setColorScheme("vivid");
  }

  @Input() workflowTemplate: any;
  @Input() workflow: any;
  @Input() stepTemplates: any[] = [];
  @Input() editMode = false;

  setColorScheme(name) {
    this.colorScheme = colorSets.find(s => s.name === name);
  }

  updateChart() {
    this.update$.next(true);
  }

  getStepColour(workflowStepId: string) {
    var result = this.workflow.steps.filter(s => s.workflowStepId == workflowStepId);

    if (result.length == 0) {
      return "#BCBFBF";
    }
    var status = result[0].status;
    switch (status) {
      case "successful":
        return "#00FF13";
      case "unassigned":
        return "#FFFF00";
      case "assigned":
        return "#009EFF";
      case "error":
        return "#ff0000";
      default:
        return "#BCBFBF";
    }
  }

  getStep(workflowStepId: number) {
    if (this.workflow == undefined) {
      return undefined;
    }
    var result = this.workflow.steps.filter(s => s.workflowStepId == workflowStepId);

    if (result.length == 0) {
      return undefined;
    } else {
      return result[0];
    }
  }

  selectStep(node: any) {
    console.log(node);
    this.selectedStepChange.emit(node);
  }

  generateGraph(): Graph {
    if (this.workflowTemplate != undefined) {
      //this.hierarchialGraph.nodes = [];
      //this.hierarchialGraph.links = [];
      //store all the steps that will be in this template
      let nodes: Node[] = [];
      let edges: Edge[] = [];
      let posCount = 0;

      nodes.push({
        id: "-1",
        label: "start",
        data: {
          color: "#3f51b5"
        }
      });

      posCount++;

      this.workflowTemplate.logicBlocks.forEach(block => {
        let noPrerequisites = block.prerequisiteSteps.length == 0;
        block.subsequentSteps.forEach(substep => {
          if (noPrerequisites) {
            edges.push({
              source: "-1",
              target: "" + substep.workflowStepId,
              label: "",
              id: id()
            });
          }
          if (nodes.filter(n => n.id == substep.workflowStepId).length == 0) {
            var foundStepTemplate = this.stepTemplates.filter(
              st => st.referenceId == substep.stepTemplateId
            )[0];
            let step = this.getStep(substep.workflowStepId);
            if (foundStepTemplate == undefined) {
              nodes.push({
                id: "" + substep.workflowStepId,
                label: substep.stepTemplateId,
                data: {
                  color: "#3f51b5",
                  mappings: substep.mappings
                },
                dimension: {
                  width: 300,
                  height: 100
                }
              });
            } else {
              nodes.push({
                id: "" + substep.workflowStepId,
                label: foundStepTemplate.name,
                data: {
                  value: foundStepTemplate,
                  color: "#3f51b5",
                  mappings: substep.mappings
                }
              });
            }
          }

          block.prerequisiteSteps.forEach(preStep => {
            edges.push({
              source: "" + preStep.workflowStepId,
              target: "" + substep.workflowStepId,
              label: preStep.status + "|" + preStep.statusCode,
              id: id()
            });
          });

          posCount++;
        });
      });
      return { edges, nodes };
    }
  }

  print()
  {
    console.log("pressed a SVG")
  }
}
