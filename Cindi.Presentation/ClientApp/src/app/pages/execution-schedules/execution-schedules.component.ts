import { NotificationService } from "./../../shared/services/notification.service";
import { Component, OnInit } from "@angular/core";
import { Page } from "../../shared/model/page";
import { CindiClientService } from "../../services/cindi-client.service";
import { NbGlobalPhysicalPosition } from "@nebular/theme";

@Component({
  selector: "execution-schedules",
  templateUrl: "./execution-schedules.component.html",
  styleUrls: ["./execution-schedules.component.css"]
})
export class ExecutionSchedulesComponent implements OnInit {
  isLoading = false;
  rows = [];
  page: Page = new Page();
  columns = [
    { name: "name" },
    { name: "description" },
    { name: "schedule" },
    { name: "isDisabled" },
    { name: "nextRun" },
    { name: "executionTemplateName" },
    { name: "Start", icon: "play-circle", action: "play", type: "button" },
    { name: "Pause", icon: "pause-circle", action: "pause", type: "button" },
    { name: "Delete", icon: "close-circle", action: "delete", type: "button" }
  ];

  ngOnInit() {}

  constructor(
    private cindiClient: CindiClientService,
    private notificationService: NotificationService
  ) {
    this.setPage({
      offset: 0,
      pageSize: 10
    });
  }

  setPage(pageInfo) {
    this.page.pageNumber = pageInfo.offset;
    this.page.size = pageInfo.pageSize;
    this.isLoading = true;
    this.cindiClient
      .GetEntity(
        "execution-schedules",
        "",
        this.page.pageNumber,
        this.page.size
      )
      .subscribe(
        pagedData => {
          this.rows = pagedData.result;
          this.page.totalElements = pagedData.count;
          this.page.totalPages = pagedData.count / this.page.size;
          pagedData.result.forEach(element => {
            if(!element.isDisabled)
            {
              element.hideplay = true;
            }
            else
            {

              element.hidepause = true;
            }
          });
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
        "execution-schedules",
        "",
        this.page.pageNumber,
        this.page.size,
        sortStatement
      )
      .subscribe(
        pagedData => {
          pagedData.result.forEach(element => {
            if(!element.isDisabled)
            {
              element.hideplay = true;
            }
            else
            {

              element.hidepause = true;
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

  onAction(event) {
    switch (event.action.action) {
      case "play":
        this.toggleDisable(event.value.name, false);
        break;
      case "pause":
        this.toggleDisable(event.value.name, true);
        break;
      case "delete":
        this.deleteBotKey(event.value.name);
        break;
    }
  }

  toggleDisable(id: string, isDisabled: boolean) {
    this.cindiClient
      .UpdateExecutionSchedule(
        id,
        {
          IsDisabled: isDisabled
        },
        true
      )
      .subscribe(
        result => {
          const toastRef = this.notificationService.show(
            (isDisabled ? "Disabled " : "Enabled ") + " Bot" + result.result.id,
            "success"
          );
          this.reload();
        },
        error => {
          console.error(error), () => {};
        }
      );
  }

  deleteBotKey(id) {
    this.cindiClient.DeleteBotKey(id).subscribe(
      result => {
        this.notificationService.show(
          "Deleted Bot " + result.objectRefId,
          "success"
        );
        //this.reload();
      },
      error => {
        console.error(error),
          () => {
          };
      }
    );
  }

  reload() {
    this.setPage({
      offset: this.page.pageNumber,
      pageSize: this.page.size
    });
  }
}
