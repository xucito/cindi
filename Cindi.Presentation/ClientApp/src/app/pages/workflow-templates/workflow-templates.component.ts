import { NotificationService } from './../../shared/services/notification.service';
import { ActivatedRoute } from "@angular/router";
import { CindiClientService } from "./../../services/cindi-client.service";
import { Subscription } from "rxjs/internal/Subscription";
import { Component, OnInit, ElementRef, ViewChild } from "@angular/core";
import { Store, select } from "@ngrx/store";
import * as fromWorkflowTemplates from "../../entities/workflow-templates/workflow-template.reducer";
import { selectAll } from "../../entities/workflow-templates/workflow-template.reducer";
import { ConvertTemplateToInputs } from "../../shared/utility/data-mapper";
import { NbWindowService } from "@nebular/theme";
import { Router } from "@angular/router";
import { Page } from "../../shared/model/page";

@Component({
  selector: "workflow-templates",
  templateUrl: "./workflow-templates.component.html",
  styleUrls: ["./workflow-templates.component.scss"]
})
export class WorkflowTemplatesComponent implements OnInit {
  @ViewChild("contentTemplate", { static: true }) contentTemplate: ElementRef;
  workflowTemplates: any[];
  workflowTemplates$: Subscription;
  columns = [
    { name: "referenceId" },
    { name: "description" },
    { name: "createdOn" },
    { name: "createdBy" },
    { name: "Clone", icon: "copy", action: "clone", type: "button" },
    { name: "View", icon: "eye-outline", action: "view", type: "button"}
  ];
  isLoading = false;

  rows = [];
  page: Page = new Page();

  constructor(
    private workflowTemplateStore: Store<fromWorkflowTemplates.State>,
    private windowService: NbWindowService,
    private cindiClient: CindiClientService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private notificationService: NotificationService
  ) {
    this.setPage({
      offset: 0,
      pageSize: 10
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
    const submission = this.cindiClient
      .PostWorkflow({
        name: "",
        description: "",
        workflowTemplateId: event.props.referenceId,
        inputs: event.value
      })
      .subscribe(result => {
        console.log("Submitted");
        this.notificationService.show(
          "success",
          "success",
          "Created workflow " + result.result.id
        );
      });
  }

  navigateToWorkflowTemplate(workflowTemplateReferenceId: string) {
    console.log(workflowTemplateReferenceId);
    this.router.navigate(["./" + workflowTemplateReferenceId], {
      relativeTo: this.activatedRoute
    });
  }

  setPage(pageInfo) {
    this.page.pageNumber = pageInfo.offset;
    this.page.size = pageInfo.pageSize;
    this.isLoading = true;
    this.cindiClient
      .GetEntity("workflow-templates", "", this.page.pageNumber, this.page.size)
      .subscribe(
        pagedData => {
          pagedData.result.forEach(element => {
            element.truncatedid = element.id.slice(0, 8);
          });

          this.rows = pagedData.result;
          this.page.totalElements = pagedData.count;
          this.page.totalPages = pagedData.count / this.page.size;
        },
        error => {
          console.error(error);
        },
        () => {
          this.isLoading = false;
        }
      );
  }

  onSort(event) {
    // event was triggered, start sort sequence
    console.log("Sort Event", event);
    this.isLoading = true;
    var sortStatement = "";

    event.sorts.forEach(element => {
      sortStatement += element.prop + ":" + (element.dir == "desc" ? -1 : 1);
    });
    this.cindiClient
      .GetEntity(
        "workflow-templates",
        "",
        this.page.pageNumber,
        this.page.size,
        sortStatement
      )
      .subscribe(
        pagedData => {
          pagedData.result.forEach(element => {
            element.truncatedid = element.id.slice(0, 8);
          });
          this.rows = pagedData.result;
          this.page.totalElements = pagedData.count;
          this.page.totalPages = pagedData.count / this.page.size;
        },
        error => {
          console.error(error);
        },
        () => {
          this.isLoading = false;
        }
      );
  }

  onAction(event) {
    switch (event.action.action) {
      case "clone":
        this.openWindow(this.contentTemplate, event.value);
        break;
      case "view":
        this.navigateToWorkflowTemplate(event.value.referenceId)
    }
  }
}
