import { Component, OnInit, OnDestroy, ViewChild } from "@angular/core";
import { NodeDataService } from "../services/node-data.service";
import { Subscription, Observable, forkJoin } from "rxjs";
import { MatSort, MatTableDataSource } from "@angular/material";
@Component({
  selector: "app-steps",
  templateUrl: "./steps.component.html",
  styleUrls: ["./steps.component.css"]
})
export class StepsComponent implements OnInit, OnDestroy {
  public unassignedSteps: any[];
  public assignedSteps: any[];
  public successfulSteps: any[];
  private steps$: Subscription;
  public erroredSteps: any[];
  columnsToDisplay = ["id", "name", "templateId", "status"];
  run: any;

  statusCodes = ["Unassigned", "Assigned", "Successful", "Warning", "Error"];

  constructor(private nodeData: NodeDataService) {}

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
      this.steps$ = forkJoin(
        this.nodeData.GetSteps("unassigned"),
        this.nodeData.GetSteps("assigned"),
        this.nodeData.GetSteps("successful"),
        this.nodeData.GetSteps("error")
      ).subscribe(result => {
        this.unassignedSteps = result[0].sort((a, b) => a.id - b.id);
        this.assignedSteps = result[1].sort((a, b) => a.id - b.id);
        this.successfulSteps = result[2].sort((a, b) => a.id - b.id);
        this.erroredSteps = result[3].sort((a, b) => a.id - b.id);
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
}
