import { CindiClientService } from './../../services/cindi-client.service';
import { Component, OnInit } from "@angular/core";
import { Store, select } from "@ngrx/store";
import {
  State,
  selectAll
} from "../../entities/global-values/global-value.reducer";
import { DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { Page } from "../../shared/model/page";
@Component({
  selector: "global-values",
  templateUrl: "./global-values.component.html",
  styleUrls: ["./global-values.component.scss"]
})
export class GlobalValuesComponent  {
  isLoading = false;
  rows = [];
  page: Page = new Page();
  globalValues$;
  globalValues;
  disableButtons = false;
  columns = [
    { name: "name", },
    { name: "type", },
    { name: "description" },
    { name: "status" },
    { name: "value" }
  ];

  constructor(private cindiClient: CindiClientService) {
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
      .GetEntity("global-values", "", this.page.pageNumber, this.page.size)
      .subscribe(
        pagedData => {
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

  onAction(event)
  {

  }

}
