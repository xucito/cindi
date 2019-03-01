import { Component, OnInit, Input } from '@angular/core';
import { InputControlServiceService } from './input-control-service.service';
import { InputBase } from './input/input-base';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-forms',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css'],
  providers: [ InputControlServiceService ]
})
export class FormsComponent implements OnInit {

  @Input() inputs: InputBase<any>[] = [];
  form: FormGroup;
  payLoad = '';
 
  constructor(private qcs: InputControlServiceService) {  }
 
  ngOnInit() {
    this.form = this.qcs.toFormGroup(this.inputs);
  }
 
  onSubmit() {
    this.payLoad = JSON.stringify(this.form.value);
  }

}
