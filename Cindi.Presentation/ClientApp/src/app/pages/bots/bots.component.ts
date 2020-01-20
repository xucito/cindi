import { CindiClientService } from "./../../services/cindi-client.service";
import { State } from "./../../entities/step-templates/step-template.reducer";
import { Component, OnInit } from "@angular/core";
import { Store, select } from "@ngrx/store";
import { Subscription } from "rxjs";
import { selectAll } from "../../entities/bot-keys/bot-key.reducer";
import {
  NbToastrService,
  NbToastRef,
  NbToastrConfig,
  NbGlobalPhysicalPosition
} from "@nebular/theme";
import { loadBotKeys } from "../../entities/bot-keys/bot-key.actions";

@Component({
  selector: "bots",
  templateUrl: "./bots.component.html",
  styleUrls: ["./bots.component.css"]
})
export class BotsComponent implements OnInit {
  botKeys$: Subscription;
  botKeys: any;

  constructor(
    private store: Store<State>,
    private cindiClient: CindiClientService,
    private toastrService: NbToastrService
  ) {
    this.botKeys$ = this.store.pipe(select(selectAll)).subscribe(result => {
      this.botKeys = result;
    });
  }

  ngOnInit() {}

  toggleDisable(id: string, isDisabled: boolean) {
    const config: Partial<NbToastrConfig> = {
      status: "success",
      destroyByClick: true,
      duration: 3000,
      hasIcon: false,
      position: NbGlobalPhysicalPosition.TOP_RIGHT,
      preventDuplicates: true
    };

    this.cindiClient.UpdateBotKey(id, isDisabled).subscribe(result => {
      const toastRef: NbToastRef = this.toastrService.show(
        "success",
        "Created step " + result.result.id,
        config
      );
      this.store.dispatch(loadBotKeys());
    });
  }

  deleteBotKey(id) {
    const config: Partial<NbToastrConfig> = {
      status: "success",
      destroyByClick: true,
      duration: 3000,
      hasIcon: false,
      position: NbGlobalPhysicalPosition.TOP_RIGHT,
      preventDuplicates: true
    };
    this.cindiClient.DeleteBotKey(id).subscribe(result => {
      const toastRef: NbToastRef = this.toastrService.show(
        "success",
        "Deleted botkey " + result.objectRefId,
        config
      );
      this.store.dispatch(loadBotKeys());
    });
  }
}
