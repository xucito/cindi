import { Component, OnInit } from "@angular/core";
import { Subscription } from "rxjs";
import { select, Store } from "@ngrx/store";
import { State, getMostRecentWorkflows } from "../../../entities/workflows/workflow.reducer";

@Component({
  selector: "workflow-feed",
  templateUrl: "./workflow-feed.component.html",
  styleUrls: ["./workflow-feed.component.scss"]
})
export class WorkflowFeedComponent implements OnInit {
  workflows$: Subscription;
  workflows: any[];
  workflowsInprogress = [];

  constructor(private store: Store<State>) {
    this.workflows$ = this.store
      .pipe(select(getMostRecentWorkflows, { hits: 100 }))
      .subscribe(result => {
        this.workflows = result;
        this.workflowsInprogress = result.filter(workflow => workflow.status == 'started' || workflow.status == 'queued')
      });
  }

  ngOnInit() {}
}
