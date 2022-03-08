import { NbMenuItem } from "@nebular/theme";

export const MENU_ITEMS: NbMenuItem[] = [
  {
    title: "Home",
    icon: "home-outline",
    link: "/pages/dashboard",
    home: true
  },
  {
    title: "Steps",
    icon: "arrow-circle-right-outline",
    link: "/pages/steps"
  },
  {
    title: "Workflows",
    icon: "shuffle-2-outline",
    link: "/pages/workflows"
  },
  {
    title: "Execution-Schedules",
    icon: "clock-outline",
    link: "/pages/execution-schedules"
  },
  // {
  //   title: "Monitoring",
  //   icon: "heart-outline",
  //   link: "/pages/monitoring"
  // },
  // {
  //   title: "Console",
  //   icon: "browser-outline",
  //   link: "/pages/console"
  // },
  {
    title: "Templates",
    expanded: false,
    children: [
      {
        title: "Step-Templates",
        icon: "file-text-outline",
        link: "/pages/step-templates"
      },
      {
        title: "Workflow-Templates",
        icon: "share-outline",
        link: "/pages/workflow-templates"
      },
      {
        title: "Execution-Templates",
        icon: "book-outline",
        link: "/pages/execution-templates"
      },
      // {
      //   title: "Workflow-Designer",
      //   icon: "edit-2-outline",
      //   link: "/pages/workflow-designer"
      // }
    ]
  },
  {
    title: "Admin",
    expanded: false,
    children: [
      {
        title: "Global-Values",
        icon: "bookmark-outline",
        link: "/pages/global-values"
      },
      {
        title: "Bots",
        icon: "people-outline",
        link: "/pages/bots"
      }
    ]
  }
];
