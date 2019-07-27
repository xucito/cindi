import { Component, Input } from "@angular/core";

@Component({
  selector: "app-nav-menu",
  templateUrl: "./nav-menu.component.html",
  styleUrls: ["./nav-menu.component.css"]
})
export class NavMenuComponent {
  isExpanded = false;

  @Input() username: string;

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
