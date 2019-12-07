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
import { Subscription } from 'rxjs';
import { selectAll } from '../../entities/step-templates/step-template.reducer';
import { take, skipWhile } from 'rxjs/operators';

@Component({
  selector: "workflow-template",
  templateUrl: "./workflow-template.component.html",
  styleUrls: ["./workflow-template.component.css"]
})
export class WorkflowTemplateComponent implements OnInit {
  selectedId: string;
  workflowTemplate: any;
  stepTemplates$: Subscription;
  stepTemplates: any[];
  save$: Subscription;

  constructor(
    private workflowTemplateStore: Store<State>,
    private _route: ActivatedRoute,
    private windowService: NbWindowService,
    private cindiClient: CindiClientService,
    private toastrService: NbToastrService,
    private store: Store<State>
  ) {
    _route.params.subscribe(p => {
      this.selectedId = p.id;
      this.workflowTemplateStore
        .pipe(
          select(getWorkflowTemplateEntityById, {
            referenceId: this.selectedId
          })
        , skipWhile(value => value == undefined),take(1)).subscribe(result => {
          this.workflowTemplate = result;
        });
    });

    this.stepTemplates$ = store.pipe(select(selectAll)).subscribe(result => {
      this.stepTemplates = result;
    });
  }

  ngOnInit() {}

  saveWorkflow(workflow) {
    console.log(workflow);
    this.save$ = this.cindiClient
      .PostWorkflowTemplate(workflow)
      .subscribe(result => {
        console.log("Save successful");
      });
  }
}
