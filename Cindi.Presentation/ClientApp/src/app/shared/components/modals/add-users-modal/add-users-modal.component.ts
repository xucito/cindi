import { Component, OnInit, Inject } from "@angular/core";
import { FormGroup, FormControl } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { DialogData } from "../secret-modal/secret-modal.component";

@Component({
  selector: "app-add-users-modal",
  templateUrl: "./add-users-modal.component.html",
  styleUrls: ["./add-users-modal.component.css"]
})
export class AddUsersModalComponent implements OnInit {
  usersForm = new FormGroup({
    username: new FormControl(""),
    password: new FormControl("")
  });
  constructor(
    public dialogRef: MatDialogRef<AddUsersModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {}

  ngOnInit() {}

  addUser() {
    this.dialogRef.close();
  }
}
