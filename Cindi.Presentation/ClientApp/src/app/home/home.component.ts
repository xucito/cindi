import { Component, OnInit, ViewEncapsulation, OnDestroy } from "@angular/core";
import { NodeDataService } from "../services/node-data.service";
import { Subscription } from "rxjs";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  styleUrls: ["./home.component.css"]
})
export class HomeComponent implements OnDestroy {

  stats$: Subscription;
  stats: any;
  run: any;

  constructor(private _nodeData: NodeDataService) {
    const _thisObject = this;
    this.run = setInterval(function() {
      _thisObject.UpdateData();
      console.log("Just refreshed steps");
    }, 2000);
  }

  UpdateData() {
    this._nodeData.GetStats().subscribe(result => {
      this.stats = result.result;
    });
  }

  GetStepStats(status) {
    if (this.stats != undefined) {
      return this.stats.steps[status];
    }

    return "?";
  }


  ngOnDestroy(): void {
    if (this.stats$ != undefined) {
      this.stats$.unsubscribe();
    }
    clearInterval(this.run);
  }
}
