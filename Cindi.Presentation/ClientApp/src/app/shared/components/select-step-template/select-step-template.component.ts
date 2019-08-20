import { Component, OnInit, Input, EventEmitter, Output } from "@angular/core";

@Component({
  selector: "select-step-template",
  templateUrl: "./select-step-template.component.html",
  styleUrls: ["./select-step-template.component.css"]
})
export class SelectStepTemplateComponent implements OnInit {
  @Input() stepTemplates: any[] = [];
  @Output() onSelect: EventEmitter<any> = new EventEmitter<any>();

  constructor() {}

  ngOnInit() {}
}
