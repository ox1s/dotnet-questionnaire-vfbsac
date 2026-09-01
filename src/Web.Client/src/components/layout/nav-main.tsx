import * as React from "react";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Link, useLocation } from "react-router-dom";
import {
  SidebarGroup,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
} from "@/components/ui/sidebar";
import { ChevronRightIcon } from "lucide-react";

type NavItem = {
  title: string;
  url: string;
  icon?: React.ReactNode;
  isActive?: boolean;
  items?: {
    title: string;
    url: string;
  }[];
};

// Group state lives in session storage, not only in component state, so the
// sidebar comes back the way the user left it after a reload or a deep link.
// <AppShell/> keeps the sidebar mounted while navigating, so within one page
// load this is a safety net rather than the mechanism — but a cheap one.
const STORAGE_KEY = "sidebar:nav-groups";

type GroupState = Record<string, boolean>;

const readGroupState = (): GroupState => {
  try {
    const raw = window.sessionStorage.getItem(STORAGE_KEY);
    if (!raw) return {};

    const parsed: unknown = JSON.parse(raw);
    if (typeof parsed !== "object" || parsed === null) return {};

    return Object.fromEntries(
      Object.entries(parsed as Record<string, unknown>).filter(
        ([, open]) => typeof open === "boolean",
      ),
    ) as GroupState;
  } catch {
    return {};
  }
};

const writeGroupState = (groups: GroupState): GroupState => {
  try {
    window.sessionStorage.setItem(STORAGE_KEY, JSON.stringify(groups));
  } catch {
    // Session storage can be unavailable (private mode, quota). Remembering
    // which groups are open is not worth breaking navigation over.
  }
  return groups;
};

export function NavMain({ items }: { items: NavItem[] }) {
  const location = useLocation();
  const [openGroups, setOpenGroups] =
    React.useState<GroupState>(readGroupState);

  // Open the group that owns the route we just landed on, unless it is already
  // open. A group the user collapsed by hand stays collapsed until they
  // navigate into it again.
  React.useEffect(() => {
    const activeGroup = items.find((item) =>
      item.items?.some((subItem) => subItem.url === location.pathname),
    );
    if (!activeGroup) return;

    setOpenGroups((current) =>
      current[activeGroup.title]
        ? current
        : writeGroupState({ ...current, [activeGroup.title]: true }),
    );
  }, [items, location.pathname]);

  const setGroupOpen = (title: string, open: boolean) => {
    setOpenGroups((current) => writeGroupState({ ...current, [title]: open }));
  };

  return (
    <SidebarGroup>
      <SidebarMenu>
        {items.map((item) => {
          const hasActiveChild = item.items?.some(
            (subItem) => subItem.url === location.pathname,
          );
          const isOpen =
            openGroups[item.title] ??
            (hasActiveChild || item.isActive || false);

          return (
            <Collapsible
              key={item.title}
              asChild
              open={isOpen}
              onOpenChange={(open) => setGroupOpen(item.title, open)}
              className="group/collapsible"
            >
              <SidebarMenuItem>
                <CollapsibleTrigger asChild>
                  <SidebarMenuButton tooltip={item.title}>
                    {item.icon}
                    <span>{item.title}</span>
                    <ChevronRightIcon className="ml-auto transition-transform duration-200 group-data-[state=open]/collapsible:rotate-90" />
                  </SidebarMenuButton>
                </CollapsibleTrigger>
                <CollapsibleContent>
                  <SidebarMenuSub>
                    {item.items?.map((subItem) => (
                      <SidebarMenuSubItem key={subItem.title}>
                        <SidebarMenuSubButton
                          asChild
                          isActive={subItem.url === location.pathname}
                        >
                          <Link to={subItem.url}>
                            <span>{subItem.title}</span>
                          </Link>
                        </SidebarMenuSubButton>
                      </SidebarMenuSubItem>
                    ))}
                  </SidebarMenuSub>
                </CollapsibleContent>
              </SidebarMenuItem>
            </Collapsible>
          );
        })}
      </SidebarMenu>
    </SidebarGroup>
  );
}
