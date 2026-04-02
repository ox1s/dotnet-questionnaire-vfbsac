import { SidebarMenu, SidebarMenuItem } from "@/components/ui/sidebar";
import { LogOutIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";

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
        <div className="flex items-center gap-2">
          <Badge>{user.name}</Badge>
          <LogOutIcon className="size-4 text-muted-foreground" />
        </div>
      </SidebarMenuItem>
    </SidebarMenu>
  );
}
