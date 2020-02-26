import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";
import { ColumnMode } from "@swimlane/ngx-datatable";
import { Page } from "../../model/page";

@Component({
  selector: "data-table",
  templateUrl: "./data-table.component.html",
  styleUrls: ["./data-table.component.css"]
})
export class DataTableComponent {
  ColumnMode = ColumnMode;
  isLoading = false;

  @Input() columns: any[] = [];
  @Input() rows: any[] = [];
  @Input() page: Page = new Page();

  @Output() onPageChange = new EventEmitter();
  @Output() onSortChange = new EventEmitter();
  @Output() onAction = new EventEmitter();

  constructor() {
  }

  reload() {
    this.onPageChange.emit({
      offset: this.page.pageNumber,
      pageSize: this.page.size
    });
  }

  getDisplayColumns() {
    return this.columns.filter(
      c => ((c.type == undefined))
    );
  }

  getButtonColumns() {
    return this.columns.filter(
      c => ((c.type == 'button'))
    );
  }


  setPage(event)
  {
    this.onPageChange.emit(event);
  }

  onSort(event)
  {
    this.onSortChange.emit(event);
  }

  emitAction(action, row)
  {
    console.log("Detected action ")
    this.onAction.emit({
      action: action,
      value: row
    });
  }
}
