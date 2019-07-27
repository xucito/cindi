import { Component, OnInit } from '@angular/core';
import { InputBase } from '../input-base';

export class IntInput extends InputBase<string>  {
  controlType = 'textbox';
 // type: number;

  constructor(options: {} = {}) {
    super(options);
    //this.type = options['type'] || '';
  }
}
