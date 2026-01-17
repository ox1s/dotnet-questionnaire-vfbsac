import React from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  FileText,
  FolderOpen,
  Users,
  Settings,
  LogOut,
  School,
} from "lucide-react";

interface AdminLayoutProps {
  children: React.ReactNode;
  title: string;
  subtitle?: string;
  actions?: React.ReactNode;
}

export const AdminLayout = ({
  children,
  title,
  subtitle,
  actions,
}: AdminLayoutProps) => {
  const location = useLocation();
  const navigate = useNavigate();

  const isActive = (path: string) => location.pathname === path;

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  const NavItem = ({
    to,
    icon: Icon,
    label,
  }: {
    to: string;
    icon: any;
    label: string;
  }) => (
    <Link
      to={to}
      className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors group ${
        isActive(to)
          ? "bg-primary/10 text-primary font-bold"
          : "text-slate-600 hover:bg-slate-50 font-medium"
      }`}
    >
      <Icon
        size={20}
        className={
          isActive(to)
            ? "text-primary"
            : "text-slate-400 group-hover:text-primary"
        }
      />
      <span className="text-sm">{label}</span>
    </Link>
  );

  return (
    <div className="flex h-screen w-full bg-background-light text-slate-900 font-display overflow-hidden">
      {/* Сайдбар */}
      <aside className="hidden lg:flex flex-col w-72 h-full bg-surface-light border-r border-slate-200 transition-colors z-20">
        <div className="p-6 flex items-center gap-3 border-b border-slate-100">
          <div className="w-10 h-10 rounded-lg bg-slate-800 flex items-center justify-center text-white shrink-0 shadow-lg shadow-primary/20">
            <School size={24} />
          </div>
          <div>
            <h1 className="text-primary text-lg font-bold leading-none tracking-tight">
              Анкетирование
            </h1>
            <p className="text-secondary text-xs font-medium mt-1">
              Панель Админа
            </p>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto py-6 px-4 flex flex-col gap-1">
          <p className="px-4 text-xs font-bold text-slate-400 uppercase tracking-wider mb-2">
            Главное меню
          </p>

          <NavItem to="/dashboard" icon={LayoutDashboard} label="Дашборд" />
          <NavItem
            to="/admin/create-form"
            icon={FileText}
            label="Конструктор анкет"
          />

          {/* Блок справочников */}
          <div
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
              location.pathname.includes("/admin/departments") ||
              location.pathname.includes("/admin/teachers") ||
              location.pathname.includes("/admin/disciplines")
                ? "bg-primary/10 text-primary font-bold"
                : "text-slate-600 hover:bg-slate-50 font-medium"
            }`}
          >
            <FolderOpen
              size={20}
              className={
                location.pathname.includes("/admin/")
                  ? "text-primary"
                  : "text-slate-400"
              }
            />
            <span className="text-sm">Справочники</span>
          </div>

          {/* Подменю */}
          <div className="ml-12 flex flex-col gap-1 border-l-2 border-slate-100 pl-3 my-1">
            <Link
              to="/admin/departments"
              className={`text-sm py-1 ${isActive("/admin/departments") ? "text-primary font-bold" : "text-slate-500 hover:text-slate-800"}`}
            >
              Кафедры
            </Link>
            <Link
              to="/admin/teachers"
              className={`text-sm py-1 ${isActive("/admin/teachers") ? "text-primary font-bold" : "text-slate-500 hover:text-slate-800"}`}
            >
              Преподаватели
            </Link>
            <Link
              to="/admin/disciplines"
              className={`text-sm py-1 ${isActive("/admin/disciplines") ? "text-primary font-bold" : "text-slate-500 hover:text-slate-800"}`}
            >
              Дисциплины
            </Link>
          </div>

          <NavItem to="/admin/groups" icon={Users} label="Группы и доступы" />

          <div className="my-4 border-t border-slate-100"></div>
          <NavItem to="#" icon={Settings} label="Настройки" />
        </div>

        <div className="p-4 border-t border-slate-200">
          <button
            onClick={handleLogout}
            className="flex w-full items-center gap-3 p-2 rounded-lg hover:bg-red-50 text-slate-600 hover:text-red-600 transition-colors"
          >
            <LogOut size={20} />
            <span className="text-sm font-bold">Выйти</span>
          </button>
        </div>
      </aside>

      {/* Основной контент */}
      <main className="flex-1 flex flex-col h-full overflow-hidden relative">
        {/* Шапка страницы */}
        <header className="flex-none bg-surface-light border-b border-slate-200 px-8 pt-8 pb-4 z-10">
          <div className="max-w-7xl mx-auto w-full">
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 mb-2">
              <div>
                <h2 className="text-3xl font-bold text-slate-900 tracking-tight">
                  {title}
                </h2>
                {subtitle && <p className="text-secondary mt-1">{subtitle}</p>}
              </div>
              {actions && <div className="flex gap-3">{actions}</div>}
            </div>
          </div>
        </header>

        {/* Тело страницы */}
        <div className="flex-1 overflow-y-auto bg-background-light p-8">
          <div className="max-w-7xl mx-auto w-full">{children}</div>
        </div>
      </main>
    </div>
  );
};
