import { CindiClientService } from './../../services/cindi-client.service';
import { Component, OnInit } from "@angular/core";
import { Store, select } from "@ngrx/store";
import { ActivatedRoute } from "@angular/router";
import {
  State,
  getStepTemplate
} from "../../entities/step-templates/step-template.reducer";
import { NbWindowService, NbToastrConfig, NbGlobalPhysicalPosition, NbToastRef, NbToastrService } from '@nebular/theme';
import { ConvertTemplateToInputs } from '../../shared/utility/data-mapper';

@Component({
  selector: "step-template",
  templateUrl: "./step-template.component.html",
  styleUrls: ["./step-template.component.scss"]
})
export class StepTemplateComponent implements OnInit {
  selectedId: string;
  stepTemplate: any;

  constructor(
    private stepTemplateStore: Store<State>,
    private _route: ActivatedRoute,
    private windowService: NbWindowService,
    private cindiClient: CindiClientService,
    private toastrService: NbToastrService
  ) {
    _route.params.subscribe(p => {
      this.selectedId = p.id;
      this.stepTemplateStore
        .pipe(select(getStepTemplate, { referenceId: this.selectedId }))
        .subscribe(result => {
          this.stepTemplate = result;
        });
    });
  }

  ngOnInit() {}

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
}
