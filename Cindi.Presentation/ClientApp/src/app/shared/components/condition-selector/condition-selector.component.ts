import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";

@Component({
  selector: "condition-selector",
  templateUrl: "./condition-selector.component.html",
  styleUrls: ["./condition-selector.component.scss"]
})
export class ConditionSelectorComponent implements OnInit {
  constructor() {}

  @Output() onSubmit: EventEmitter<any> = new EventEmitter<any>();

  ngOnInit() {}

  @Input()
  set condition(condition: any) {
    this.selectedConditionType = condition.name;
    this._condition = condition;
  }
  @Input() options: any;

  _condition: any;
  _selectedStep = "0";

  //All fields used for StepStatus type
  stepStatuses = ["successful", "warning", "error"];

  numberStore = 0;

  selectableConditionTypes = [
    {
      name: "Step Status",
      value: "StepStatus"
    }
  ];

  selectedConditionType = "";

  changed(event) {
    console.log(event);
    if (event == "StepStatus") {
      this._condition = {
        name: "StepStatus",
        comparer: "is",
        status: "successful",
        statusCode: undefined,
        workflowStepId: this.options.steps ? this.options.steps[0] : undefined
      };
    }
  }

  AddCondition() {
    this.onSubmit.emit(this._condition);
  }
}
