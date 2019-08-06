import { Component, OnInit, Input, OnDestroy, OnChanges } from "@angular/core";
import * as fromStep from "../../../entities/steps/step.reducer";
import * as fromStepTemplate from "../../../entities/step-templates/step-template.reducer";
import { Store, select } from "@ngrx/store";
import { Subscription, Observable } from "rxjs";
import {
  ConvertTemplateToInputs,
  ConvertTemplateToOutputs
} from "../../utility/data-mapper";

@Component({
  selector: "step-view",
  templateUrl: "./step-view.component.html",
  styleUrls: ["./step-view.component.css"]
})
export class StepViewComponent implements OnInit, OnDestroy, OnChanges {
  ngOnChanges(): void {
    this.GenerateView();
  }

  ngOnDestroy(): void {}
  inputs: any;
  outputs: any;
  content: string;

  @Input() step: any;
  @Input() stepTemplate: any;

  private GenerateView() {
    if (this.stepTemplate != undefined && this.step != undefined) {
      this.inputs = ConvertTemplateToInputs(this.stepTemplate, this.step);
      this.outputs = ConvertTemplateToOutputs(this.stepTemplate, this.step);
    }
  }

  constructor() {}

  ngOnInit() {}

  private LoadLogs(logs: any) {
    this.content = "";
    if (logs != undefined) {
      logs.forEach(log => {
        this.content += "\n " + "<" + log.createdOn + ">: " + log.message;
      });
    }
  }
}
