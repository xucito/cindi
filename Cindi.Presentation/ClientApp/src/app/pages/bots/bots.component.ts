import { filter } from "rxjs/operators";
import { CindiClientService } from "./../../services/cindi-client.service";
import { State } from "./../../entities/step-templates/step-template.reducer";
import { Component, OnInit } from "@angular/core";
import { Store, select } from "@ngrx/store";
import { Subscription } from "rxjs";
import { selectAll } from "../../entities/bot-keys/bot-key.reducer";
import {
  NbToastrService,
  NbToastRef,
  NbToastrConfig,
  NbGlobalPhysicalPosition
} from "@nebular/theme";
import { loadBotKeys } from "../../entities/bot-keys/bot-key.actions";
import { DataTableComponent } from "../../shared/components/data-table/data-table.component";
import { Page } from "../../shared/model/page";

@Component({
  selector: "bots",
  templateUrl: "./bots.component.html",
  styleUrls: ["./bots.component.css"]
})
export class BotsComponent {
  isLoading = false;
  rows = [];
  page: Page = new Page();
  botKeys$: Subscription;
  botKeys: any;
  columns = [
    { name: "id", prop: "truncatedid" },
    { name: "botName" },
    { name: "registeredOn" },
    { name: "isDisabled" },
    { name: "Start", icon: "play-circle", action: "play", type: "button" },
    { name: "Pause", icon: "pause-circle", action: "pause", type: "button" },
    { name: "Delete", icon: "close-circle", action: "delete", type: "button" }
  ];
  disableButtons = false;

  constructor(
    private store: Store<State>,
    private cindiClient: CindiClientService,
    private toastrService: NbToastrService
  ) {
    this.setPage({
      offset: 0,
      pageSize: 10
    });
  }

  onAction(event) {
    switch (event.action.action) {
      case "play":
        this.toggleDisable(event.value.id, false);
        break;
      case "pause":
        this.toggleDisable(event.value.id, true);
        break;
      case "delete":
        this.deleteBotKey(event.value.id);
        break;
    }
  }

  toggleDisable(id: string, isDisabled: boolean) {
    this.disableButtons = true;
    const config: Partial<NbToastrConfig> = {
      status: "success",
      destroyByClick: true,
      duration: 3000,
      hasIcon: false,
      position: NbGlobalPhysicalPosition.TOP_RIGHT,
      preventDuplicates: true
    };

    this.cindiClient.UpdateBotKey(id, isDisabled).subscribe(
      result => {
        const toastRef: NbToastRef = this.toastrService.show(
          "success",
          (isDisabled ? "Disabled " : "Enabled ") + " Bot" + result.result.id,
          config
        );
        this.reload();
      },
      error => {
        console.error(error),
          () => {
            this.disableButtons = false;
          };
      }
    );
  }

  deleteBotKey(id) {
    this.disableButtons = true;
    const config: Partial<NbToastrConfig> = {
      status: "success",
      destroyByClick: true,
      duration: 3000,
      hasIcon: false,
      position: NbGlobalPhysicalPosition.TOP_RIGHT,
      preventDuplicates: true
    };
    this.cindiClient.DeleteBotKey(id).subscribe(
      result => {
        const toastRef: NbToastRef = this.toastrService.show(
          "success",
          "Deleted Bot " + result.objectRefId,
          config
        );
        this.reload();
      },
      error => {
        console.error(error),
          () => {
            this.disableButtons = false;
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
  setPage(pageInfo) {
    this.page.pageNumber = pageInfo.offset;
    this.page.size = pageInfo.pageSize;
    this.isLoading = true;
    this.cindiClient
      .GetEntity("bot-Keys", "", this.page.pageNumber, this.page.size)
      .subscribe(
        pagedData => {
          pagedData.result.forEach(element => {
            element.truncatedid = element.id.slice(0, 8);
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
        "bot-Keys",
        "",
        this.page.pageNumber,
        this.page.size,
        sortStatement
      )
      .subscribe(
        pagedData => {
          pagedData.result.forEach(element => {
            element.truncatedid = element.id.slice(0, 8);
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
}
