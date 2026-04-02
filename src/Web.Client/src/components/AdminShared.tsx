import React from "react";
import { Search, Edit2, Trash2, RotateCcw } from "lucide-react";
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
import { AppSidebar } from "@/components/app-sidebar";

interface AdminLayoutProps {
  title: string;
  subtitle?: string;
  actions?: React.ReactNode;
  children: React.ReactNode;
}

export const AdminLayout = ({
  title,
  subtitle,
  actions,
  children,
}: AdminLayoutProps) => {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-16 shrink-0 items-center justify-between gap-2 px-4 border-b border-slate-100 bg-white">
          <div className="flex items-center gap-2">
            <SidebarTrigger className="-ml-1" />
            <Separator orientation="vertical" className="mr-2 h-4" />
            <Breadcrumb>
              <BreadcrumbList>
                <BreadcrumbItem className="hidden md:block">
                  <BreadcrumbPage className="font-medium text-slate-500">
                    Справочники
                  </BreadcrumbPage>
                </BreadcrumbItem>
                <BreadcrumbSeparator className="hidden md:block" />
                <BreadcrumbItem>
                  <BreadcrumbPage className="font-bold text-slate-900">
                    {title}
                  </BreadcrumbPage>
                </BreadcrumbItem>
              </BreadcrumbList>
            </Breadcrumb>
          </div>
          {actions && <div>{actions}</div>}
        </header>

        <main className="p-4 md:p-6 max-w-7xl mx-auto w-full">
          {subtitle && (
            <p className="text-sm text-slate-500 mb-6">{subtitle}</p>
          )}
          {children}
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
};

interface AdminTableProps<T> {
  searchQuery?: string;
  onSearchChange?: (val: string) => void;
  searchPlaceholder?: string;
  columns: { header: string; className?: string }[];
  data: T[];
  keyExtractor: (item: T) => string;
  renderRow: (item: T) => React.ReactNode;
  emptyText?: string;
  topContent?: React.ReactNode;
}

export function AdminTable<T>({
  searchQuery,
  onSearchChange,
  searchPlaceholder = "Поиск...",
  columns,
  data,
  keyExtractor,
  renderRow,
  emptyText = "Ничего не найдено",
  topContent,
}: AdminTableProps<T>) {
  return (
    <div className="space-y-6">
      {topContent}

      {onSearchChange && (
        <div className="bg-white p-4 rounded-xl shadow-sm border border-slate-200">
          <div className="relative w-full md:max-w-md">
            <Search
              className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              size={18}
            />
            <input
              className="w-full pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:ring-2 focus:ring-primary/20 outline-none"
              placeholder={searchPlaceholder}
              value={searchQuery || ""}
              onChange={(e) => onSearchChange(e.target.value)}
            />
          </div>
        </div>
      )}

      <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50/50 border-b border-slate-200">
              {columns.map((col, i) => (
                <th
                  key={i}
                  className={`py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase ${col.className || ""}`}
                >
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {data.length > 0 ? (
              data.map((item) => (
                <React.Fragment key={keyExtractor(item)}>
                  {renderRow(item)}
                </React.Fragment>
              ))
            ) : (
              <tr>
                <td
                  colSpan={columns.length}
                  className="p-8 text-center text-slate-400 text-sm"
                >
                  {emptyText}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

interface AdminTableActionsProps {
  isDeleted?: boolean;
  onEdit?: () => void;
  onDelete?: () => void;
  onRestore?: () => void;
}

export const AdminTableActions = ({
  isDeleted,
  onEdit,
  onDelete,
  onRestore,
}: AdminTableActionsProps) => {
  return (
    <div className="flex flex-col sm:flex-row items-end justify-end gap-1 opacity-100 lg:opacity-0 lg:group-hover:opacity-100 transition-opacity">
      {isDeleted ? (
        <button
          onClick={onRestore}
          className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 transition-colors"
          title="Восстановить"
        >
          <RotateCcw size={16} className="md:w-[18px] md:h-[18px]" />
        </button>
      ) : (
        <>
          {onEdit && (
            <button
              onClick={onEdit}
              className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-primary hover:bg-primary/10 transition-colors"
              title="Редактировать"
            >
              <Edit2 size={16} className="md:w-[18px] md:h-[18px]" />
            </button>
          )}
          {onDelete && (
            <button
              onClick={onDelete}
              className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-red-600 hover:bg-red-50 transition-colors"
              title="Удалить"
            >
              <Trash2 size={16} className="md:w-[18px] md:h-[18px]" />
            </button>
          )}
        </>
      )}
    </div>
  );
};
