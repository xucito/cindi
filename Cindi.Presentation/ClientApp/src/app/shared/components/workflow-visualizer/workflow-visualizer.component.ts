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
  highestSubsequentStepRefId = 0;
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
    version: 1,
    description: null,
    logicBlocks: {
      0: {
        dependencies: {
          id: "97dae226-98ae-4f8e-b2b7-bc54216bfa21",
          operator: "AND",
          conditions: {},
          conditionGroups: {}
        },
        subsequentSteps:[
          {
            description: null,
            stepTemplateId: "Fibonacci_stepTemplate:0",
            mappings: [
              {
                outputReferences: [
                  { workflowStepId: -1, outputId: "n-1", priority: 0 }
                ],
                description: null,
                defaultValue: { priority: 99999999, value: 1 },
                stepInputId: "n-1"
              },
              {
                outputReferences: [
                  { workflowStepId: -1, outputId: "n-2", priority: 0 }
                ],
                description: null,
                defaultValue: { priority: 99999999, value: 1 },
                stepInputId: "n-2"
              }
            ],
            isPriority: false,
            workflowStepId: 0
          }]
      },
      1: {
        dependencies: {
          id: "61860b31-4b3c-4332-b5c5-0e1f095154ff",
          operator: "AND",
          conditions: {
           "0": {
              name: "StepStatus",
              comparer: "is",
              workflowStepId: 0,
              stepTemplateReferenceId: null,
              status: "successful",
              statusCode: 0,
              id: "c7e9ca0d-c15b-4d5f-aea2-fc1b099de35c",
              description: null
            }
          },
          conditionGroups: {}
        },
        subsequentSteps: [
          {
            description: null,
            stepTemplateId: "Fibonacci_stepTemplate:0",
            mappings: [
              {
                outputReferences: [
                  { workflowStepId: 0, outputId: "n", priority: 0 }
                ],
                description: null,
                defaultValue: { priority: 99999999, value: 1 },
                stepInputId: "n-1"
              },
              {
                outputReferences: [
                  { workflowStepId: 0, outputId: "n", priority: 0 }
                ],
                description: null,
                defaultValue: { priority: 99999999, value: 1 },
                stepInputId: "n-2"
              }
            ],
            isPriority: false,
            workflowStepId: 1
          }
        ]
      }
    },
    inputDefinitions: {
      "n-1": { description: null, type: "int" },
      "n-2": { description: null, type: "int" }
    },
    createdBy: "admin",
    createdOn: new Date("2019-08-13T09:12:13.819Z"),
    id: "2e9df69f-411f-4b06-8f10-1e9998d57c82"
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
      id: "" + -1,
      label: "Start",
      data: {},
      meta: {
        type: "start"
      }
    });

    Object.keys(this.workflowTemplate.logicBlocks).forEach(key => {
      this.getLogicBlockNodesAndEdges(nodes, edges, this.workflowTemplate.logicBlocks[key]);
    });

    this.addNewNodes(edges, nodes);

    this.clusters = [];
    Object.keys(this.workflowTemplate.logicBlocks).forEach(key => {
      this.clusters.push({
        id: id(),
        label: key,
        childNodeIds: this.workflowTemplate.logicBlocks[key].subsequentSteps.map(
          ss => "" + ss.workflowStepId
        ),
        data: this.workflowTemplate.logicBlocks[key]
      });
    });

    this.nodes = nodes;

    this.highestSubsequentStepRefId = this.getHighestWorkflowStepId();
    this.edges = edges;
  }

  getLogicBlockNodesAndEdges(nodes: Node[], edges: Edge[], logicBlock: any) {
    logicBlock.subsequentSteps.forEach(step => {
      if (nodes.filter(n => n.id == step.workflowStepId).length == 0) {
        nodes.push({
          id: "" + step.workflowStepId,
          label: step.workflowStepId,
          dimension: {
            height: 30,
            width: step.stepTemplateId.length * 9
          },
          data: {
            step: step,
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
          logicBlock.dependencies.conditions.length == 0) &&
          (logicBlock.dependencies.conditionGroups == undefined ||
            logicBlock.dependencies.conditionGroups.length == 0))
      ) {
        edges.push({
          id: id(), //dependenciesStep.workflowStepId + "-" + step.workflowStepId,
          source: "" + -1,
          target: "" + step.workflowStepId
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
    subsequentSteps: any[]
  ) {
    Object.keys(conditionGroup.conditions).forEach(key => {
      subsequentSteps.forEach(step => {
        edges.push({
          id: id(), //dependenciesStep.workflowStepId + "-" + step.workflowStepId,
          source: "" + conditionGroup.conditions[key].workflowStepId,
          target: "" + step.workflowStepId
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
      stepRefId: "" + -1,
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
    this.workflowTemplate.logicBlocks[id()] = {
      dependencies: {
        id: id(),
        operator: "AND",
        conditions: [
          {
            name: "StepStatus",
            comparer: "is",
            stepRefId: node.meta.parent,
            status: "successful",
            id: id()
          }
        ],
        conditionGroups: []
      },
      subsequentSteps: [{}]
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

    logicblock.subsequentSteps.push({
      description: undefined,
      isPriority: false,
      mappings: [],
      stepTemplateId: stepTemplate.referenceId,
      workflowStepId: this.getHighestWorkflowStepId() + 1
    });
    this.generateGraph();
  }

  addLogicBlockWithSubsequentStep(stepTemplate, parentId) {
    console.log(stepTemplate);

    console.log(parentId);

    let newLogicBlock = {
      dependencies: {
        id: id(),
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
        conditionGroups: []
      },
      subsequentSteps: [
        {
          description: undefined,
          isPriority: false,
          mappings: [],
          stepTemplateId: stepTemplate.referenceId,
          workflowStepId: this.getHighestWorkflowStepId() + 1
        }
      ]
    };

    this.workflowTemplate.logicBlocks[id()] = newLogicBlock;

    this.generateGraph();
  }

  getHighestWorkflowStepId() {
    return +this.nodes
      .filter(n => n.meta.type == "step")
      .sort((a, b) => (+a.id < +b.id ? 1 : -1))[0].id;
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
