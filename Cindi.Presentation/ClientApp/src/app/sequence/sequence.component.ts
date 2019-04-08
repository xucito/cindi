import { Component, OnInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { AppStateService } from "../services/app-state.service";
import { NodeDataService } from "../services/node-data.service";
import { Subscription } from "rxjs";

@Component({
  selector: "app-sequence",
  templateUrl: "./sequence.component.html",
  styleUrls: ["./sequence.component.css"]
})
export class SequenceComponent implements OnInit, OnDestroy {
  selectedId;
  public template;
  public sequence;
  public template$: Subscription;
  public sequence$: Subscription;
  public sequenceInputs: any[] = [];
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
    this.sequence$.unsubscribe();
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

    if (this.sequence != undefined) {
      let filteredSteps: any[] = this.sequence.steps.filter(
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

  sequenceSteps$:Subscription;

  updateData() {
    this.sequence$ = this._nodeData
      .GetSequence(this.selectedId)
      .subscribe((result: any) => {
        this.sequence = result.result;
        this.sequenceSteps$ = this._nodeData.GetSequenceSteps(this.selectedId).subscribe((sequenceStepResults: any) => {
          this.sequence.steps = sequenceStepResults.result;
        })
        if (this.sequence.inputs != null) {
          this.sequenceInputs = this.ConvertDynamicDataToArray(this.sequence.inputs).map(n => {
            return {
              id: n.id,
              value: n.value
            };
          });
        }
        if (this.template == undefined) {
          this.template$ = this._nodeData
            .GetSequenceTemplates()
            .subscribe(templates => {
              this.template = templates.result.filter(
                r =>
                  r.name ==
                    this.sequence.sequenceTemplateId.split(':')[0] &&
                  r.version ==
                  this.sequence.sequenceTemplateId.split(':')[1]
              )[0];
            });
        }
      });
  }
}
