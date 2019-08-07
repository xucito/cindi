import { Component, OnInit } from "@angular/core";
import { Store, select } from "@ngrx/store";
import {
  State,
  selectAll
} from "../../entities/global-values/global-value.reducer";

@Component({
  selector: "global-values",
  templateUrl: "./global-values.component.html",
  styleUrls: ["./global-values.component.scss"]
})
export class GlobalValuesComponent implements OnInit {
  globalValues$;
  globalValues;

  constructor(private workflowTemplateStore: Store<State>) {
    this.globalValues$ = workflowTemplateStore
      .pipe(select(selectAll))
      .subscribe(result => {
        this.globalValues = result;
      });
  }

  ngOnInit() {}
}
