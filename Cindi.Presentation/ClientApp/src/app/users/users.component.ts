import { Component, OnInit } from "@angular/core";
import { NodeDataService } from "../services/node-data.service";
import { MatDialog } from "@angular/material";
import { AddUsersModalComponent } from "../shared/components/modals/add-users-modal/add-users-modal.component";

@Component({
  selector: "app-users",
  templateUrl: "./users.component.html",
  styleUrls: ["./users.component.css"]
})
export class UsersComponent implements OnInit {
  columnsToDisplay = ["username", "email"];
  public users: any[] = [];

  constructor(public nodeData: NodeDataService, public dialog: MatDialog) {
    nodeData.GetUsers().subscribe(result => {
      this.users = result.result;
    });
  }

  ngOnInit() {}

  addUser() {
    const dialogRef = this.dialog.open(AddUsersModalComponent, {
      width: "250px",
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(result);
      this.nodeData.PostUser(result).subscribe(result => {
        console.log(result);
        this.nodeData.GetUsers().subscribe(result => {
          this.users = result.result;
        });
      });
    });
  }
}
