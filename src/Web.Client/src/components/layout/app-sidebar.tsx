"use client";

import * as React from "react";
import { Link } from "react-router-dom";

import { NavMain } from "@/components/layout/nav-main";
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
import {
  Settings2Icon,
  BookIcon,
  ClipboardListIcon,
  LogOutIcon,
  UsersIcon,
} from "lucide-react";
import { Button } from "../ui/button";
import { ModeToggle } from "../shared/mode-toggle";
import { logout } from "@/utils/auth";
import { getEnvLabel } from "@/utils/env-label";
import { Badge } from "@/components/ui/badge";

// Grouped by what the admin actually maintains: the forms, the people who fill
// them in (access is managed on each person's own page, not in a separate
// users section) and the teaching structure being evaluated. Specializations
// are opened from their speciality, so they have no entry of their own.
const data = {
  navMain: [
    {
      title: "Анкеты",
      icon: <ClipboardListIcon />,
      url: "#",
      isActive: true,
      items: [
        {
          title: "Все анкеты",
          url: "/dashboard",
        },
        {
          title: "Конструктор",
          url: "/admin/create-form",
        },
      ],
    },
    {
      title: "Участники",
      url: "#",
      icon: <UsersIcon />,
      isActive: true,
      items: [
        {
          title: "Преподаватели",
          url: "/admin/teachers",
        },
        {
          title: "Сотрудники",
          url: "/admin/staff",
        },
        {
          title: "Группы",
          url: "/admin/groups",
        },
        {
          title: "Наниматели",
          url: "/admin/employers",
        },
      ],
    },
    {
      title: "Учебная структура",
      url: "#",
      icon: <BookIcon />,
      isActive: true,
      items: [
        {
          title: "Кафедры",
          url: "/admin/departments",
        },
        {
          title: "Специальности",
          url: "/admin/specialities",
        },
        {
          title: "Дисциплины",
          url: "/admin/disciplines",
        },
      ],
    },
    {
      title: "Настройки",
      url: "#",
      icon: <Settings2Icon />,
      items: [
        {
          title: "Семестр",
          url: "/admin/settings",
        },
        {
          title: "Аккаунт",
          url: "/admin/account",
        },
        {
          title: "Администраторы",
          url: "/admin/admins",
        },
      ],
    },
  ],
};

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const envLabel = getEnvLabel();

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              asChild
              className="data-[slot=sidebar-menu-button]:p-1.5!"
            >
              <Link to="/dashboard">
                <img src="/logo.png" alt="Logo" className="size-5!" />
                <span className="text-base font-semibold">ВФБГАС</span>
                {envLabel && (
                  <Badge variant="destructive" data-testid="env-label">
                    {envLabel}
                  </Badge>
                )}
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={data.navMain} />
      </SidebarContent>
      <SidebarFooter>
        <div className="flex items-center justify-end gap-4 px-4 py-3 group-data-[collapsible=icon]:flex-col group-data-[collapsible=icon]:gap-2 group-data-[collapsible=icon]:px-0">
          <ModeToggle />
          <Button
            onClick={logout}
            variant="outline"
            size="icon"
            aria-label="Выйти"
            title="Выйти"
          >
            <LogOutIcon />
          </Button>
        </div>
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  );
}
