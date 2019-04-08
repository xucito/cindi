import { Component, OnInit } from "@angular/core";
import { Subscription } from "rxjs";
import { NodeDataService } from "../services/node-data.service";
import { ActivatedRoute, Router } from "@angular/router";
import { AppStateService } from "../services/app-state.service";

@Component({
  selector: "app-sequence-templates",
  templateUrl: "./sequence-templates.component.html",
  styleUrls: ["./sequence-templates.component.css"]
})
export class SequenceTemplatesComponent implements OnInit {
  public sequenceTemplates: any[];
  private sequenceTemplates$: Subscription;

  columnsToDisplay = ["name", "version", "visualize"];

  run: any;

  constructor(
    private nodeData: NodeDataService,
    private _router: Router,
    private _route: ActivatedRoute,
    private _appState: AppStateService
  ) {}

  ngOnInit() {
    this.sequenceTemplates$ = this.nodeData
      .GetSequenceTemplates()
      .subscribe(result => {
        this.sequenceTemplates = result.result;
      });

    let test = this._router.url;
    /*
    if()
    {

    }*/
  }

  selectSequence(selectedRow) {
    this._appState.setSelectedSequenceTemplate(selectedRow);

    this._router.navigate([selectedRow.name], {
      relativeTo: this._route
    });
  }
}
