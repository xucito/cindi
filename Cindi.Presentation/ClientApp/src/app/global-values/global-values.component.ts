import { Component, OnInit } from "@angular/core";
import { NodeDataService } from "../services/node-data.service";
import { Subscription } from "rxjs";
import { AddGlobalValueModalComponent } from "../shared/components/modals/add-global-value-modal/add-global-value-modal.component";
import { MatDialog } from "@angular/material";

@Component({
  selector: "app-global-values",
  templateUrl: "./global-values.component.html",
  styleUrls: ["./global-values.component.css"]
})
export class GlobalValuesComponent implements OnInit {
  globalValues: any;

  globalValues$: Subscription;

  columnsToDisplay = ["name", "type"];

  constructor(private nodeData: NodeDataService, public dialog: MatDialog) {
    this.globalValues$ = nodeData.GetGlobalValues().subscribe((result: any) => {
      this.globalValues = result.result;
    });
  }

  ngOnInit() {}

  addUser() {
    const dialogRef = this.dialog.open(AddGlobalValueModalComponent, {
      width: "250px",
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(result);
      this.nodeData.PostGlobalValues(result).subscribe(result => {
        console.log(result);
        this.nodeData.GetGlobalValues().subscribe(result => {
          this.globalValues = result.result;
        });
      });
    });
  }
}
