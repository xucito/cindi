import { NbWindowService } from "@nebular/theme";
import { Component, OnInit, Input, OnChanges } from "@angular/core";
import { Graph, Node, Edge, Layout } from "@swimlane/ngx-graph";

@Component({
  selector: "conditions-group-visualizer",
  templateUrl: "./conditions-group-visualizer.component.html",
  styleUrls: ["./conditions-group-visualizer.component.css"]
})
export class ConditionsGroupVisualizerComponent implements OnInit, OnChanges {
  ngOnChanges(): void {
    this.generateGraph();
  }

  constructor(private windowService: NbWindowService) {
    this.generateGraph();
  }

  edges: Edge[] = [];
  nodes: Node[] = [];
  options: any;
  nextId: number;

  @Input() availableSteps: any[] = [
    {
      workflowStepId: 0
    },
    {
      workflowStepId: 1
    }
  ];

  @Input() prerequisites: any = {
    id: "1",
    operator: "AND",
    conditions: [
      {
        name: "StepStatus",
        comparer: "is",
        workflowStepId: 0,
        stepTemplateReferenceId: null,
        status: "successful",
        id: "2",
        description: null
      }
    ],
    conditionGroups: [
      {
        id: "3",
        operator: "AND",
        conditions: [
          {
            name: "StepStatus",
            comparer: "is",
            workflowStepId: 0,
            stepTemplateReferenceId: null,
            status: "successful",
            statusCode: 0,
            id: "4",
            description: null
          }
        ],
        conditionGroups: [
          {
            id: "7",
            operator: "AND",
            conditions: [
              {
                name: "StepStatus",
                comparer: "is",
                workflowStepId: 0,
                stepTemplateReferenceId: null,
                status: "successful",
                statusCode: 0,
                id: "8",
                description: null
              }
            ]
          }
        ]
      },
      {
        id: "5",
        operator: "AND",
        conditions: [
          {
            name: "StepStatus",
            comparer: "is",
            workflowStepId: 0,
            stepTemplateReferenceId: null,
            status: "successful",
            statusCode: 0,
            id: "6",
            description: null
          }
        ]
      }
    ]
  };

  generateGraph() {
    let nodes: Node[] = [];
    let edges: Edge[] = [];
    //nodes.push({ id: this.prerequisites.id, label: "Start" });
    this.getNodesAndEdges(undefined, nodes, edges, this.prerequisites);

    this.edges = edges;
    this.nodes = nodes;
  }

  getNodesAndEdges(
    fromNode: string, //node you are creating the tree from
    existingNodes: Node[],
    existingEdges: Edge[],
    conditionGroup: any
  ): { nodes: Node[]; edges: Edge[] } {
    let nodes = existingNodes;
    let edges = existingEdges;

    //Add the condition group node
    nodes.push({
      id: conditionGroup.id,
      label: "Group",
      data: {
        type: "conditionGroup",
        operator: conditionGroup.operator
      }
    });

    //Add the button for new condition
    let newConditionId = this.generateId();
    nodes.push({
      id: newConditionId,
      label: "Group",
      data: {
        type: "newCondition",
        parent: conditionGroup.id,
        options: {
          steps: this.availableSteps
        }
      }
    });
    edges.push({
      id: newConditionId,
      source: conditionGroup.id,
      target: newConditionId
    });

    //Add the button for new condition group
    let newConditionGroupId = this.generateId();
    nodes.push({
      id: newConditionGroupId,
      label: "Group",
      data: {
        type: "newConditionGroup",
        parent: conditionGroup.id
      }
    });
    edges.push({
      id: newConditionGroupId,
      source: conditionGroup.id,
      target: newConditionGroupId
    });

    if (fromNode !== undefined) {
      edges.push({
        id: conditionGroup.id,
        source: fromNode,
        target: conditionGroup.id
      });
    }
    if (conditionGroup.conditions) {
      conditionGroup.conditions.forEach(element => {
        nodes.push({
          id: element.id,
          label: element.name,
          data: {
            type: "condition",
            condition: element,
            options: {
              steps: this.availableSteps
            }
          }
        });

        edges.push({
          id: element.id,
          source: conditionGroup.id,
          target: element.id
        });
      });
    }

    if (conditionGroup.conditionGroups) {
      conditionGroup.conditionGroups.forEach(element => {
        this.getNodesAndEdges(conditionGroup.id, nodes, edges, element);
      });
    }

    return { nodes, edges };
  }

  ngOnInit() {}

  ToggleConditionGroupOperator(id: string, comparator: string) {
    this.SetConditionGroupOperator(id, comparator, this.prerequisites);
    this.generateGraph();
  }

  SetConditionGroupOperator(
    id: string,
    comparator: string,
    conditionGroup: any
  ) {
    if (conditionGroup.id == id) {
      conditionGroup.operator = comparator;
      return true;
    } else {
      if (conditionGroup.conditionGroups != undefined) {
        for (var i = 0; i < conditionGroup.conditionGroups.length; i++) {
          if (
            this.SetConditionGroupOperator(
              id,
              comparator,
              conditionGroup.conditionGroups[i]
            )
          ) {
            return true;
          }
        }
      }
    }
    return false;
  }

  addConditionGroup(
    parentId: string,
    newConditionGroup: any,
    conditionGroup: any
  ) {
    if (conditionGroup.id == parentId) {
      //Create an empty array if it does not exist
      if (conditionGroup.conditionGroups == undefined) {
        conditionGroup.conditionGroups = [];
      }
      conditionGroup.conditionGroups.push(newConditionGroup);
      return true;
    } else {
      if (conditionGroup.conditionGroups != undefined) {
        for (var i = 0; i < conditionGroup.conditionGroups.length; i++) {
          if (
            this.addConditionGroup(
              parentId,
              newConditionGroup,
              conditionGroup.conditionGroups[i]
            )
          ) {
            return true;
          }
        }
      }
    }
    return false;
  }

  SetCondition(newCondition: any, conditionGroup: any) {
    for (var i = 0; i < conditionGroup.conditions.length; i++) {
      if (conditionGroup.conditions[i].id == newCondition.id) {
        conditionGroup.conditions[i] = newCondition;
        return true;
      }
    }

    if (conditionGroup.conditionGroups != undefined) {
      for (var i = 0; i < conditionGroup.conditionGroups.length; i++) {
        if (
          this.SetCondition(newCondition, conditionGroup.conditionGroups[i])
        ) {
          return true;
        }
      }
    }

    return false;
  }

  openWindow(contentTemplate, parentNode) {
    this.windowService.open(contentTemplate, {
      title: "Add logicBlock",
      context: parentNode
    });
  }

  updateCondition(event, data) {
    console.log(event);
    console.log(data);
    event.id = data.id;
    this.SetCondition(event, this.prerequisites);
    this.generateGraph();
  }

  generateId() {
    return (
      "_" +
      Math.random()
        .toString(36)
        .substr(2, 9)
    );
  }

  addNewConditionGroup(node, selectedOption) {
    console.log(node);
    if (
      this.addConditionGroup(
        node.data.parent,
        {
          id: node.id,
          conditions: [],
          conditionGroups: [],
          operator: selectedOption
        },
        this.prerequisites
      )
    ) {
      this.generateGraph();
      console.log("Successfully added condition Group");
    } else {
      console.error("Failed to add condition group.");
    }
  }

  addCondition(condition, parentId, conditionGroup)
  {

    if (conditionGroup.id == parentId) {
      //Create an empty array if it does not exist
      if (conditionGroup.conditions == undefined) {
        conditionGroup.conditions = [];
      }
      conditionGroup.conditions.push(condition);
      return true;
    } else {
      if (conditionGroup.conditionGroups != undefined) {
        for (var i = 0; i < conditionGroup.conditionGroups.length; i++) {
          if (
            this.addCondition(
              condition,
              parentId,
              conditionGroup.conditionGroups[i]
            )
          ) {
            return true;
          }
        }
      }
    }
    return false;
  }
  addNewCondition(newConditionId,condition, parentId)
  {
    condition.id = newConditionId;
    if(this.addCondition(condition, parentId, this.prerequisites))
    {
      console.log("Added new condition");
      this.generateGraph();
    }
    else
    {
      console.error("failed to add condition")
    }
  }
}
