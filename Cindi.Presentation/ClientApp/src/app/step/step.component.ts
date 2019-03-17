import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { NodeDataService } from "../services/node-data.service";
import { Subscription } from "rxjs";
import { InputBase } from "../shared/components/form/input/input-base";
import { ConvertStepTemplateToInputs } from "../shared/utility";

@Component({
  selector: "app-step",
  templateUrl: "./step.component.html",
  styleUrls: ["./step.component.css"]
})
export class StepComponent implements OnInit {
  IsNewStep = false;
  selectedId = "";
  selectedTemplateName = "";
  selectedTemplateVersion;
  $StepTemplate: Subscription;
  stepTemplate;
  inputs: InputBase<any>[];
  constructor(
    private _route: ActivatedRoute,
    private _nodeData: NodeDataService
  ) {
    _route.params.subscribe(p => {
      this.selectedId = p.id;
      if (this.selectedId == "new") {
        this.IsNewStep = true;
      }

      this.selectedTemplateName = p.templateName;
      this.selectedTemplateVersion = p.version;
      this.$StepTemplate= _nodeData.GetStepTemplate(this.selectedTemplateName, this.selectedTemplateVersion).subscribe(
        (result) => {
          console.log(result);
          this.stepTemplate = result.result;
          this.inputs = ConvertStepTemplateToInputs(this.stepTemplate);
        }
      );
    });
  }

  ngOnInit() {}
}
