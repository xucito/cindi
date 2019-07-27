import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { CindiClientService } from "./cindi-client.service";

@Injectable({
  providedIn: "root"
})
export class AppStateService {
  User: BehaviorSubject<any> = new BehaviorSubject({});
  Steps: BehaviorSubject<any> = new BehaviorSubject([]);

  constructor(private cindiClient: CindiClientService) {}

  public UpdateUser(user: any) {
    this.User.next(user);
  }

  public UpdateSteps() {
    this.cindiClient.GetSteps().subscribe(result => {
      let newResult = this.Steps.value;
      newResult.concat(result);
      this.Steps.push(result);
    });
  }
}
