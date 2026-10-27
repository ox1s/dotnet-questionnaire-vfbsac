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
  ChartAreaIcon,
  KeyRoundIcon,
  LogOutIcon,
  UsersIcon,
} from "lucide-react";
import { Button } from "../ui/button";
import { ModeToggle } from "../shared/mode-toggle";
import { ChangePasswordModal } from "@/components/admin/change-password-modal";
import { logout } from "@/utils/auth";

const data = {
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
        {
          title: "Наниматели",
          url: "/admin/employers",
        },
      ],
    },
    {
      // Everyone who can sign in. Reference data about the same people (e.g.
      // the teacher directory) stays under "Справочники".
      title: "Пользователи",
      url: "#",
      icon: <UsersIcon />,
      items: [
        {
          title: "Группы",
          url: "/admin/groups",
        },
        {
          title: "Преподаватели",
          url: "/admin/teacher-accounts",
        },
        {
          title: "Сотрудники",
          url: "/admin/staff",
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
      ],
    },
  ],
};

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const [isPasswordOpen, setIsPasswordOpen] = React.useState(false);

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
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        <NavMain items={data.navMain} />
      </SidebarContent>
      <SidebarFooter>
        <div className="px-4 py-3 flex justify-end items-center">
          {" "}
          <ModeToggle />
          <Button
            onClick={() => setIsPasswordOpen(true)}
            variant="outline"
            size="icon"
            className="ml-4"
            aria-label="Сменить пароль"
            title="Сменить пароль"
          >
            <KeyRoundIcon />
          </Button>
          <Button
            onClick={logout}
            variant="outline"
            size="icon"
            className="ml-4"
            aria-label="Выйти"
            title="Выйти"
          >
            <LogOutIcon />
          </Button>
        </div>
      </SidebarFooter>
      <SidebarRail />
      <ChangePasswordModal
        isOpen={isPasswordOpen}
        onClose={() => setIsPasswordOpen(false)}
      />
    </Sidebar>
  );
}
