import {
  Component,
  OnInit,
  EventEmitter,
  Output,
  Input,
  OnChanges
} from "@angular/core";
import { Graph, Node, Edge, Layout, ClusterNode } from "@swimlane/ngx-graph";
import * as shape from "d3-shape";
import { Subscription, Subject } from "rxjs";
import { id, colorSets } from "@swimlane/ngx-charts/release/utils";
import { Store } from "@ngrx/store";
import { State } from "../../../entities/workflow-templates/workflow-template.reducer";
import { NbWindowService } from "@nebular/theme";
import { WorkflowTemplate } from "../../../entities/workflow-templates/workflow-template.model";

@Component({
  selector: "workflow-visualizer",
  templateUrl: "./workflow-visualizer.component.html",
  styleUrls: ["./workflow-visualizer.component.scss"]
})
export class WorkflowVisualizerComponent implements OnInit, OnChanges {
  ngOnChanges(): void {}

  nodes: Node[] = [];
  edges: Edge[] = [];
  clusters: ClusterNode[];

  selectedStepTemplate: any;
  selectedStep: any;
  selectedLogicblock: any;
  selectedLogicBlockPreviousSteps: any;
  selectedLogicBlockPossibleMappings: any[] = [];
  workflowName: string;
  workflowVersion: number;

  public ngOnInit(): void {}
  @Output() selectedStepChange = new EventEmitter();
  @Output() onSave = new EventEmitter();

  constructor(
    private stepTemplateStore: Store<State>,
    private windowService: NbWindowService
  ) {
    this.generateGraph();
  }

  @Input() stepTemplates: any[] = [];
  @Input() workflow: any;

  @Input() workflowTemplate: WorkflowTemplate = {
    referenceId: "SimpleWorkflowWithInputs:1",
    name: "SimpleWorkflowWithInputs",
    version: 0,
    description: null,
    logicBlocks: {
      "0": {
        dependencies: {
          operator: "AND",
          conditions: {},
          conditionGroups: {}
        },
        subsequentSteps: {
          "0": {
            description: null,
            stepTemplateId: "Fibonacci_stepTemplate:0",
            mappings: {
              "n-1": {
                outputReferences: [
                  { stepName: "start", outputId: "n-1", priority: 0 }
                ],
                description: null,
                defaultValue: { priority: 99999999, value: 1 }
              },
              "n-2": {
                outputReferences: [
                  { stepName: "start", outputId: "n-2", priority: 0 }
                ],
                description: null,
                defaultValue: { priority: 99999999, value: 1 }
              }
            },
            isPriority: false
          }
        }
      },
      "1": {
        dependencies: {
          operator: "AND",
          conditions: {
            "0": {
              name: "StepStatus",
              comparer: "is",
              stepName: "0",
              stepTemplateReferenceId: null,
              status: "successful",
              statusCode: 0,
              description: null
            }
          },
          conditionGroups: {}
        },
        subsequentSteps: {
          "1": {
            description: null,
            stepTemplateId: "Fibonacci_stepTemplate:0",
            mappings: {
              "n-1": {
                outputReferences: [
                  { stepName: "0", outputId: "n", priority: 0 }
                ],
                description: null,
                defaultValue: { priority: 99999999, value: 1 }
              },
              "n-2": {
                outputReferences: [
                  { stepName: "0", outputId: "n", priority: 0 }
                ],
                description: null,
                defaultValue: { priority: 99999999, value: 1 }
              }
            },
            isPriority: false
          }
        }
      }
    },
    inputDefinitions: {
      "n-1": { description: null, type: "int" },
      "n-2": { description: null, type: "int" }
    },
    createdBy: "admin",
    createdOn: new Date("2019-08-21T20:14:16.42Z"),
    id: "e62a01aa-10ac-452a-9ba8-0be38d66e205"
  };

  @Input() editMode = false;

  /*getStepColour(workflowStepId: string) {
    var result = this.workflowTemplate.steps.filter(
      s => s.workflowStepId == workflowStepId
    );

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
  }*/

  updateInputDefinitions(newDefinitions) {
    this.workflowTemplate.inputDefinitions = newDefinitions;
    this.reloadSelectedLogicBlockMappings();
  }

  generateGraph() {
    let nodes: Node[] = [];
    let edges: Edge[] = [];

    nodes.push({
      id: "start",
      label: "Start",
      data: {},
      meta: {
        type: "start"
      }
    });

    Object.keys(this.workflowTemplate.logicBlocks).forEach(key => {
      this.getLogicBlockNodesAndEdges(
        nodes,
        edges,
        this.workflowTemplate.logicBlocks[key]
      );
    });

    this.addNewNodes(edges, nodes);

    this.clusters = [];
    Object.keys(this.workflowTemplate.logicBlocks).forEach(key => {
      this.clusters.push({
        id: id(),
        label: key,
        childNodeIds: Object.keys(
          this.workflowTemplate.logicBlocks[key].subsequentSteps
        ).map(key => key),
        data: this.workflowTemplate.logicBlocks[key]
      });
    });

    this.nodes = nodes;
    this.edges = edges;
  }

  getLogicBlockNodesAndEdges(nodes: Node[], edges: Edge[], logicBlock: any) {
    Object.keys(logicBlock.subsequentSteps).forEach(key => {
      if (nodes.filter(n => n.id == key).length == 0) {
        nodes.push({
          id: "" + key,
          label: key,
          dimension: {
            height: 30,
            width: logicBlock.subsequentSteps[key].stepTemplateId.length * 9
          },
          data: {
            step: logicBlock.subsequentSteps[key],
            logicBlock: logicBlock
          },
          meta: {
            type: "step"
          }
        });
      }

      //Add an edge for each of the steps without a start
      if (
        logicBlock.dependencies == null ||
        ((logicBlock.dependencies.conditions == undefined ||
          Object.keys(logicBlock.dependencies.conditions).length == 0) &&
          (logicBlock.dependencies.conditionGroups == undefined ||
            Object.keys(logicBlock.dependencies.conditionGroups).length == 0))
      ) {
        edges.push({
          id: id(), //dependenciesStep.workflowStepId + "-" + step.workflowStepId,
          source: "" + "start",
          target: "" + key
        });
      }
    });

    this.getConditionGroupNodesAndEdges(
      nodes,
      edges,
      logicBlock.dependencies,
      logicBlock.subsequentSteps
    );
  }

  getConditionGroupNodesAndEdges(
    nodes: Node[],
    edges: Edge[],
    conditionGroup: any,
    subsequentSteps: any
  ) {
    Object.keys(conditionGroup.conditions).forEach(key => {
      Object.keys(subsequentSteps).forEach(subsequentStepName => {
        edges.push({
          id: id(), //dependenciesStep.workflowStepId + "-" + step.workflowStepId,
          source: "" + conditionGroup.conditions[key].stepName,
          target: "" + subsequentStepName
        });
      });
    });

    Object.keys(conditionGroup.conditionGroups).forEach(key => {
      this.getConditionGroupNodesAndEdges(
        nodes,
        edges,
        conditionGroup.conditionGroups[key],
        subsequentSteps
      );
    });
  }

  addNewNodes(edges: Edge[], nodes: Node[]) {
    nodes.forEach(node => {
      let randomId = id();
      nodes.push({
        id: "" + randomId,
        label: "NEW",
        data: {},
        meta: {
          type: "new",
          parent: node.id
        }
      });

      edges.push({
        source: node.id,
        target: "" + randomId,
        label: "",
        id: id()
      });
    });
  }

  print() {
    console.log("pressed a SVG");
  }

  openWindow(contentTemplate, parentNode) {
    this.windowService.open(contentTemplate, {
      title: "Add logicBlock",
      context: {
        parentNode: parentNode
      }
    });
  }

  selectStep(node) {
    this.selectedLogicblock = node.data.logicBlock;
    let logicBlockSubsequentSteps = [];
    this.selectedLogicblock.subsequentSteps.forEach(element => {
      logicBlockSubsequentSteps.push("" + element.workflowStepId);
    });
    this.selectedStep = node.data.step;

    this.selectedLogicBlockPreviousSteps = this.nodes
      .filter(
        n =>
          n.meta.type == "step" && logicBlockSubsequentSteps.indexOf(n.id) == -1
      )
      .map(n => {
        return n.data.step;
      });

    this.selectedStepTemplate = this.stepTemplates.filter(
      st => st.referenceId == this.selectedStep.stepTemplateId
    )[0];

    this.reloadSelectedLogicBlockMappings();
  }

  reloadSelectedLogicBlockMappings() {
    this.selectedLogicBlockPossibleMappings = this.nodes
      .filter(n => n.meta.type == "step")
      .map(n => {
        return {
          stepTemplateId: n.data.step.stepTemplateId,
          stepRefId: n.id,
          mappings: this.getMappings(n.data.step.stepTemplateId)
        };
      });

    let workflowInputMappings = [];
    for (let prop of Object.keys(this.workflowTemplate.inputDefinitions)) {
      workflowInputMappings.push({
        name: prop,
        type: this.workflowTemplate.inputDefinitions[prop].type,
        description: this.workflowTemplate.inputDefinitions[prop].description
      });
    }

    this.selectedLogicBlockPossibleMappings.push({
      stepRefId: "start",
      mappings: workflowInputMappings
    });
  }

  getMappings(stepTemplateId: string) {
    let foundStepTemplate = this.stepTemplates.filter(
      st => st.referenceId == stepTemplateId
    )[0];
    let mappings = [];
    for (let prop of Object.keys(foundStepTemplate.outputDefinitions)) {
      mappings.push({
        name: prop,
        type: foundStepTemplate.outputDefinitions[prop].type,
        description: foundStepTemplate.outputDefinitions[prop].description
      });
    }

    return mappings;
  }

  addLogicBlock(node) {
    console.log(node);
    let newId = id();
    this.workflowTemplate.logicBlocks[id()].dependencies = {
      operator: "AND",
      conditions: {
        new_condition: {
          name: "StepStatus",
          comparer: "is",
          stepRefId: node.meta.parent,
          status: "successful",
          id: id()
        },
        conditionGroups: {}
      },
      subsequentSteps: {}
    };

    this.generateGraph();
  }

  openNewLogicBlockWindow(contentTemplate, node) {
    this.windowService.open(contentTemplate, {
      title: "",
      context: node.meta.parent
    });
  }

  openNewStepWindow(contentTemplate, logicBlock) {
    this.windowService.open(contentTemplate, {
      title: "",
      context: logicBlock
    });
  }

  addSubsequentStep(stepTemplate, logicblock) {
    console.log(stepTemplate);
    console.log(logicblock);

    logicblock.subsequentSteps[id()] = {
      description: undefined,
      isPriority: false,
      mappings: {},
      stepTemplateId: stepTemplate.referenceId
    };
    this.generateGraph();
  }

  addLogicBlockWithSubsequentStep(stepTemplate, parentId) {
    console.log(stepTemplate);

    console.log(parentId);
    let newId = id();
    let nextStepName = id();
    let newLogicBlock = {
      dependencies: {
        operator: "AND",
        conditions: [
          {
            name: "StepStatus",
            comparer: "is",
            workflowStepId: parentId,
            status: "successful",
            id: id()
          }
        ],
        conditionGroups: {}
      },
      subsequentSteps: {
        nextStepName :{
          description: undefined,
          isPriority: false,
          mappings: {},
          stepTemplateId: stepTemplate.referenceId
        }
      }
    };

    this.workflowTemplate.logicBlocks[id()] = newLogicBlock;

    this.generateGraph();
  }

  openSaveWindow(contentTemplate) {
    this.windowService.open(contentTemplate, {
      title: ""
    });
  }

  save() {
    this.onSave.emit(this.workflowTemplate);
  }
}
