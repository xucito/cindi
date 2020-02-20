import { Component, OnInit } from "@angular/core";
import { ColumnMode } from "@swimlane/ngx-datatable";
import { Page } from "../../model/page";

export abstract class DataTableComponent {
  ColumnMode = ColumnMode;
  page = new Page();
  rows = [];
  isLoading = false;
  columns = [];

  constructor() {}

  abstract setPage(pageInfo: any): void;
  abstract onSort(event: any): void;
  reload() {
    this.setPage({
      offset: this.page.pageNumber,
      pageSize: this.page.size
    });
  }

  resetDataTable() {
    this.page.pageNumber = 0;
    this.page.size = 20;
    this.setPage({
      offset: 0,
      pageSize: 10
    });
  }
}
