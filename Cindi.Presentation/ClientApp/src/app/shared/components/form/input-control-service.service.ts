import { Injectable } from '@angular/core';
import { InputBase } from './input/input-base';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class InputControlServiceService {
  constructor() { }

  toFormGroup(questions: InputBase<any>[] ) {
    let group: any = {};

    questions.forEach(question => {
      group[question.id] = new FormControl(question.value || '', Validators.required)
      /*question.required ? new FormControl(question.value || '', Validators.required)
                                              : new FormControl(question.value || '');*/
    });
    return new FormGroup(group);
  }
}
