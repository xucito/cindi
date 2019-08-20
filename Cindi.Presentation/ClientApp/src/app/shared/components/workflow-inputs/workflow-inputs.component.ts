import { updateWorkflow } from "./../../../entities/workflows/workflow.actions";
import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";

@Component({
  selector: "workflow-inputs",
  templateUrl: "./workflow-inputs.component.html",
  styleUrls: ["./workflow-inputs.component.scss"]
})
export class WorkflowInputsComponent implements OnInit {
  @Output() OnChange: EventEmitter<any> = new EventEmitter<any>();

  _inputDefinitions: any[];
  inputTypes = [
    "int",
    "string",
    "bool",
    "object",
    "errorMessage",
    "decimal",
    "dateTime",
    "secret"
  ];

  @Input()
  set InputDefinitions(definitions) {
    this._inputDefinitions = [];
    Object.keys(definitions).forEach(prop => {
      this._inputDefinitions.push({
        name: prop,
        value: definitions[prop]
      });
    });
  }

  constructor() {}

  ngOnInit() {}

  addInput() {
    this._inputDefinitions.push({
      name: "",
      value: {
        type: "int",
        description: ""
      }
    });
  }

  deleteDefinition(index: number) {
    this._inputDefinitions.splice(index, 1);
  }

  updateWorkflow() {
    let newDefinitions = {};
    this._inputDefinitions.forEach(element => {
      newDefinitions[element.name] = {
        type: element.value.type,
        description: element.value.description
      };
    });

    this.OnChange.emit(newDefinitions);

    console.log(newDefinitions);
  }
}
