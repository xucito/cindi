import { Component, OnInit, Input } from '@angular/core';
import { InputBase } from './input-base';
import { FormGroup } from '@angular/forms';
import { InputDataType } from '../../../utility';

@Component({
  selector: 'app-input',
  templateUrl: './input.component.html',
  styleUrls: ['./input.component.css']
})
export class InputComponent{
  InputDataType = InputDataType;

  constructor(){
  }

  @Input() readOnly: boolean = false; 
  @Input() input: InputBase<any>;
  @Input() form: FormGroup;
  get isValid() { return this.form.controls[this.input.id].valid; }
}
