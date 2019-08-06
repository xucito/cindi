import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { Store, select } from '@ngrx/store';
import { State, getStepCreationMetrics } from '../../../entities/steps/step.reducer';

@Component({
  selector: 'step-time-line',
  templateUrl: './step-time-line.component.html',
  styleUrls: ['./step-time-line.component.css']
})
export class StepTimeLineComponent implements OnInit {
  metrics: any[];
  metrics$: Subscription;

  constructor(store: Store<State>) {
    this.metrics$ = store
      .pipe(select(getStepCreationMetrics, {}))
      .subscribe(result => {
        this.metrics = result;
      });
  }

  ngOnInit() {
  }

}
