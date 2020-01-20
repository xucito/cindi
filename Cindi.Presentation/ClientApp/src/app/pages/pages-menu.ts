import { NbMenuItem } from "@nebular/theme";

export const MENU_ITEMS: NbMenuItem[] = [
  {
    title: "Dashboard",
    icon: "home-outline",
    link: "/pages/dashboard",
    home: true
  },
  {
    title: "ADMIN",
    group: true
  },
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
    title: "Global-Values",
    icon: "bookmark-outline",
    link: "/pages/global-values"
  },
  {
    title: "Bots",
    icon: "people-outline",
    link: "/pages/bots"
  },
  {
    title: "Workflow-Designer",
    icon: "edit-2-outline",
    link: "/pages/workflow-designer"
  },
  {
    title: "Monitoring",
    icon: "heart-outline",
    link: "/pages/monitoring"
  }/*,
  {
    title: "FEATURES",
    group: true
  }
  ,
  {
    title: "Auth",
    icon: "lock-outline",
    children: [
      {
        title: "Login",
        link: "/auth/login"
      },
      {
        title: "Register",
        link: "/auth/register"
      },
      {
        title: "Request Password",
        link: "/auth/request-password"
      },
      {
        title: "Reset Password",
        link: "/auth/reset-password"
      }
    ]
  }*/
];
