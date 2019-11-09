import {
  Component,
  OnInit,
  Input,
  OnChanges,
  EventEmitter,
  Output
} from "@angular/core";

@Component({
  selector: "mapping-selector",
  templateUrl: "./mapping-selector.component.html",
  styleUrls: ["./mapping-selector.component.css"]
})
export class MappingSelectorComponent implements OnInit, OnChanges {
  @Output() onMappingChange = new EventEmitter<any>();

  ngOnChanges(changes: import("@angular/core").SimpleChanges): void {
    if (this.mapping != undefined && this._options != undefined) {
      this.filterOptions();
    }
  }
  //_mapping: any;
  _options: any[];
  @Input() mapping;

  @Input()
  set options(options) {
    this._options = options;
  }

  increasePriority(i) {}

  filterOptions() {
    let alreadyMappedIds = [];
    this.mapping.outputReferences.forEach(element => {
      alreadyMappedIds.push(element.stepName + ":" + element.outputId);
    });
    this._options.forEach(option => {
      option.mappings = option.mappings.filter(
        mapping =>
          alreadyMappedIds.indexOf(option.stepRefId + ":" + mapping.name) == -1
      );
    });
  }

  addOutputReference(option, mapping) {
    console.log(option);
    console.log(mapping);

    this.mapping.outputReferences.push({
      stepName: option.stepRefId,
      outputId: mapping.name,
      priority: 0
    });
    this.filterOptions();
    this.onMappingChange.emit(this.mapping);
  }

  constructor() {}

  ngOnInit() {}
}
