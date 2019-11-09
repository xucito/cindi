import { NbWindowService } from "@nebular/theme";
import { Component, OnInit, Input, OnChanges } from "@angular/core";
import { Graph, Node, Edge, Layout } from "@swimlane/ngx-graph";
import { id } from "@swimlane/ngx-charts/release/utils";

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

  @Input() availableSteps: any[] = [ ];

  @Input() dependencies: any = {};

  generateGraph() {
    let nodes: Node[] = [];
    let edges: Edge[] = [];
    //nodes.push({ id: this.dependencies.id, label: "Start" });
    this.getNodesAndEdges(undefined, nodes, edges, "start", this.dependencies);

    this.edges = edges;
    this.nodes = nodes;
  }

  getNodesAndEdges(
    fromNode: string, //node you are creating the tree from
    existingNodes: Node[],
    existingEdges: Edge[],
    conditonGroupId: string,
    conditionGroup: any
  ): { nodes: Node[]; edges: Edge[] } {
    let nodes = existingNodes;
    let edges = existingEdges;

    //Add the condition group node
    nodes.push({
      id: conditonGroupId,
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
        parent: conditonGroupId,
        options: {
          steps: this.availableSteps
        }
      }
    });
    edges.push({
      id: newConditionId,
      source: conditonGroupId,
      target: newConditionId
    });

    //Add the button for new condition group
    let newConditionGroupId = this.generateId();
    nodes.push({
      id: newConditionGroupId,
      label: "Group",
      data: {
        type: "newConditionGroup",
        parent: conditonGroupId
      }
    });
    edges.push({
      id: newConditionGroupId,
      source: conditonGroupId,
      target: newConditionGroupId
    });

    if (fromNode !== undefined) {
      edges.push({
        id: conditonGroupId,
        source: fromNode,
        target: conditonGroupId
      });
    }
    if (conditionGroup.conditions) {
      Object.keys(conditionGroup.conditions).forEach(element => {
        let condition = conditionGroup.conditions[element];
        nodes.push({
          id: element,
          label: condition.name,
          data: {
            type: "condition",
            condition: condition,
            options: {
              steps: this.availableSteps
            }
          }
        });

        edges.push({
          id: element,
          source: conditonGroupId,
          target: element
        });
      });
    }

    if (conditionGroup.conditionGroups) {
      Object.keys(conditionGroup.conditionGroups).forEach(element => {
        this.getNodesAndEdges(
          conditonGroupId,
          nodes,
          edges,
          element,
          conditionGroup.conditionGroups[element]
        );
      });
    }

    return { nodes, edges };
  }

  ngOnInit() {}

  ToggleConditionGroupOperator(id: string, comparator: string) {
    this.SetConditionGroupOperator(id, comparator, this.dependencies);
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
    newConditionGroupId: any,
    newConditionGroup: any,
    conditionGroupId: any,
    conditionGroup: any
  ) {
    if (conditionGroupId == parentId) {
      //Create an empty array if it does not exist
      if (conditionGroup.conditionGroups == undefined) {
        conditionGroup.conditionGroups = {};
      }
      conditionGroup.conditionGroups[newConditionGroupId] = newConditionGroup;
      return true;
    }

    if (conditionGroup.conditionGroups != undefined) {
      Object.keys(conditionGroup.conditionGroups).forEach(
        conditionGroupName => {
          if (
            this.addConditionGroup(
              parentId,
              newConditionGroupId,
              newConditionGroup,
              conditionGroupName,
              conditionGroup.conditionGroups[conditionGroupName]
            )
          ) {
            return true;
          }
        }
      );
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
    this.SetCondition(event, this.dependencies);
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
    let newId = id();
    this.addConditionGroup(
      node.data.parent,
      newId,
      {
        id: node.id,
        conditions: {},
        conditionGroups: {},
        operator: selectedOption
      },
      "start",
      this.dependencies
    );
    /*node.data.parent.conditionGroups[id()] = {
      operator: "AND",
      conditions: {},
      conditionGroups: {}
    };*/
    this.generateGraph();
    /*
    console.log(node);
    if (
      this.addConditionGroup(
        node.data.parent,
        {
          id: node.id,
          conditions: {},
          conditionGroups: {},
          operator: selectedOption
        },
        id(),
        this.dependencies
      )
    ) {
      this.generateGraph();
      console.log("Successfully added condition Group");
    } else {
      console.error("Failed to add condition group.");
    }*/
  }

  addCondition(condition, parentId, conditionGroupId, conditionGroup) {
    if (conditionGroupId == parentId) {
      //Create an empty array if it does not exist
      if (conditionGroup.conditions == undefined) {
        conditionGroup.conditions = {};
      }
      conditionGroup.conditions[id()] = condition;
      return true;
    } else {
      if (conditionGroup.conditionGroups != undefined) {
        Object.keys(conditionGroup.conditionGroups).forEach(
          conditionGroupName => {
            if (
              this.addCondition(
                condition,
                parentId,
                conditionGroupName,
                conditionGroup.conditionGroups[conditionGroupName]
              )
            ) {
              return true;
            }
          }
        );
      }
    }
    return false;
  }
  addNewCondition(condition, event, parentId) {
    if (this.addCondition(event, parentId, "start", this.dependencies)) {
      console.log("Added new condition");
      this.generateGraph();
    } else {
      console.error("failed to add condition");
    }
  }

  deleteNode(node) {
    console.log(node);
    if (node.data.type == "condition") {
      if (this.removeCondition(node.id, this.dependencies)) {
        console.log("Sucessfully removed condition");
        this.generateGraph();
      } else {
        console.error("Failed to remove condition");
      }
    } else if (node.data.type == "conditionGroup") {
      if (this.removeConditionGroup(node.id, this.dependencies)) {
        console.log("Sucessfully removed condition");
        this.generateGraph();
      } else {
        console.error("Failed to remove condition");
      }
    }
  }

  removeCondition(id: string, conditionGroup) {
    for (var i = 0; i < conditionGroup.conditions.length; i++) {
      if (conditionGroup.conditions[i].id == id) {
        conditionGroup.conditions.splice(i, 1);
        return true;
      }
    }

    for (var i = 0; i < conditionGroup.conditionGroups.length; i++) {
      this.removeCondition(id, conditionGroup.conditionGroups[i]);
    }
  }

  removeConditionGroup(id: string, conditionGroup) {
    for (var i = 0; i < conditionGroup.conditionGroups.length; i++) {
      if (conditionGroup.conditionGroups[i].id == id) {
        conditionGroup.conditionGroups.splice(i, 1);
        return true;
      }
      this.removeCondition(id, conditionGroup.conditionGroups[i]);
    }
  }
}
