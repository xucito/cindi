import { CindiClientService } from "./../../services/cindi-client.service";
import { Component, OnInit } from "@angular/core";
import { Page } from "../../shared/model/page";
import { ColumnMode } from "@swimlane/ngx-datatable";
import { DataTableComponent } from '../../shared/components/data-table/data-table.component';

@Component({
  selector: "steps",
  templateUrl: "./steps.component.html",
  styleUrls: ["./steps.component.css"]
})
export class StepsComponent  extends DataTableComponent implements OnInit {
  columns = [
    { name: "id", prop: "truncatedid" },
    { name: "stepTemplateId" },
    { name: "createdOn" },
    { name: "statusCode" },
    { name: "workflowId" }
  ];

  constructor(private cindiData: CindiClientService) {
    super();
    this.resetDataTable();
  }

  ngOnInit() {}

  setPage(pageInfo) {
    this.page.pageNumber = pageInfo.offset;
    this.page.size = pageInfo.pageSize;
    this.isLoading = true;
    this.cindiData
      .GetEntity("steps", "", this.page.pageNumber, this.page.size)
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
    this.cindiData
      .GetEntity(
        "steps",
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
}
