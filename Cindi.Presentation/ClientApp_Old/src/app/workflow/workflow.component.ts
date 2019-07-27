import { Component, OnInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { AppStateService } from "../services/app-state.service";
import { NodeDataService } from "../services/node-data.service";
import { Subscription } from "rxjs";

@Component({
  selector: "app-workflow",
  templateUrl: "./workflow.component.html",
  styleUrls: ["./workflow.component.css"]
})
export class WorkflowComponent implements OnInit, OnDestroy {
  selectedId;
  public template;
  public workflow;
  public template$: Subscription;
  public workflow$: Subscription;
  public workflowInputs: any[] = [];
  run: any;
  runStep: any;
  selectedStepId: string;
  public selectedStepInputs: any[] = [];
  public selectedStepOutputs: any[] = [];

  columnsToDisplay = ["id", "value"];

  constructor(
    private _route: ActivatedRoute,
    private _appState: AppStateService,
    private _nodeData: NodeDataService
  ) {
    _route.params.subscribe(p => {
      this.selectedId = p.id;
    });
  }

  ngOnInit() {
    const _thisObject = this;
    this.updateData();
    this.run = setInterval(function() {
      _thisObject.updateData();
      console.log("Just refreshed steps");
    }, 30000);
  }

  ngOnDestroy() {
    this.template$.unsubscribe();
    this.workflow$.unsubscribe();
  }

  getType(type: number) {
    switch (type) {
      case 0:
        return "bool";
      case 1:
        return "string";
      case 2:
        return "int";
      case 3:
        return "object";
    }
  }

  selectStep(event) {
    console.log("catch");

    this.selectedStepId = event.id;

    if (this.workflow != undefined) {
      let filteredSteps: any[] = this.workflow.steps.filter(
        s => s.stepRefId == this.selectedStepId
      );
      if (filteredSteps.length == 1) {
        this.runStep = filteredSteps[0];
        this.selectedStepInputs = this.ConvertDynamicDataToArray(this.runStep.inputs);
        this.selectedStepOutputs =this.ConvertDynamicDataToArray(this.runStep.outputs);
      }
    }
  }

  public ConvertDynamicDataToArray(data: any[])
  {
    var dynamicData: any[] = []
      Object.keys(data).forEach(key => 
        dynamicData.push(
          {
            id: key,
            value: data[key]
          }
        )
    );

    return dynamicData;
  }

  workflowSteps$:Subscription;

  updateData() {
    this.workflow$ = this._nodeData
      .GetWorkflow(this.selectedId)
      .subscribe((result: any) => {
        this.workflow = result.result;
        this.workflowSteps$ = this._nodeData.GetWorkflowSteps(this.selectedId).subscribe((workflowStepResults: any) => {
          this.workflow.steps = workflowStepResults.result;
        })
        if (this.workflow.inputs != null) {
          this.workflowInputs = this.ConvertDynamicDataToArray(this.workflow.inputs).map(n => {
            return {
              id: n.id,
              value: n.value
            };
          });
        }
        if (this.template == undefined) {
          this.template$ = this._nodeData
            .GetWorkflowTemplates()
            .subscribe(templates => {
              this.template = templates.result.filter(
                r =>
                  r.name ==
                    this.workflow.workflowTemplateId.split(':')[0] &&
                  r.version ==
                  this.workflow.workflowTemplateId.split(':')[1]
              )[0];
            });
        }
      });
  }
}
