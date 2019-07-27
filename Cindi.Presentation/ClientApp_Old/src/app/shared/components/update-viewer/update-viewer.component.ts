import { Component, OnInit, Input } from "@angular/core";

@Component({
  selector: "update-viewer",
  templateUrl: "./update-viewer.component.html",
  styleUrls: ["./update-viewer.component.css"]
})
export class UpdateViewerComponent implements OnInit {
  _journal: any;
  ProgressValue = 0;

  @Input()
  set journal(journal) {
    this._journal = journal;

    for (var i = 0; i < this._journal.entries.length; i++) {
      this._journal.entries[i].updates = this._journal.entries[
        i
      ].updates.filter(u => u.fieldName != "logs");
    }

    this._journal.entries = this._journal.entries.filter(
      e => e.updates.length > 0
    );
  }

  constructor() {}

  ngOnInit() {}
}
