import { Component, OnInit, Input } from "@angular/core";
import { Store, select } from "@ngrx/store";
import {
  State,
  selectAll,
  getMostRecentSteps
} from "../../../entities/steps/step.reducer";
import { Subscription } from "rxjs";
import { StepStatuses } from '../../../entities/steps/step-statuses.enum';

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

  constructor(store: Store<State>) {
    this.steps$ = store
      .pipe(select(getMostRecentSteps, { hits: 10 }))
      .subscribe(result => {
        this.steps = result;
      });

    this.uncompletedSteps$ = store
      .pipe(select(getMostRecentSteps, { hits: 10,
      validStatuses: [
        StepStatuses.Assigned,
        StepStatuses.Suspended,
        StepStatuses.Unassigned
       ] }))
      .subscribe(result => {
        this.uncompletedSteps = result;
      });
  }

  ngOnInit() {}
}
