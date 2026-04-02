"use client";

import * as React from "react";

import { NavMain } from "@/components/layout/nav-main";
import { NavUser } from "@/components/layout/nav-user";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from "@/components/ui/sidebar";
import { Settings2Icon, BookIcon, ChartAreaIcon } from "lucide-react";

const data = {
  user: {
    name: "Администратор",
    email: "m@example.com",
    avatar: "/avatars/shadcn.jpg",
  },
  navMain: [
    {
      title: "Дашборд",
      icon: <ChartAreaIcon />,
      url: "#",
      isActive: true,
      items: [
        {
          title: "Анкеты",
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
          url: "/admin/specialities",
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
          url: "/admin/settings",
        },
        {
          title: "Группы",
          url: "/admin/groups",
        },
        {
          title: "Преподаватели",
          url: "/admin/teachers",
        },
        {
          title: "Наниматели",
          url: "/admin/employers",
        },
      ],
    },
  ],
};

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              asChild
              className="data-[slot=sidebar-menu-button]:p-1.5!"
            >
              <a href="#">
                <img src="/logo.png" alt="Logo" className="size-5!" />
                <span className="text-base font-semibold">ВФБГАС</span>
              </a>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
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
