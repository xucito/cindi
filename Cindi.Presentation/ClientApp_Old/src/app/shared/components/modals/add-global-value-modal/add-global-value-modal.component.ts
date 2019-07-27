import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { DialogData } from '../secret-modal/secret-modal.component';
import { FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-add-global-value-modal',
  templateUrl: './add-global-value-modal.component.html',
  styleUrls: ['./add-global-value-modal.component.css']
})
export class AddGlobalValueModalComponent implements OnInit {
  globalValuesForm = new FormGroup({
    name: new FormControl(""),
    type: new FormControl(""),
    description: new FormControl(""),
    value: new FormControl("")
  });
  constructor(
    public dialogRef: MatDialogRef<AddGlobalValueModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  ngOnInit() {}

  addGlobalValue() {
    this.dialogRef.close();
  }

}
