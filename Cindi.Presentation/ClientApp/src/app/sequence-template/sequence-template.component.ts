import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { AppStateService } from "../services/app-state.service";
import { NodeDataService } from "../services/node-data.service";

@Component({
  selector: "app-sequence-template",
  templateUrl: "./sequence-template.component.html",
  styleUrls: ["./sequence-template.component.css"]
})
export class SequenceTemplateComponent implements OnInit {
  selectedId;
  public template;
  sequenceInputs: any[];

  columnsToDisplay = ["id", "type"];
  constructor(
    private _route: ActivatedRoute,
    private _appState: AppStateService,
    private _nodeData: NodeDataService
  ) {
    _route.params.subscribe(p => {
      this.selectedId = p.id;

      this.template = _appState.selectedSequenceTemplate;
      if (this.template == undefined) {
        _nodeData.GetSequenceTemplates().subscribe(result => {
          this.template = result.result.filter(r => r.name == this.selectedId)[0];
          this.sequenceInputs = Object.keys(this.template.inputDefinitions).map(
            n => {
              return {
                id: n,
                type: this.getType(+this.template.inputDefinitions[n].type)
              };
            }
          );
        });
      }
    });
  }

  getType(type: number) {
    switch (type) {
      case 0:
        return "bool";
      case 1:
        return "string";
      case 2:
        return "int";
      case 3:
        return "object";
    }
  }

  ngOnInit() {}
}
