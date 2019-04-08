import { Component, OnInit, Input } from "@angular/core";

@Component({
  selector: "step-progress-bar",
  templateUrl: "./step-progress-bar.component.html",
  styleUrls: ["./step-progress-bar.component.css"]
})
export class StepProgressBarComponent implements OnInit {
  _step: any;
  ProgressValue = 0;

  @Input()
  set step(step) {
    this._step = step;
    switch (step.status) {
      case "unassigned":
        this.ProgressValue = 10;
        break;
        case "assigned":
        this.ProgressValue = 50;
        break;
        case "successful":
        this.ProgressValue = 100;
        break;
        case "warning":
        this.ProgressValue = 100;
        break;
        case "error":
        this.ProgressValue = 100;
        break;
        case "unknown":
        this.ProgressValue = 0;
        break;
    }
  }

  constructor() {}

  ngOnInit() {}
}
