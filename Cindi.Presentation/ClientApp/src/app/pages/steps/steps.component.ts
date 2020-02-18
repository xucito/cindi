import { CindiClientService } from "./../../services/cindi-client.service";
import { Component, OnInit } from "@angular/core";
import { Page } from "../../shared/model/page";
import { ColumnMode } from "@swimlane/ngx-datatable";

@Component({
  selector: "steps",
  templateUrl: "./steps.component.html",
  styleUrls: ["./steps.component.css"]
})
export class StepsComponent implements OnInit {
  ColumnMode = ColumnMode;
  page = new Page();
  rows = [];
  columns = [
    { name: "id", prop: "truncatedid" },
    { name: "stepTemplateId" },
    { name: "createdOn" },
    { name: "statusCode" },
    { name: "workflowId" }
  ];

  constructor(private cindiData: CindiClientService) {
    this.page.pageNumber = 0;
    this.page.size = 20;
    this.setPage({
      offset: 0,
      pageSize: 20
    });
  }

  ngOnInit() {}

  setPage(pageInfo) {
    this.page.pageNumber = pageInfo.offset;
    this.page.size = pageInfo.pageSize;

    // cache results
    // if(this.cache[this.page.pageNumber]) return;

    this.cindiData
      .GetSteps("", this.page.pageNumber, this.page.size)
      .subscribe(pagedData => {
        // calc start
        const start = this.page.pageNumber * this.page.size;

        pagedData.result.forEach(element => {
          element.truncatedid = element.id.slice(0, 8);
        });

        // copy rows
        // const rows = [...this.rows];

        // insert rows into new position
        //rows.splice(start, 0, ...pagedData.result);

        // set rows to our new rows
        this.rows = pagedData.result;

        this.page.totalElements = pagedData.count;

        this.page.totalPages = pagedData.count / this.page.size;
        // add flag for results
        // this.cache[this.page.pageNumber] = true;
      });
  }

  onSort(event) {
    // event was triggered, start sort sequence
    console.log("Sort Event", event);
    // emulate a server request with a timeout
    setTimeout(() => {
      var sortStatement = "";

      event.sorts.forEach(element => {
        sortStatement += element.prop + ":" + (element.dir == "desc" ? -1 : 1);
      });
      this.cindiData
        .GetSteps("", this.page.pageNumber, this.page.size, sortStatement)
        .subscribe(pagedData => {
          // calc start
          const start = this.page.pageNumber * this.page.size;

          pagedData.result.forEach(element => {
            element.truncatedid = element.id.slice(0, 8);
          });

          this.rows = pagedData.result;

          this.page.totalElements = pagedData.count;

          this.page.totalPages = pagedData.count / this.page.size;
          // add flag for results
          // this.cache[this.page.pageNumber] = true;
        });
    }, 1000);
  }
}
