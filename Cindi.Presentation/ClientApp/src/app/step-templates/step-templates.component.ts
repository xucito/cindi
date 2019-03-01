import { Component, OnInit, OnDestroy } from "@angular/core";
import { NodeDataService } from "../services/node-data.service";
import { Subscription } from "rxjs";
import { Router } from "@angular/router";

@Component({
  selector: "app-step-templates",
  templateUrl: "./step-templates.component.html",
  styleUrls: ["./step-templates.component.css"]
})
export class StepTemplatesComponent implements OnInit, OnDestroy {
  public templates: any[] = [];

  templates$: Subscription;
  columnsToDisplay = ["name", "version", "start"];
  run: any;

  constructor(private nodeData: NodeDataService, private router: Router) {}

  ngOnInit() {
    const _thisObject = this;
    this.updateData();
    this.run = setInterval(function() {
      _thisObject.updateData();
      console.log("Just refreshed Templates");
    }, 10000);
  }

  updateData() {
    if (this.templates$ === undefined || this.templates$.closed) {
      this.templates$ = this.nodeData.GetStepTemplates().subscribe(result => {
        this.templates = result;
      });
    }
  }

  createStep(name: string, version: string) {
    console.log(name);
    console.log(version);
    this.router.navigate([
      "steps/new",
      { templateName: name, version: version }
    ]);
  }

  ngOnDestroy() {
    clearInterval(this.run);
  }
}
