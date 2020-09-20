import { Component, OnInit } from '@angular/core';
import { CindiClientService } from '../../services/cindi-client.service';
import { Page } from '../../shared/model/page';

@Component({
  selector: 'execution-templates',
  templateUrl: './execution-templates.component.html',
  styleUrls: ['./execution-templates.component.css']
})
export class ExecutionTemplatesComponent implements OnInit {


  isLoading = false;
  rows = [];
  page: Page = new Page();
  columns = [
    { name: "name", },
    { name: "description" },
    { name: "schedule", },
    { name: "isDisabled" },
    { name: "nextRun" },
    { name: "createdOn" }
  ];

  ngOnInit() {
  }

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
      .GetEntity("execution-templates", "", this.page.pageNumber, this.page.size)
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
        "execution-templates",
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
