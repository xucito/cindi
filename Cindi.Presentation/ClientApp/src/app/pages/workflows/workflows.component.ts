import { Component, OnInit } from "@angular/core";
import { ColumnMode } from '@swimlane/ngx-datatable';
import { Page } from '../../shared/model/page';
import { CindiClientService } from '../../services/cindi-client.service';

@Component({
  selector: "workflows",
  templateUrl: "./workflows.component.html",
  styleUrls: ["./workflows.component.css"]
})
export class WorkflowsComponent implements OnInit {
  ColumnMode = ColumnMode;
  page = new Page();
  rows = [];
  isLoading = false;
  columns = [
    { name: "id", prop: "truncatedid" },
    { name: "workflowTemplateId" },
    { name: "createdOn" },
    { name: "status" }
  ];

  constructor(private cindiData: CindiClientService) {
    this.page.pageNumber = 0;
    this.page.size = 20;
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
      .GetEntity("workflows", "", this.page.pageNumber, this.page.size)
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
    this.isLoading = true;
    var sortStatement = "";

    event.sorts.forEach(element => {
      sortStatement += element.prop + ":" + (element.dir == "desc" ? -1 : 1);
    });
    this.cindiData
      .GetEntity(
        "workflows",
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
