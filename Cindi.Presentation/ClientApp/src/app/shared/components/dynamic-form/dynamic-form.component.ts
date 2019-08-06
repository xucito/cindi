import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { InputBase } from './input/input-base';
import { InputAction } from './input/input-action';
import { FormGroup } from '@angular/forms';
import { InputControlService } from './input-control.service';

@Component({
  selector: 'dynamic-form',
  templateUrl: './dynamic-form.component.html',
  styleUrls: ['./dynamic-form.component.css']
})
export class DynamicFormComponent implements OnInit {
  @Output() onSubmit: EventEmitter<any> = new EventEmitter();
  @Output() onInputAction: EventEmitter<InputAction> = new EventEmitter();

  _inputs: InputBase<any>[] = [];

  get inputs() {
    return this._inputs;
  }
  @Input()
  set inputs(givenInputs: InputBase<any>[]) {
    this._inputs = givenInputs;
    this.form = this.qcs.toFormGroup(this.inputs);
  }

  _props: any;

  @Input()
  set additionalProps(props: any) {
    this._props = props;
  }

  form: FormGroup;
  //public payLoad = '';

  @Input() readOnly: boolean = false;

  constructor(private qcs: InputControlService) {}

  ngOnInit() {
    this.form = this.qcs.toFormGroup(this.inputs);
  }

  submit() {
    this.onSubmit.emit(
      {
        props: this._props,
        value: this.form.value
      });
  }

  actionInput(event) {
   // console.log(event);
    this.onInputAction.emit(event);
  }

}
