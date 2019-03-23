import { Component, OnInit, ViewChild, OnDestroy } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { NodeDataService } from "../services/node-data.service";
import { Subscription } from "rxjs";
import { InputBase } from "../shared/components/form/input/input-base";
import { ConvertStepTemplateToInputs, IsStepComplete } from "../shared/utility";
import { FormsComponent } from "../shared/components/form/form.component";

@Component({
  selector: "app-step",
  templateUrl: "./step.component.html",
  styleUrls: ["./step.component.css"]
})
export class StepComponent implements OnInit, OnDestroy {
  ngOnDestroy(): void {
    if (this.$step != null) {
      this.$step.unsubscribe();
    }

    if (this.params$ != null) {
      this.params$.unsubscribe();
    }
    if (this.$StepTemplate != null) {
      this.$StepTemplate.unsubscribe();
    }
    clearInterval(this.run$);
  }
  IsNewStep = false;
  selectedId = "";
  selectedTemplateName = "";
  selectedTemplateVersion;
  $StepTemplate: Subscription;
  stepTemplate;
  $step: Subscription;
  step: any;
  inputs: InputBase<any>[];
  content: any;
  params$: Subscription;

  run$;
  @ViewChild(FormsComponent)
  form: FormsComponent;
  clonedId: string;
  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _nodeData: NodeDataService
  ) {
    this.params$ = _route.params.subscribe(p => {
      this.selectedId = p.id;
      if (this.selectedId == "new") {
        this.IsNewStep = true;

        this.selectedTemplateName = p.templateName;
        this.selectedTemplateVersion = p.version;
        this.$StepTemplate = _nodeData
          .GetStepTemplate(
            this.selectedTemplateName,
            this.selectedTemplateVersion
          )
          .subscribe(result => {
            console.log(result);
            this.stepTemplate = result.result;
            this.clonedId = p.clonedId;
            if (this.clonedId != undefined) {
              this.$step = this._nodeData
                .GetStep(this.clonedId)
                .subscribe(stepResult => {
                  this.inputs = ConvertStepTemplateToInputs(
                    this.stepTemplate,
                    stepResult.result
                  );
                });
            } else {
              this.inputs = ConvertStepTemplateToInputs(this.stepTemplate);
            }
          });
      } else {
        this.$step = _nodeData
          .GetStep(this.selectedId)
          .subscribe(stepResult => {
            this.step = stepResult.result;
            const _thisObject = this;
            this.outputHeaders = this.getOutputKeys(this.step)
            this.run$ = setInterval(function() {
              _thisObject.loadLogs();
              this.outputHeaders = this.getOutputKeys(this.step)
              console.log("Just refreshed steps");
            }, 2000);
            this.$StepTemplate = _nodeData
              .GetStepTemplate(
                this.step.stepTemplateId.split(":")[0],
                this.step.stepTemplateId.split(":")[1]
              )
              .subscribe(result => {
                console.log(result);
                this.stepTemplate = result.result;
                this.inputs = ConvertStepTemplateToInputs(
                  this.stepTemplate,
                  this.step
                );
              });
          });
      }
    });
  }
outputHeaders: string[] = [];
  payload: any;

  submission$: Subscription;

  getOutputKeys(step: any): string[]
  {
    let outputs = [];
    for (let key of Object.keys(step.outputs)) { 
      outputs.push(key)
    }
    return outputs;
  }

  loadLogs() {
    this.$step = this._nodeData.GetStep(this.selectedId).subscribe(result => {
      this.step = result.result;
      this.LoadLogs(this.step.logs);
      if (IsStepComplete(this.step)) {
        clearInterval(this.run$);
      }
    });
  }

  submit(event) {
    this.payload = {
      stepTemplateId: this.stepTemplate.id,
      inputs: event
    };

    this.IsNewStep = false;
    this.submission$ = this._nodeData
      .PostStep(this.payload)
      .subscribe(result => {
        this._router.navigate(["../" + result.objectRefId], {
          relativeTo: this._route
        });
      });
  }

  private LoadLogs(logs: any) {
    this.content = "";
    logs.forEach(log => {
      this.content += "\n " + "<" + log.createdOn + ">: " + log.message;
    });
  }

  _thisObject;
  ngOnInit() {}
}
