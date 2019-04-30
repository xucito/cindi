import { Component, OnInit, Inject} from '@angular/core';
import { MatBottomSheetRef, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-secret-modal',
  templateUrl: './secret-modal.component.html',
  styleUrls: ['./secret-modal.component.css']
})
export class SecretModalComponent implements OnInit {

  constructor(public bottomSheetRef: MatDialogRef<SecretModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData) { }

  ngOnInit() {
  }

}

export interface DialogData {
  secret: string
}