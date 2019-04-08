import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";
import { InputControlServiceService } from "./input-control-service.service";
import { InputBase } from "./input/input-base";
import { FormGroup } from "@angular/forms";

@Component({
  selector: "app-forms",
  templateUrl: "./form.component.html",
  styleUrls: ["./form.component.css"],
  providers: [InputControlServiceService]
})
export class FormsComponent implements OnInit {
  @Output() onSubmit: EventEmitter<any> = new EventEmitter();

  _inputs: InputBase<any>[] = [];

  get inputs() {
    return this._inputs;
  }
  @Input()
  set inputs(givenInputs: InputBase<any>[]) {
    this._inputs = givenInputs;
    this.form = this.qcs.toFormGroup(this.inputs);
  }
  form: FormGroup;
  //public payLoad = '';

  @Input() readOnly: boolean = false;

  constructor(private qcs: InputControlServiceService) {}

  ngOnInit() {
    this.form = this.qcs.toFormGroup(this.inputs);
  }

  submit() {
    this.onSubmit.emit(this.form.value);
  }
}
