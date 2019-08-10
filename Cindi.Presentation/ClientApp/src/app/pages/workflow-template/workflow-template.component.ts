import { Component, OnInit } from "@angular/core";
import { Store, select } from "@ngrx/store";
import {
  State,
  selectIds,
  selectEntities,
  getWorkflowTemplateEntityById
} from "../../entities/workflow-templates/workflow-template.reducer";
import { ActivatedRoute } from "@angular/router";
import { NbWindowService, NbToastrService } from "@nebular/theme";
import { CindiClientService } from "../../services/cindi-client.service";

@Component({
  selector: "workflow-template",
  templateUrl: "./workflow-template.component.html",
  styleUrls: ["./workflow-template.component.css"]
})
export class WorkflowTemplateComponent implements OnInit {
  selectedId: string;
  workflowTemplate: any;
  stepTemplates: any[];

  constructor(
    private workflowTemplateStore: Store<State>,
    private _route: ActivatedRoute,
    private windowService: NbWindowService,
    private cindiClient: CindiClientService,
    private toastrService: NbToastrService
  ) {
    _route.params.subscribe(p => {
      this.selectedId = p.id;
      this.workflowTemplateStore
        .pipe(
          select(getWorkflowTemplateEntityById, {
            referenceId: this.selectedId
          })
        ).subscribe(result => {
          this.workflowTemplate = result;
        });
    });
  }

  ngOnInit() {}
}
