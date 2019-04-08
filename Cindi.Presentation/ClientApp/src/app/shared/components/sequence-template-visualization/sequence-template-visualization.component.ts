import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";
import * as shape from "d3-shape";
import { AppStateService } from "../../../services/app-state.service";
import { Subscription, BehaviorSubject, Subject } from "rxjs";
import {
  Graph,
  Node,
  Edge,
  Layout
} from "../../../../../linked_projects/ngx-graph/src/ngx-graph.module";
import { colorSets } from "../../../../../linked_projects/ngx-graph/src/utils/color-sets";
import {
  countries,
  generateGraph
} from "../../../../../linked_projects/ngx-graph/demo/data";
import { id } from "../../../../../linked_projects/ngx-graph/src/utils";

@Component({
  selector: "cindi-sequence-template-visualization",
  templateUrl: "./sequence-template-visualization.component.html",
  styleUrls: ["./sequence-template-visualization.component.scss"]
})
export class SequenceTemplateVisualizationComponent implements OnInit {
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

  private _sequenceTemplate;

  constructor(private _appState: AppStateService) {
    //this.showGraph();
    this.setColorScheme("vivid");
  }

  @Input()
  set sequenceTemplate(template) {
    this._sequenceTemplate = template;

    this.stepTemplates$ = this._appState.allStepTemplates.subscribe(result => {
      //this.generateGraph();
      this.hierarchialGraph = this.generateGraph();
      this.updateChart();
    });
  }

  setColorScheme(name) {
    this.colorScheme = colorSets.find(s => s.name === name);
  }

  _sequence: any;
  @Input()
  set Sequence(sequence) {
    this._sequence = sequence;
    this.hierarchialGraph = this.generateGraph();
    this.updateChart();
  }

  updateChart() {
    this.update$.next(true);
  }

  getStepColour(stepRefId: string) {
    var result = this._sequence.steps.filter(s => s.stepRefId == stepRefId);

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

  getStep(stepRefId: number) {
    if (this._sequence == undefined) {
      return undefined;
    }
    var result = this._sequence.steps.filter(s => s.stepRefId == stepRefId);

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
    if (this._sequenceTemplate != undefined) {
      //this.hierarchialGraph.nodes = [];
      //this.hierarchialGraph.links = [];
      //store all the steps that will be in this template
      let nodes: Node[] = [];
      let edges: Edge[] = [];
      let posCount = 0;

      /* var foundStepTemplate = this._appState.getStepTemplateDef(
        this._sequenceTemplate.startingStepTemplateReference
      );*/

      nodes.push({
        id: "-1",
        label: "start",
        data: {
          color: "#3f51b5"
        }
      });

      /*
      let firstStep = this.getStep(0);

      nodes.push({
        id: "0",
        label:
          (foundStepTemplate == null ? "-1" : foundStepTemplate.name + " | 0") + (firstStep != undefined ? "| step: " + firstStep.id : ""),
        data: {
          color: "#3f51b5"
        }
      });
*/
      /*edges.push({
        source: "-1",
        target: "0",
        label: "",
        id: id()
      });*/


      posCount++;

      this._sequenceTemplate.logicBlocks.forEach(block => {
        let noPrerequisites = block.prerequisiteSteps.length == 0;
        block.subsequentSteps.forEach(substep => {
          if(noPrerequisites)
          {
            edges.push({
              source: "-1",
              target: "" + substep.stepRefId,
              label: "",
              id: id()
          })};
          if (nodes.filter(n => n.id == substep.stepRefId).length == 0) {
            var foundStepTemplate = this._appState.getStepTemplateDef(
              substep.stepTemplateId
            );
            let step = this.getStep(substep.stepRefId);
            if (foundStepTemplate == undefined) {
              nodes.push({
                id: "" + substep.stepRefId,
                label:
                  "" +
                  substep.stepTemplateId + "|" +
                  substep.stepRefId +
                  (step != undefined ? "| step: " + step.id : ""),
                data: {
                  color: "#3f51b5"
                }
              });
            } else {
              nodes.push({
                id: "" + substep.stepRefId,
                label:
                  "" +
                  foundStepTemplate.name +
                  " | " +
                  substep.stepRefId +
                  (step != undefined ? " | step: " + step.id : ""),
                data: {
                  value: foundStepTemplate,
                  color: "#3f51b5"
                }
              });
            }
          }

          block.prerequisiteSteps.forEach(preStep => {
            edges.push({
              source: "" + preStep.stepRefId,
              target: "" + substep.stepRefId,
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
}
