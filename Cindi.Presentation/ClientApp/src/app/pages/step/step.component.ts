import { Component, OnInit, OnDestroy, OnChanges } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { CindiClientService } from "../../services/cindi-client.service";
import { Subscription } from "rxjs";
import { Store, select } from "@ngrx/store";
import * as fromStepTemplate from "../../entities/step-templates/step-template.reducer";
import { getStepTemplate } from "../../entities/step-templates/step-template.reducer";
import { take } from "rxjs/operators";
import {
  ConvertTemplateToInputs,
  ConvertTemplateToOutputs
} from "../../shared/utility/data-mapper";
import { StepStatuses } from "../../entities/steps/step-statuses.enum";
@Component({
  selector: "step",
  templateUrl: "./step.component.html",
  styleUrls: ["./step.component.css"]
})
export class StepComponent implements OnInit, OnDestroy {
  ngOnDestroy(): void {}
  inputs: any;
  outputs: any;
  public content: string;
  stepTemplate: any;
  step: any;
  step$: Subscription;
  private GenerateView() {
    if (this.stepTemplate != undefined && this.step != undefined) {
      this.inputs = ConvertTemplateToInputs(this.stepTemplate, this.step);
      this.outputs = ConvertTemplateToOutputs(this.stepTemplate, this.step);
      this.LoadLogs(this.step.logs);
    }
  }

  pageStatus(status: string) {
    switch (status) {
      case StepStatuses.Successful:
        return "success";
      case StepStatuses.Error:
        return "danger";
      case StepStatuses.Warning:
        return "warning";
    }
    return 'basic';
  }
  constructor(
    private route: ActivatedRoute,
    private cindiData: CindiClientService,
    private stepTemplateStore: Store<fromStepTemplate.State>
  ) {
    this.step$ = cindiData
      .GetStep(route.snapshot.paramMap.get("stepId"))
      .subscribe(result => {
        this.step = result.result;
        this.stepTemplateStore
          .pipe(
            select(getStepTemplate, { referenceId: this.step.stepTemplateId })
          )
          .pipe(take(1))
          .subscribe(stepTemplate => {
            this.stepTemplate = stepTemplate;
            this.GenerateView();
          });
      });
  }

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
