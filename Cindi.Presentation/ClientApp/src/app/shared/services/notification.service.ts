import { Injectable } from "@angular/core";
import {
  NbToastrService,
  NbToastrConfig,
  NbToastRef,
  NbGlobalPhysicalPosition
} from "@nebular/theme";

@Injectable({
  providedIn: "root"
})
export class NotificationService {
  constructor(private toastrService: NbToastrService) {}

  notifications = {
    success: {
      status: "success",
      destroyByClick: true,
      duration: 3000,
      hasIcon: false,
      position: NbGlobalPhysicalPosition.TOP_RIGHT,
      preventDuplicates: true
    },
    error: {
      status: "danger",
      destroyByClick: true,
      duration: 3000,
      hasIcon: false,
      position: NbGlobalPhysicalPosition.TOP_RIGHT,
      preventDuplicates: true
    }
  };

  show(message: any, type: string, title?: any): NbToastRef {
    return this.toastrService.show(message, title, this.notifications[type]);
  }
}
