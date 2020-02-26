import { CindiClientService } from "./../../services/cindi-client.service";
import {
  Component,
  OnInit,
  Input,
  TemplateRef,
  ViewChild,
  ElementRef
} from "@angular/core";
import { Page } from "../../shared/model/page";
import { ColumnMode } from "@swimlane/ngx-datatable";
import { DataTableComponent } from "../../shared/components/data-table/data-table.component";
import { State } from "../../entities/steps/step.reducer";
import * as fromStepTemplate from "../../entities/step-templates/step-template.reducer";
import { Store, select } from "@ngrx/store";
import { take } from "rxjs/operators";
import { getStepTemplate } from "../../entities/step-templates/step-template.reducer";
import {
  NbWindowService,
  NbToastrConfig,
  NbGlobalPhysicalPosition,
  NbToastRef,
  NbToastrService
} from "@nebular/theme";
import { ConvertTemplateToInputs } from "../../shared/utility/data-mapper";
import { loadSteps } from "../../entities/steps/step.actions";
import { StepStatuses } from "../../entities/steps/step-statuses.enum";
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: "steps",
  templateUrl: "./steps.component.html",
  styleUrls: ["./steps.component.css"]
})
export class StepsComponent implements OnInit {
  @ViewChild("stepView", { static: true }) stepView: ElementRef;

  @ViewChild("newStepForm", { static: true }) newStepForm: ElementRef;

  columns = [
    { name: "id", prop: "truncatedid" },
    { name: "stepTemplateId" },
    { name: "createdOn" },
    { name: "status" },
    { name: "workflowId" },
    { name: "Clone", icon: "copy", action: "clone", type: "button" },
    { name: "View", icon: "eye-outline", action: "view", type: "button" },
    { name: "Stop", icon: "close-circle", action: "stop", type: "button" },
    { name: "Pause", icon: "pause-circle", action: "pause", type: "button" },
    { name: "Play", icon: "play-circle", action: "play", type: "button" },
    { name: "Retry", icon: "sync-outline", action: "retry", type: "button" }
  ];

  rows = [];
  page: Page = new Page();
  isLoading = false;

  constructor(
    private cindiData: CindiClientService,
    private store: Store<State>,
    private stepTemplateStore: Store<fromStepTemplate.State>,
    private windowService: NbWindowService,
    private toastrService: NbToastrService,
    private notificationService: NotificationService
  ) {
    this.setPage({
      offset: 0,
      pageSize: 10
    });
  }

  ngOnInit() {}

  setPage(pageInfo) {
    this.page.pageNumber = pageInfo.offset;
    this.page.size = pageInfo.pageSize;
    this.isLoading = true;
    this.cindiData
      .GetEntity("steps", "", this.page.pageNumber, this.page.size, this.page.sortStatement)
      .subscribe(
        pagedData => {
          pagedData.result.forEach(element => {
            element.truncatedid = element.id.slice(0, 8);

            if (
              element.status != StepStatuses.Suspended &&
              element.status != StepStatuses.Unassigned &&
              element.status != StepStatuses.Assigned
            ) {
              element.hidestop = true;
              element.hidepause = true;
              element.hideplay = true;
              element.hideretry = true;
            } else if (element.status == StepStatuses.Assigned) {
              element.hidepause = true;
              element.hideplay = true;
            } else if (element.status == StepStatuses.Suspended) {
              element.hidepause = true;
              element.hideretry = true;
            } else if (element.status == StepStatuses.Unassigned) {
              element.hideplay = true;
              element.hideretry = true;
            }
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

    this.page.sortStatement = sortStatement;
    this.cindiData
      .GetEntity(
        "steps",
        "",
        this.page.pageNumber,
        this.page.size,
        this.page.sortStatement
      )
      .subscribe(
        pagedData => {
          pagedData.result.forEach(element => {
            element.truncatedid = element.id.slice(0, 8);
            if (
              element.status != StepStatuses.Suspended &&
              element.status != StepStatuses.Unassigned &&
              element.status != StepStatuses.Assigned
            ) {
              element.hidestop = true;
              element.hidepause = true;
              element.hideplay = true;
            } else if (element.status == StepStatuses.Assigned) {
              element.hidepause = true;
            } else if (element.status == StepStatuses.Suspended) {
              element.hidepause = true;
            } else if (element.status == StepStatuses.Unassigned) {
              element.hideplay = true;
            }
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

  openWindow(contentTemplate, step) {
    this.stepTemplateStore
      .pipe(select(getStepTemplate, { referenceId: step.stepTemplateId }))
      .pipe(take(1))
      .subscribe(result => {
        this.windowService.open(contentTemplate, {
          title: "Create Step: " + result.referenceId,
          context: {
            inputs: ConvertTemplateToInputs(result, step, undefined),
            stepTemplate: result
          }
        });
      });
  }

  openStepViewWindow(contentTemplate, step) {
    this.stepTemplateStore
      .pipe(select(getStepTemplate, { referenceId: step.stepTemplateId }))
      .pipe(take(1))
      .subscribe(result => {
        this.windowService.open(contentTemplate, {
          title: "Create Step: " + result.referenceId,
          context: {
            step: step,
            stepTemplate: result
          }
        });
      });
  }

  onAction(event) {
    switch (event.action.action) {
      case "clone":
        this.openWindow(this.newStepForm, event.value);
        break;
      case "view":
        this.openStepViewWindow(this.stepView, event.value);
        break;
      case "stop":
        this.updateStep(event.value.id, "cancelled");
        break;
      case "pause":
        this.updateStep(event.value.id, "suspended");
        break;
      case "play":
        this.updateStep(event.value.id, "unassigned");
        break;
      case "retry":
        this.updateStep(event.value.id, "unassigned");
        break;
    }
  }

  submitNewStep(event) {
    let submit = this.cindiData
      .PostStep({
        name: "",
        description: "",
        stepTemplateId: event.props.referenceId,
        inputs: event.value
      })
      .subscribe(result => {
        const config: Partial<NbToastrConfig> = {
          status: "success",
          destroyByClick: true,
          duration: 3000,
          hasIcon: false,
          position: NbGlobalPhysicalPosition.TOP_RIGHT,
          preventDuplicates: true
        };

        const toastRef: NbToastRef = this.toastrService.show(
          "success",
          "Created step " + result.result.id,
          config
        );
        console.log("Submitted");
      });
    console.log(event);
  }

  updateStep(stepId: string, status: string) {
    const config: Partial<NbToastrConfig> = {
      status: "danger",
      destroyByClick: true,
      duration: 3000,
      hasIcon: false,
      position: NbGlobalPhysicalPosition.TOP_RIGHT,
      preventDuplicates: true
    };

    this.cindiData.PutStep(stepId, status).subscribe(
      result => {
        const config: Partial<NbToastrConfig> = {
          status: "success",
          destroyByClick: true,
          duration: 3000,
          hasIcon: false,
          position: NbGlobalPhysicalPosition.TOP_RIGHT,
          preventDuplicates: true
        };

        const toastRef: NbToastRef = this.toastrService.show(
          "success",
          status + " step " + result.result.id,
          config
        );

        this.store.dispatch(loadSteps({ status: undefined }));
        this.setPage({
          offset: this.page.pageNumber,
          pageSize: this.page.size
        });
      },
      error => {
        const toastRef: NbToastRef = this.toastrService.show(
          "error",
          "Failed to update step " +
            stepId +
            " with status " +
            status +
            " with error " +
            error.error,
          config
        );
      }
    );
  }
}
