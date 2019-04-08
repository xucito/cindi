import { Component, OnInit, OnDestroy, ViewChild } from "@angular/core";
import { NodeDataService } from "../services/node-data.service";
import { Subscription, Observable, forkJoin } from "rxjs";
import { MatSort, MatTableDataSource } from "@angular/material";
import { Router, ActivatedRoute } from "@angular/router";
@Component({
  selector: "app-steps",
  templateUrl: "./steps.component.html",
  styleUrls: ["./steps.component.css"]
})
export class StepsComponent implements OnInit, OnDestroy {
  public steps: any[];
  private steps$: Subscription;
  columnsToDisplay = ["id", "name", "templateId", "status", "copy"];
  run: any;

  statusCodes = ["Unassigned", "Assigned", "Successful", "Warning", "Error"];

  constructor(
    private nodeData: NodeDataService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit() {
    const _thisObject = this;
    this.updateData();
    this.run = setInterval(function() {
      _thisObject.updateData();
      console.log("Just refreshed steps");
    }, 10000);
  }

  updateData() {
    if (this.steps$ == undefined || this.steps$.closed) {
      this.steps$ = forkJoin(this.nodeData.GetSteps()).subscribe(result => {
        this.steps = result[0].result.sort((a, b) => a.id - b.id);
        // this.steps$.unsubscribe();
      });
    }
  }

  ngOnDestroy() {
    clearInterval(this.run);
  }

  AddStep() {
    this.nodeData
      .PostStep({
        name: "TestStep",
        TemplateId: "TestStep_v1"
      })
      .subscribe(result => {
        console.log("Complete");
        this.updateData();
      });
  }

  cloneStep(row) {
    this.router.navigate(
      [
        "./new",
        {
          templateName: row.stepTemplateId.split(":")[0],
          version: row.stepTemplateId.split(":")[1],
          clonedId: row.id
        }
      ],
      { relativeTo: this.activatedRoute }
    );
  }

  selectRow(row) {
    this.router.navigate(["./" + row.id], { relativeTo: this.activatedRoute });
  }
}
