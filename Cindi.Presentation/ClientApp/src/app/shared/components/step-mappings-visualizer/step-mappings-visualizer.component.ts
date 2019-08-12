import { Component, OnInit, Input, OnChanges } from "@angular/core";
import { Edge, Node } from "@swimlane/ngx-graph";
import { id } from "@swimlane/ngx-charts/release/utils";

@Component({
  selector: "step-mappings-visualizer",
  templateUrl: "./step-mappings-visualizer.component.html",
  styleUrls: ["./step-mappings-visualizer.component.css"]
})
export class StepMappingsVisualizerComponent implements OnInit, OnChanges {
  ngOnChanges(changes: import("@angular/core").SimpleChanges): void {

    this.generateGraph();
  }
  @Input() subsequentStep;

  edges: Edge[] = [];
  nodes: Node[] = [];

  constructor() {}

  ngOnInit() {}

  generateGraph() {
    let nodes: Node[] = [];
    let edges: Edge[] = [];
    if (this.subsequentStep) {
      this.subsequentStep.mappings.forEach(mapping => {
        if (
          nodes.filter(n => n.id == "field_" + mapping.stepInputId).length == 0
        ) {
          nodes.push({
            id: "field_" + mapping.stepInputId,
            label: mapping.stepInputId,
            data: {
              defaultValue: mapping.defaultValue
                ? mapping.defaultValue.value
                : ""
            },
            meta: {
              type: "field"
            },
            dimension: {
              width: mapping.defaultValue
                ? ("" + mapping.defaultValue.value).length >
                  mapping.stepInputId.length
                  ? ("" + mapping.defaultValue.value).length * 15
                  : (mapping.stepInputId.length + 14) * 9
                : (mapping.stepInputId.length + 14) * 9,
              height: 50
            }
          });
        }

        if (mapping.outputReferences != undefined) {
          mapping.outputReferences.forEach(reference => {
            if (
              nodes.filter(n => n.id == "step_" + reference.workflowStepId)
                .length == 0
            ) {
              nodes.push({
                id: "step_" + reference.workflowStepId,
                label: reference.workflowStepId,
                meta: {
                  type: "step"
                }
              });
            }

            edges.push({
              id: id(),
              source: "step_" + reference.workflowStepId,
              target: "field_" + mapping.stepInputId
            });
          });
        }
      });
      this.nodes = nodes;
      this.edges = edges;
    }
  }
}
