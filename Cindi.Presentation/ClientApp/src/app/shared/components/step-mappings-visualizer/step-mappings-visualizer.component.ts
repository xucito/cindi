import { NbWindowService } from "@nebular/theme";
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
  @Input() stepTemplate;
  @Input() allPossibleMappings: any[] = [
    {
      stepTemplateName: "Fibonacci_stepTemplate:0",
      stepRefId: 0,
      mappings: [
        { name: "n", description: null, type: "int" },
        { name: "n-1", description: null, type: "int" }
      ]
    },
    {
      stepTemplateName: "Fibonacci_stepTemplate:0",
      stepRefId: 1,
      mappings: [
        { name: "string-example", description: null, type: "string" },
        { name: "n-1", description: null, type: "int" }
      ]
    }
  ];

  edges: Edge[] = [];
  nodes: Node[] = [];

  constructor(private windowService: NbWindowService) {}

  ngOnInit() {}

  generateGraph() {
    let nodes: Node[] = [];
    let edges: Edge[] = [];
    if (this.subsequentStep && this.stepTemplate) {
      Object.keys(this.stepTemplate.inputDefinitions).forEach(prop => {

        let foundMapping = [];
        let foundMappingKeys = Object.keys(this.subsequentStep.mappings).forEach(
          m =>
          {
            if(this.subsequentStep.mappings[m].stepInputId == prop)
            {
              foundMapping.push(this.subsequentStep.mappings[m])
            }
          }
        );



        nodes.push({
          id: "field_" + prop,
          label: prop,
          data: {
            mapping:
              foundMapping.length > 0
                ? foundMapping[0]
                : {
                    outputReferences: [],
                    description: null,
                    stepInputId: prop
                  }
          },
          meta: {
            type: "field"
          },
          dimension: {
            width: 200,
            height: 50
          }
        });
      });
      Object.keys(this.subsequentStep.mappings).forEach(mappingKey => {
        let mapping = this.subsequentStep.mappings[mappingKey];
        /*if (
          nodes.filter(n => n.id == "field_" + mapping.stepInputId).length == 0
        ) {
          nodes.push({
            id: "field_" + mapping.stepInputId,
            label: mapping.stepInputId,
            data: {
              defaultValue: mapping.defaultValue
                ? mapping.defaultValue.value
                : "",
              mapping: mapping
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
        }*/

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

  openStepMappingWindow(contentTemplate, data) {
    console.log(data);

    let mappingType = "int"; //Get the type of the mapping
    const filteredMappings = JSON.parse(
      JSON.stringify(this.allPossibleMappings)
    );
    filteredMappings.forEach(
      fm => (fm.mappings = fm.mappings.filter(map => map.type == mappingType))
    );

    this.windowService.open(contentTemplate, {
      title: "Edit mapping",
      context: {
        mapping: data.mapping,
        options: filteredMappings
      }
    });
  }

  updateMapping(mapping) {
    console.log(mapping);

    if (
      !this.subsequentStep.mappings ||
      this.subsequentStep.mappings.length == 0
    ) {
      this.subsequentStep.mappings = [];
      this.subsequentStep.mappings.push(mapping);
    } else {
      let updatedMapping = false;
      for (var i = 0; i < this.subsequentStep.mappings; i++) {
        if (
          this.subsequentStep.mappings[i].stepInputId == mapping.stepInputId
        ) {
          this.subsequentStep.mappings[i] = mapping;
          updatedMapping = true;
        }
      }

      if (!updatedMapping) {
        this.subsequentStep.mappings.push(mapping);
      }
    }

    this.generateGraph();
  }
}
