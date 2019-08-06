import { Component, OnInit, Input, OnChanges } from "@angular/core";
import * as fromStepTemplate from "../../../entities/step-templates/step-template.reducer";
import { Store, select } from "@ngrx/store";
import { ConvertTemplateToInputs } from "../../utility/data-mapper";
import { Subscription } from "rxjs/internal/Subscription";
``
@Component({
  selector: "template-view",
  templateUrl: "./template-view.component.html",
  styleUrls: ["./template-view.component.scss"]
})
export class TemplateViewComponent implements OnInit, OnChanges {
  ngOnChanges(): void {
    this.generateView();
  }
  inputs: any[];
  outputs: any[];

  @Input() inputDefinitions;
  @Input() outputDefinitions;

  constructor(private stepTemplateStore: Store<fromStepTemplate.State>) {}

  generateView() {
    if (this.inputDefinitions != null && this.outputDefinitions != null) {
      this.inputs = [];
      this.outputs = [];

      for (let prop of Object.keys(this.inputDefinitions)) {
        this.inputs.push({
          name: prop,
          properties: this.inputDefinitions[prop]
        });
      }

      for (let prop of Object.keys(this.outputDefinitions)) {
        this.outputs.push({
          name: prop,
          properties: this.outputDefinitions[prop]
        });
      }
    }
  }

  ngOnInit() {}
}
