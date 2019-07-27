import { Component, OnInit } from "@angular/core";
import { Subscription } from "rxjs";
import { NodeDataService } from "../services/node-data.service";
import { ActivatedRoute, Router } from "@angular/router";
import { AppStateService } from "../services/app-state.service";

@Component({
  selector: "app-workflow-templates",
  templateUrl: "./workflow-templates.component.html",
  styleUrls: ["./workflow-templates.component.css"]
})
export class WorkflowTemplatesComponent implements OnInit {
  public workflowTemplates: any[];
  private workflowTemplates$: Subscription;

  columnsToDisplay = ["name", "version", "visualize"];

  run: any;

  constructor(
    private nodeData: NodeDataService,
    private _router: Router,
    private _route: ActivatedRoute,
    private _appState: AppStateService
  ) {}

  ngOnInit() {
    this.workflowTemplates$ = this.nodeData
      .GetWorkflowTemplates()
      .subscribe(result => {
        this.workflowTemplates = result.result;
      });

    let test = this._router.url;
    /*
    if()
    {

    }*/
  }

  selectWorkflow(selectedRow) {
    this._appState.setSelectedWorkflowTemplate(selectedRow);

    this._router.navigate([selectedRow.name], {
      relativeTo: this._route
    });
  }
}
