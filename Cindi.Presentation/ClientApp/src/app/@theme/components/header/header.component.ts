import { Component, OnDestroy, OnInit } from "@angular/core";
import {
  NbMediaBreakpointsService,
  NbMenuService,
  NbSidebarService,
  NbThemeService
} from "@nebular/theme";
import { map, takeUntil } from "rxjs/operators";
import { Subject, Subscription, Observable } from "rxjs";
import { Store, select } from "@ngrx/store";
import { State } from "../../../reducers";
import * as fromUser from "../../../entities/user.reducer";
import { User } from "../../../entities/user.model";
@Component({
  selector: "ngx-header",
  styleUrls: ["./header.component.scss"],
  templateUrl: "./header.component.html"
})
export class HeaderComponent implements OnInit, OnDestroy {
  private destroy$: Subject<void> = new Subject<void>();
  userPictureOnly: boolean = false;
  user: any;
  user$: Subscription;
  themes = [
    {
      value: "default",
      name: "Light"
    },
    {
      value: "dark",
      name: "Dark"
    },
    {
      value: "cosmic",
      name: "Cosmic"
    },
    {
      value: "corporate",
      name: "Corporate"
    }
  ];

  currentTheme = "dark";

  userMenu = [{ title: "Profile" }, { title: "Log out" }];

  constructor(
    private sidebarService: NbSidebarService,
    private menuService: NbMenuService,
    private themeService: NbThemeService,
    private breakpointService: NbMediaBreakpointsService,
    private store: Store<State>
  ) {}

  ngOnInit() {
    this.currentTheme = this.themeService.currentTheme;

    this.user$ = this.store
      .select(state => state.currentUser)
      .subscribe(result => {
        this.user = result;
      });

    const { xl } = this.breakpointService.getBreakpointsMap();
    this.themeService
      .onMediaQueryChange()
      .pipe(
        map(([, currentBreakpoint]) => currentBreakpoint.width < xl),
        takeUntil(this.destroy$)
      )
      .subscribe(
        (isLessThanXl: boolean) => (this.userPictureOnly = isLessThanXl)
      );

    this.themeService
      .onThemeChange()
      .pipe(
        map(({ name }) => name),
        takeUntil(this.destroy$)
      )
      .subscribe(themeName => (this.currentTheme = themeName));
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  changeTheme(themeName: string) {
    this.themeService.changeTheme(themeName);
  }

  toggleSidebar(): boolean {
    this.sidebarService.toggle(true, "menu-sidebar");

    return false;
  }

  navigateHome() {
    this.menuService.navigateHome();
    return false;
  }
}
