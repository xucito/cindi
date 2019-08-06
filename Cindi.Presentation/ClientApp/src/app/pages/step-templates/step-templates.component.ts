import { Router, ActivatedRoute } from "@angular/router";
import { NbToastrService, NbWindowService } from "@nebular/theme";
import { CindiClientService } from "./../../services/cindi-client.service";
import { InputAction } from "./../../shared/components/dynamic-form/input/input-action";
import { Component, OnInit } from "@angular/core";
import { Store, select } from "@ngrx/store";
import {
  State,
  selectAll,
  getStepTemplate
} from "../../entities/step-templates/step-template.reducer";
import { Subscription } from "rxjs";
import {
  NbToastrConfig,
  NbGlobalPhysicalPosition,
  NbToastRef
} from "@nebular/theme";
import { ConvertTemplateToInputs } from "../../shared/utility/data-mapper";

@Component({
  selector: "step-templates",
  templateUrl: "./step-templates.component.html",
  styleUrls: ["./step-templates.component.scss"]
})
export class StepTemplatesComponent implements OnInit {
  stepTemplates;
  stepTemplates$: Subscription;

  constructor(
    private stepTemplateStore: Store<State>,
    private cindiClient: CindiClientService,
    private toastrService: NbToastrService,
    private windowService: NbWindowService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this.stepTemplates$ = this.stepTemplateStore
      .pipe(select(selectAll))
      .subscribe(result => {
        this.stepTemplates = result;
      });
  }

  ngOnInit() {}

  actionInput(event: InputAction) {
    console.log("Selected Input");
  }

  openWindow(contentTemplate, stepTemplate) {
    this.windowService.open(contentTemplate, {
      title: "Create Step: " + stepTemplate.referenceId,
      context: {
        inputs: ConvertTemplateToInputs(stepTemplate),
        stepTemplate: stepTemplate
      }
    });
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

  navigateToStepTemplate(stepTemplateReferenceId: string) {
    console.log(stepTemplateReferenceId);
    this.router.navigate(["./" + stepTemplateReferenceId], { relativeTo: this.activatedRoute });
  }
}
