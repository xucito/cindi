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
  ngOnChanges(): void {}

  allStepTemplates: any[] = [];
  allStepTemplates$: Subscription;

  nodes: Node[] = [];
  edges: Edge[] = [];

  selectedLogicblock: any;
  selectedLogicBlockPreviousSteps: any;

  public ngOnInit(): void {}
  @Output() selectedStepChange = new EventEmitter();

  constructor(
    private stepTemplateStore: Store<State>,
    private windowService: NbWindowService
  ) {
    this.allStepTemplates$ = stepTemplateStore
      .pipe(select(selectAll))
      .subscribe(result => {
        this.allStepTemplates = result;
      });

    this.generateGraph();
  }

  @Input() workflowTemplate: WorkflowTemplate = new WorkflowTemplate();
  @Input() workflow: any = {
    referenceId: "SimpleWorkflow:1",
    name: "SimpleWorkflow",
    version: "1",
    description: null,
    logicBlocks: [
      {
        id: 0,
        prerequisites: {
          id: "59e45676-a39d-46df-97b3-f2bb7d25e949",
          operator: "AND",
          conditions: [],
          conditionGroups: []
        },
        subsequentSteps: [
          {
            description: null,
            stepTemplateId: "Fibonacci_stepTemplate:0",
            mappings: [
              {
                outputReferences: null,
                description: null,
                defaultValue: { priority: 99999999, value: 1 },
                stepInputId: "n-1"
              },
              {
                outputReferences: null,
                description: null,
                defaultValue: { priority: 99999999, value: 1 },
                stepInputId: "n-2"
              }
            ],
            isPriority: false,
            workflowStepId: 0
          }
        ]
      },
      {
        id: 1,
        prerequisites: {
          id: "a2321d96-203d-4670-9eeb-2aa78d3512fb",
          operator: "AND",
          conditions: [
            {
              name: "StepStatus",
              comparer: "is",
              workflowStepId: 0,
              stepTemplateReferenceId: null,
              status: "successful",
              statusCode: 0,
              id: "716055cf-b603-4219-a9bf-e67e9c99f822",
              description: null
            }
          ],
          conditionGroups: []
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
    ],
    inputDefinitions: null,
    createdBy: "admin",
    createdOn: "2019-08-12T11:34:28.275Z",
    id: "d687417e-976a-4efa-859d-5c504d8c3962",
    shardId: "e51da395-24a8-41df-b8a8-9bec3952b708",
    shardType: "WorkflowTemplate",
    data: null,
    className: "Cindi.Domain.Entities.WorkflowsTemplates.WorkflowTemplate"
  };
  @Input() stepTemplates: any[] = [];
  @Input() editMode = false;

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

  generateGraph() {
    let nodes: Node[] = [];
    let edges: Edge[] = [];

    nodes.push({
      id: "" + 0,
      label: "Start",
      data: {},
      meta: {
        type: "start"
      }
    });

    this.workflow.logicBlocks.forEach(logicBlock => {
      this.getLogicBlockNodesAndEdges(nodes, edges, logicBlock);
    });

    this.addNewNodes(edges, nodes);

    this.nodes = nodes;
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
    });

    this.getConditionGroupNodesAndEdges(
      nodes,
      edges,
      logicBlock.prerequisites,
      logicBlock.subsequentSteps
    );
  }

  getConditionGroupNodesAndEdges(
    nodes: Node[],
    edges: Edge[],
    conditionGroup: any,
    subsequentSteps: any[]
  ) {
    conditionGroup.conditions.forEach(prerequisiteStep => {
      subsequentSteps.forEach(step => {
        edges.push({
          id: id(), //prerequisiteStep.workflowStepId + "-" + step.workflowStepId,
          source: "" + prerequisiteStep.workflowStepId,
          target: "" + step.workflowStepId
        });
      });
    });

    conditionGroup.conditionGroups.forEach(conditionGroup => {
      this.getConditionGroupNodesAndEdges(
        nodes,
        edges,
        conditionGroup,
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
          type: "new"
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
      logicBlockSubsequentSteps.push(element.workflowStepId);
    });

    this.selectedLogicBlockPreviousSteps = this.nodes
      .filter(
        n =>
          n.meta.type == "step" && logicBlockSubsequentSteps.indexOf(n.id) > -1
      )
      .map(n => {
        return {
          workflowStepId: n.id
        };
      });
  }
}
