"use client";

import * as React from "react";

import { NavMain } from "@/components/nav-main";
import { NavUser } from "@/components/nav-user";
import { TeamSwitcher } from "@/components/team-switcher";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarRail,
} from "@/components/ui/sidebar";
import {
  GalleryVerticalEndIcon,
  AudioLinesIcon,
  TerminalIcon,
  Settings2Icon,
  BookIcon,
  ChartAreaIcon,
} from "lucide-react";
import { isCancel } from "axios";

// This is sample data.
const data = {
  user: {
    name: "Администратор",
    email: "m@example.com",
    avatar: "/avatars/shadcn.jpg",
  },
  teams: [
    {
      name: "Acme Inc",
      logo: <GalleryVerticalEndIcon />,
      plan: "Enterprise",
    },
    {
      name: "Acme Corp.",
      logo: <AudioLinesIcon />,
      plan: "Startup",
    },
    {
      name: "Evil Corp.",
      logo: <TerminalIcon />,
      plan: "Free",
    },
  ],
  navMain: [
    {
      title: "Статистика",
      icon: <ChartAreaIcon />,
      url: "#",
      isActive: true,
      items: [
        {
          title: "Дашборд",
          url: "/dashboard",
        },
        {
          title: "Конструктор анкет",
          url: "/admin/create-form",
        },
      ],
    },
    {
      title: "Справочники",
      url: "#",
      icon: <BookIcon />,
      isActive: true,
      items: [
        {
          title: "Кафедры",
          url: "/admin/departments",
        },
        {
          title: "Преподаватели",
          url: "/admin/teachers",
        },
        {
          title: "Дисциплины",
          url: "/admin/disciplines",
        },
        {
          title: "Специальности",
          url: "/admin/specialties",
        },
        {
          title: "Специализации",
          url: "/admin/specializations",
        },
      ],
    },
    {
      title: "Настройки",
      url: "#",
      icon: <Settings2Icon />,
      items: [
        {
          title: "Общие",
          url: "admin/settings",
        },
        {
          title: "Группы",
          url: "admin/groups",
        },
        {
          title: "Преподаватели",
          url: "admin/teachers",
        },
        {
          title: "Наниматели",
          url: "admin/employers",
        },
      ],
    },
  ],
};

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <TeamSwitcher teams={data.teams} />
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={data.navMain} />
      </SidebarContent>
      <SidebarFooter>
        <NavUser user={data.user} />
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  );
}
