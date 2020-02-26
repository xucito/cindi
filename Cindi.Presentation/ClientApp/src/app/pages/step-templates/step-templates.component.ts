import { Router, ActivatedRoute } from "@angular/router";
import { NbToastrService, NbWindowService } from "@nebular/theme";
import { CindiClientService } from "./../../services/cindi-client.service";
import { InputAction } from "./../../shared/components/dynamic-form/input/input-action";
import { Component, OnInit, ElementRef, ViewChild } from "@angular/core";
import { Store, select } from "@ngrx/store";
import {
  State,
  selectAll,
  getStepTemplate
} from "../../entities/step-templates/step-template.reducer";
import { Subscription } from "rxjs";
import {
  NbToastrConfig,
  NbGlobalPhysicalPosition,
  NbToastRef
} from "@nebular/theme";
import { ConvertTemplateToInputs } from "../../shared/utility/data-mapper";
import { Page } from "../../shared/model/page";
import { NotificationService } from "../../shared/services/notification.service";

@Component({
  selector: "step-templates",
  templateUrl: "./step-templates.component.html",
  styleUrls: ["./step-templates.component.scss"]
})
export class StepTemplatesComponent implements OnInit {
  @ViewChild("contentTemplate", { static: true }) contentTemplate: ElementRef;
  stepTemplates;
  stepTemplates$: Subscription;

  columns = [
    { name: "referenceId" },
    { name: "description" },
    { name: "createdOn" },
    { name: "createdBy" },
    { name: "Clone", icon: "copy", action: "clone", type: "button" }
  ];

  rows = [];
  page: Page = new Page();
  isLoading = false;

  constructor(
    private stepTemplateStore: Store<State>,
    private cindiClient: CindiClientService,
    private toastrService: NbToastrService,
    private windowService: NbWindowService,
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

  actionInput(event: InputAction) {
    console.log("Selected Input");
  }

  openWindow(contentTemplate, stepTemplate) {
    this.windowService.open(contentTemplate, {
      title: "Create Step: " + stepTemplate.referenceId,
      context: {
        inputs: ConvertTemplateToInputs(stepTemplate),
        stepTemplate: stepTemplate
      }
    });
  }

  submitNewStep(event) {
    let submit = this.cindiClient
      .PostStep({
        name: "",
        description: "",
        stepTemplateId: event.props.referenceId,
        inputs: event.value
      })
      .subscribe(result => {
        this.notificationService.show(
          "success",
          "success",
          "Created step " + result.result.id
        );
      });
    console.log(event);
  }

  navigateToStepTemplate(stepTemplateReferenceId: string) {
    console.log(stepTemplateReferenceId);
    this.router.navigate(["./" + stepTemplateReferenceId], {
      relativeTo: this.activatedRoute
    });
  }

  setPage(pageInfo) {
    this.page.pageNumber = pageInfo.offset;
    this.page.size = pageInfo.pageSize;
    this.isLoading = true;
    this.cindiClient
      .GetEntity("step-templates", "", this.page.pageNumber, this.page.size)
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
        "step-templates",
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
    }
  }
}
