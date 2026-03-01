import React, { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  FileText,
  FolderOpen,
  Users,
  Settings,
  LogOut,
  Menu, // Иконка бургера
  X, // Иконка крестика
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
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const isActive = (path: string) => location.pathname === path;

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  const closeMenu = () => setIsMobileMenuOpen(false);

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
      onClick={closeMenu} // Закрываем меню при клике
      className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors group ${
        isActive(to)
          ? "bg-primary/5 text-primary font-bold border-l-4 border-accent"
          : "text-slate-600 hover:bg-slate-50 font-medium border-l-4 border-transparent"
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
    <div className="flex h-screen w-full bg-background-light text-slate-900 font-display overflow-hidden relative">
      {/* Затемнение фона для мобильного меню */}
      {isMobileMenuOpen && (
        <div
          className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-40 lg:hidden"
          onClick={closeMenu}
        />
      )}

      {/* Сайдбар */}
      <aside
        className={`fixed inset-y-0 left-0 z-50 w-72 bg-surface-light border-r border-slate-200 transform transition-transform duration-300 ease-in-out flex flex-col h-full lg:relative lg:translate-x-0 ${
          isMobileMenuOpen ? "translate-x-0 shadow-2xl" : "-translate-x-full"
        }`}
      >
        <div className="p-6 flex items-center justify-between gap-3 border-b border-slate-100">
          <div className="flex items-center gap-3">
            {/* Логотип */}
            <div className="w-12 h-12 shrink-0 flex items-center justify-center">
              <img
                src="/logo.png"
                alt="Логотип"
                className="w-full h-full object-contain"
              />
            </div>
            <div>
              <h1 className="text-primary text-lg font-bold leading-none tracking-tight">
                ВФБАС
              </h1>
              <p className="text-accent text-xs font-bold mt-1">
                Анкетирование
              </p>
            </div>
          </div>

          {/* Кнопка закрытия только для мобилок */}
          <button
            className="lg:hidden p-2 text-slate-400 hover:text-slate-800 rounded-lg hover:bg-slate-100"
            onClick={closeMenu}
          >
            <X size={24} />
          </button>
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
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors border-l-4 ${
              location.pathname.includes("/admin/departments") ||
              location.pathname.includes("/admin/teachers") ||
              location.pathname.includes("/admin/disciplines")
                ? "bg-primary/5 text-primary font-bold border-accent"
                : "text-slate-600 hover:bg-slate-50 font-medium border-transparent"
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
              onClick={closeMenu}
              className={`text-sm py-1.5 ${isActive("/admin/departments") ? "text-primary font-bold" : "text-slate-500 hover:text-slate-800"}`}
            >
              Кафедры
            </Link>
            <Link
              to="/admin/teachers"
              onClick={closeMenu}
              className={`text-sm py-1.5 ${isActive("/admin/teachers") ? "text-primary font-bold" : "text-slate-500 hover:text-slate-800"}`}
            >
              Преподаватели
            </Link>
            <Link
              to="/admin/disciplines"
              onClick={closeMenu}
              className={`text-sm py-1.5 ${isActive("/admin/disciplines") ? "text-primary font-bold" : "text-slate-500 hover:text-slate-800"}`}
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
            className="flex w-full items-center gap-3 p-3 rounded-lg hover:bg-accent/10 text-slate-600 hover:text-accent transition-colors"
          >
            <LogOut size={20} />
            <span className="text-sm font-bold">Выйти</span>
          </button>
        </div>
      </aside>

      {/* Основной контент */}
      <main className="flex-1 flex flex-col h-full overflow-hidden w-full relative">
        {/* Шапка страницы */}
        <header className="flex-none bg-surface-light border-b border-slate-200 px-4 md:px-8 pt-4 md:pt-8 pb-4 z-10">
          <div className="max-w-7xl mx-auto w-full flex flex-col md:flex-row md:items-end justify-between gap-4 mb-2">
            <div className="flex items-center gap-3">
              {/* Бургер для мобилки */}
              <button
                className="lg:hidden p-2 -ml-2 text-slate-500 hover:bg-slate-100 rounded-lg"
                onClick={() => setIsMobileMenuOpen(true)}
              >
                <Menu size={28} />
              </button>
              <div>
                <h2 className="text-2xl md:text-3xl font-bold text-slate-900 tracking-tight">
                  {title}
                </h2>
                {subtitle && (
                  <p className="text-secondary mt-1 text-sm md:text-base">
                    {subtitle}
                  </p>
                )}
              </div>
            </div>
            {/* Панель кнопок действий */}{" "}
            {actions && (
              <div className="flex flex-wrap gap-3 py-2 -my-2">{actions}</div>
            )}
          </div>
        </header>

        {/* Тело страницы */}
        <div className="flex-1 overflow-y-auto bg-background-light p-4 md:p-8">
          <div className="max-w-7xl mx-auto w-full">{children}</div>
        </div>
      </main>
    </div>
  );
};
