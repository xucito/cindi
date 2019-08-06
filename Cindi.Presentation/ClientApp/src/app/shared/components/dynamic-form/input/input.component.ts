import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { InputBase } from './input-base';
import { FormGroup } from '@angular/forms';
import { InputAction } from './input-action';
import { InputDataType } from '../../../enums/input-data-types';

@Component({
  selector: 'app-input',
  templateUrl: './input.component.html',
  styleUrls: ['./input.component.css']
})
export class InputComponent {
  InputDataType = InputDataType;

  constructor() {}

  @Input() readOnly: boolean = false;
  @Input() input: InputBase<any>;
  @Input() form: FormGroup;
  get isValid() {
    return this.form.controls[this.input.id].valid;
  }

  @Output() onInputAction: EventEmitter<InputAction> = new EventEmitter();

  UnencryptField() {
    const inputAction: InputAction = {
      action: 'unencrypt',
      inputId: this.input.id
    };
    this.onInputAction.emit(inputAction);
  }
}
