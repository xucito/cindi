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

  columnsToDisplay = ["id", "type", "value"];

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
      }
    }
  }

  updateData() {
    this.sequence$ = this._nodeData
      .GetSequence(this.selectedId)
      .subscribe(result => {
        this.sequence = result;
        this.sequenceInputs = this.sequence.reference.inputs.map(n => {
          return {
            id: n.id,
            type: this.getType(n.type),
            value: n.value
          };
        });
        if (this.template == undefined) {
          this.template$ = this._nodeData
            .GetSequenceTemplates()
            .subscribe(templates => {
              this.template = templates.filter(
                r =>
                  r.name ==
                    this.sequence.reference.sequenceTemplateReference.name &&
                  r.version ==
                    this.sequence.reference.sequenceTemplateReference.version
              )[0];
            });
        }
      });
  }
}
