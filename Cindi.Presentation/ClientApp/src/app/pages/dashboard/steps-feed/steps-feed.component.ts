import { CindiClientService } from "./../../../services/cindi-client.service";
import { Component, OnInit, Input } from "@angular/core";
import { Store, select } from "@ngrx/store";
import {
  State,
  selectAll,
  getMostRecentSteps
} from "../../../entities/steps/step.reducer";
import { Subscription } from "rxjs";
import { StepStatuses } from "../../../entities/steps/step-statuses.enum";
import { InputAction } from "../../../shared/components/dynamic-form/input/input-action";
import {
  NbWindowService,
  NbToastRef,
  NbToastrService,
  NbToastrConfig,
  NbGlobalPhysicalPosition
} from "@nebular/theme";
import * as fromStepTemplate from "../../../entities/step-templates/step-template.reducer";
import { getStepTemplate } from "../../../entities/step-templates/step-template.reducer";
import { ConvertTemplateToInputs } from "../../../shared/utility/data-mapper";
import { take } from 'rxjs/operators';
import { loadSteps } from '../../../entities/steps/step.actions';

@Component({
  selector: "steps-feed",
  templateUrl: "./steps-feed.component.html",
  styleUrls: ["./steps-feed.component.scss"]
})
export class StepsFeedComponent implements OnInit {
  steps: any[];
  steps$: Subscription;
  uncompletedSteps: any[];
  uncompletedSteps$: Subscription;
  @Input()
  set Steps(steps) {
    this.steps = steps;
  }

  constructor(
    private store: Store<State>,
    private stepTemplateStore: Store<fromStepTemplate.State>,
    private windowService: NbWindowService,
    private cindiClient: CindiClientService,
    private toastrService: NbToastrService
  ) {
    this.steps$ = store
      .pipe(select(getMostRecentSteps, { hits: 100 }))
      .subscribe(result => {
        this.steps = result;
      });

    this.uncompletedSteps$ = store
      .pipe(
        select(getMostRecentSteps, {
          hits: 100,
          validStatuses: [
            StepStatuses.Assigned,
            StepStatuses.Suspended,
            StepStatuses.Unassigned
          ]
        })
      )
      .subscribe(result => {
        this.uncompletedSteps = result;
      });
  }

  ngOnInit() {}

  openWindow(contentTemplate, step) {
    this.stepTemplateStore
      .pipe(select(getStepTemplate, { referenceId: step.stepTemplateId }))
      .pipe(take(1))
      .subscribe(result => {
        this.windowService.open(contentTemplate, {
          title: "Create Step: " + result.referenceId,
          context: {
            inputs: ConvertTemplateToInputs(result, step, undefined),
            stepTemplate: result
          }
        });
      });
  }

  getStepTemplate(stepTemplateReferenceId) {
    this.stepTemplateStore
      .pipe(select(getStepTemplate, { referenceId: stepTemplateReferenceId }))
      .subscribe(result => {
        return result;
      });
  }

  openStepViewWindow(contentTemplate, step) {
    this.stepTemplateStore
      .pipe(select(getStepTemplate, { referenceId: step.stepTemplateId }))
      .pipe(take(1))
      .subscribe(result => {
        this.windowService.open(contentTemplate, {
          title: "Create Step: " + result.referenceId,
          context: {
            step: step,
            stepTemplate: result
          }
        });
      });
  }

  actionInput(event: InputAction) {
    console.log("Selected Input");
  }

  submitNewStep(event) {
    let submit = this.cindiClient
      .PostStep({
        name: "",
        description: "",
        stepTemplateId: event.props.referenceId,
        inputs: event.value
      })
      .subscribe(result => {
        const config: Partial<NbToastrConfig> = {
          status: "success",
          destroyByClick: true,
          duration: 3000,
          hasIcon: false,
          position: NbGlobalPhysicalPosition.TOP_RIGHT,
          preventDuplicates: true
        };

        const toastRef: NbToastRef = this.toastrService.show(
          "success",
          "Created step " + result.result.id,
          config
        );
        console.log("Submitted");
      });
    console.log(event);
  }

  updateStep(stepId: string, status: string)
  {
    this.cindiClient.PutStep(stepId, status).subscribe(result => {
      const config: Partial<NbToastrConfig> = {
        status: "success",
        destroyByClick: true,
        duration: 3000,
        hasIcon: false,
        position: NbGlobalPhysicalPosition.TOP_RIGHT,
        preventDuplicates: true
      };

      const toastRef: NbToastRef = this.toastrService.show(
        "success",
         status + " step " + result.result.id,
        config
      );

      this.store.dispatch(loadSteps({status: undefined}));
    });
  }
}
