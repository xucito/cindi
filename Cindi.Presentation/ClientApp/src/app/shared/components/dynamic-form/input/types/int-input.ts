import { Component, OnInit } from '@angular/core';
import { InputBase } from '../input-base';

export class IntInput extends InputBase<string>  {
  controlType = 'textbox';

  constructor(options: {} = {}) {
    super(options);
  }
}
