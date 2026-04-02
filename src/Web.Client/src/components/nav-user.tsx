import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar";
import {
  ChevronsUpDownIcon,
  SparklesIcon,
  BadgeCheckIcon,
  CreditCardIcon,
  BellIcon,
  LogOutIcon,
} from "lucide-react";
import { ModeToggle } from "./mode-toggle";
import { Link } from "react-router-dom";
import { Badge } from "./ui/badge";

export function NavUser({
  user,
}: {
  user: {
    name: string;
  };
}) {
  return (
    <SidebarMenu>
      <SidebarMenuItem>
        <Badge>{user.name}</Badge>
        <LogOutIcon />
        Log out
      </SidebarMenuItem>
    </SidebarMenu>
  );
}
