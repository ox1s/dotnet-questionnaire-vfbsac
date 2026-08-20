import React from "react";
import { Outlet } from "react-router-dom";
import { Edit2, Trash2, RotateCcw, SearchIcon, Trash2Icon } from "lucide-react";
import { AdminPageProvider } from "@/contexts/admin-page-context";
import { useAdminPage } from "@/hooks/use-admin-page";
import {
  SidebarInset,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Separator } from "@/components/ui/separator";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogMedia,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { AppSidebar } from "@/components/layout/app-sidebar";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Empty, EmptyHeader, EmptyTitle } from "@/components/ui/empty";

interface AdminTableIconCellProps {
  icon?: React.ReactNode;
  textIcon?: string;
  iconColorClass?: string;
  title: string;
  isDeleted?: boolean;
}
interface AdminTableProps<T> {
  searchQuery?: string;
  onSearchChange?: (val: string) => void;
  searchPlaceholder?: string;
  columns: { header: string; className?: string }[];
  data: T[];
  renderRow: (item: T) => React.ReactNode;
  emptyText?: string;
  topContent?: React.ReactNode;
}
interface AdminTableActionsProps {
  isDeleted?: boolean;
  onEdit?: () => void;
  onDelete?: () => void;
  onRestore?: () => void;
  deleteTitle?: string;
  deleteDescription?: string;
}
interface AdminModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  onSubmit: (e: React.FormEvent) => void;
  children: React.ReactNode;
  submitText?: string;
}
type AdminLayoutProps = {
  title?: string;
  subtitle?: string;
  actions?: React.ReactNode;
  children?: React.ReactNode;
};

export const AdminLayout = ({
  title,
  subtitle,
  actions,
  children,
}: AdminLayoutProps) => {
  return (
    <AdminPageProvider>
      <AdminLayoutContent title={title} subtitle={subtitle} actions={actions}>
        {children}
      </AdminLayoutContent>
    </AdminPageProvider>
  );
};
const AdminLayoutContent = ({
  title,
  subtitle,
  actions,
  children,
}: AdminLayoutProps) => {
  const { config } = useAdminPage();
  const resolvedTitle = title ?? config.title;
  const resolvedSubtitle = subtitle ?? config.subtitle;
  const resolvedActions = actions ?? config.actions;

  return (
    <SidebarProvider
      style={
        {
          "--sidebar-width": "calc(var(--spacing) * 72)",
          "--header-height": "calc(var(--spacing) * 12)",
        } as React.CSSProperties
      }
    >
      <AppSidebar variant="inset" />

      <SidebarInset>
        <header className="flex h-16 shrink-0 items-center justify-between gap-2 border-b bg-background px-4">
          <div className="flex items-center gap-2">
            <SidebarTrigger className="-ml-1" />
            <Separator orientation="vertical" />
            <Breadcrumb>
              <BreadcrumbList>
                <BreadcrumbItem className="hidden md:block">
                  <BreadcrumbPage className="font-medium text-muted-foreground">
                    {resolvedTitle}
                  </BreadcrumbPage>
                </BreadcrumbItem>
                <BreadcrumbSeparator className="hidden md:block" />
                <BreadcrumbItem>
                  <BreadcrumbPage className="font-bold text-foreground">
                    {resolvedSubtitle}
                  </BreadcrumbPage>
                </BreadcrumbItem>
              </BreadcrumbList>
            </Breadcrumb>
          </div>
          {resolvedActions ? <div>{resolvedActions}</div> : null}
        </header>

        <main className="p-4 md:p-6 max-w-7xl mx-auto w-full">
          {children ?? <Outlet />}
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
};

export const AdminTableTextBadge = ({ text }: { text: string }) => (
  <span className="inline-block px-2 py-1 text-xs font-medium bg-muted text-muted-foreground border">
    {text}
  </span>
);

export const AdminTableRow = ({
  isDeleted,
  children,
}: {
  isDeleted?: boolean;
  children: React.ReactNode;
}) => (
  <TableRow
    className={`group ${isDeleted ? "bg-muted/50 text-muted-foreground" : "hover:bg-muted/30"}`}
  >
    {children}
  </TableRow>
);
export const AdminTableIconCell = ({
  icon,
  textIcon,
  iconColorClass = "bg-primary/15 text-primary",
  title,
  isDeleted,
}: AdminTableIconCellProps) => (
  <div className="flex items-start gap-3">
    <div
      className={`flex h-8 w-8 items-center justify-center shrink-0 mt-0.5 ${iconColorClass}`}
    >
      {icon ? icon : <span className="text-xs font-bold">{textIcon}</span>}
    </div>
    <span
      className={`text-sm font-bold line-clamp-3 pt-1 ${isDeleted ? "text-muted-foreground" : "text-foreground"}`}
    >
      {title}
    </span>
    {isDeleted && (
      <Badge
        variant="secondary"
        className="mt-0.5 text-[10px] uppercase tracking-wide"
      >
        Удалено
      </Badge>
    )}
  </div>
);

export function AdminTable<T>({
  searchQuery,
  onSearchChange,
  searchPlaceholder = "Поиск...",
  columns,
  data,
  renderRow,
  emptyText = "Ничего не найдено",
  topContent,
}: AdminTableProps<T>) {
  return (
    <div className="space-y-6">
      {topContent}
      {onSearchChange && (
        <div>
          <div>
            <InputGroup>
              <InputGroupInput
                placeholder={searchPlaceholder}
                value={searchQuery || ""}
                onChange={(e) => onSearchChange(e.target.value)}
              />
              <InputGroupAddon>
                <SearchIcon />
              </InputGroupAddon>
            </InputGroup>
          </div>
        </div>
      )}
      <div>
        <Table>
          <TableHeader className="bg-muted/50">
            <TableRow>
              {columns.map((col, i) => (
                <TableHead
                  key={i}
                  className={`text-xs font-bold text-muted-foreground uppercase ${col.className || ""}`}
                >
                  {col.header}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.length > 0 ? (
              data.map((item) => renderRow(item))
            ) : (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 p-0">
                  <Empty>
                    <EmptyHeader>
                      <EmptyTitle>{emptyText}</EmptyTitle>
                    </EmptyHeader>
                  </Empty>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
export const AdminTableActions = ({
  isDeleted,
  onEdit,
  onDelete,
  onRestore,
  deleteTitle = "Удалить?",
  deleteDescription = "Вы уверены, что хотите удалить эту запись?",
}: AdminTableActionsProps) => {
  return (
    <div className="flex flex-col sm:flex-row items-end justify-end gap-1 opacity-100 lg:opacity-0 lg:group-hover:opacity-100 transition-opacity">
      {isDeleted ? (
        <Button
          variant="ghost"
          size="icon"
          onClick={onRestore}
          className="text-slate-400 hover:text-emerald-600 hover:bg-emerald-50"
          title="Восстановить"
        >
          <RotateCcw size={16} />
        </Button>
      ) : (
        <>
          {onEdit && (
            <Button
              variant="ghost"
              size="icon"
              onClick={onEdit}
              className="text-slate-400 hover:text-primary hover:bg-primary/10"
              title="Редактировать"
            >
              <Edit2 size={16} />
            </Button>
          )}

          {onDelete && (
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button variant="destructive" size="icon" title="Удалить">
                  <Trash2 size={16} />
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent size="sm">
                <AlertDialogHeader>
                  <AlertDialogMedia className="bg-destructive/10 text-destructive dark:bg-destructive/20 dark:text-destructive">
                    <Trash2Icon />
                  </AlertDialogMedia>
                  <AlertDialogTitle>{deleteTitle}</AlertDialogTitle>
                  <AlertDialogDescription>
                    {deleteDescription}
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter className="sm:justify-center gap-2 mt-4">
                  <AlertDialogCancel className="mt-0">Отмена</AlertDialogCancel>
                  <AlertDialogAction variant="destructive" onClick={onDelete}>
                    Удалить
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          )}
        </>
      )}
    </div>
  );
};

export const AdminModal = ({
  isOpen,
  onClose,
  title,
  onSubmit,
  children,
  submitText = "Сохранить",
}: AdminModalProps) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div
        className="absolute inset-0 bg-black/10 supports-backdrop-filter:backdrop-blur-xs transition-all"
        onClick={onClose}
      ></div>

      <Card className="relative w-full max-w-md shadow-lg animate-in fade-in zoom-in-95 duration-200">
        <form onSubmit={onSubmit}>
          <CardHeader>
            <CardTitle className="text-lg">{title}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4 ">{children}</CardContent>
          <CardFooter className="flex gap-3 pt-2 mt-2">
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              className="flex-1"
            >
              Отмена
            </Button>
            <Button type="submit" className="flex-1">
              {submitText}
            </Button>
          </CardFooter>
        </form>
      </Card>
    </div>
  );
};
