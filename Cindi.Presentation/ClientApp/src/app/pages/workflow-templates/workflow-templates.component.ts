import { ActivatedRoute } from '@angular/router';
import { CindiClientService } from "./../../services/cindi-client.service";
import { Subscription } from "rxjs/internal/Subscription";
import { Component, OnInit } from "@angular/core";
import { Store, select } from "@ngrx/store";
import * as fromWorkflowTemplates from "../../entities/workflow-templates/workflow-template.reducer";
import { selectAll } from "../../entities/workflow-templates/workflow-template.reducer";
import { ConvertTemplateToInputs } from "../../shared/utility/data-mapper";
import { NbWindowService } from "@nebular/theme";
import { Router } from '@angular/router';

@Component({
  selector: "workflow-templates",
  templateUrl: "./workflow-templates.component.html",
  styleUrls: ["./workflow-templates.component.scss"]
})
export class WorkflowTemplatesComponent implements OnInit {
  workflowTemplates: any[];
  workflowTemplates$: Subscription;

  constructor(
    private workflowTemplateStore: Store<fromWorkflowTemplates.State>,
    private windowService: NbWindowService,
    private cindiClient: CindiClientService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this.workflowTemplates$ = workflowTemplateStore
      .pipe(select(selectAll))
      .subscribe(result => {
        this.workflowTemplates = result;
      });
  }

  ngOnInit() {}

  openWindow(contentTemplate, workflowTemplate) {
    console.log(workflowTemplate);

    this.windowService.open(contentTemplate, {
      title: "Create Workflow: " + workflowTemplate.referenceId,
      context: {
        inputs: ConvertTemplateToInputs(workflowTemplate),
        workflowTemplate: workflowTemplate
      }
    });
  }

  submitNewWorkflow(event) {
    const submission = this.cindiClient.PostWorkflow({
      name: "",
      description: "",
      workflowTemplateId: event.props.referenceId,
      inputs: event.value
    }).subscribe((result) => {
      console.log("Submitted")
    });
  }

  navigateToWorkflowTemplate(workflowTemplateReferenceId: string) {
    console.log(workflowTemplateReferenceId);
    this.router.navigate(["./" + workflowTemplateReferenceId], { relativeTo: this.activatedRoute });
  }
}
