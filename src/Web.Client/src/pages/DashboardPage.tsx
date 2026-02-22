import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api, { type Form } from "../api";
import { LogOut, FileText, BarChart3, User, ArrowRight } from "lucide-react";
import { isAdmin } from "../utils/auth";
import { AdminLayout } from "../layouts/AdminLayout";

export const DashboardPage = () => {
  const [forms, setForms] = useState<Form[]>([]);
  const navigate = useNavigate();
  const userIsAdmin = isAdmin();

  useEffect(() => {
    api
      .get<Form[]>("/forms")
      .then((res) => setForms(res.data))
      .catch(() => navigate("/login"));
  }, [navigate]);

  const logout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  // --- Контент для карточек ---
  const Content = () => (
    <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
      {forms.map((form) => (
        <div
          key={form.id}
          className="group flex flex-col bg-white p-6 rounded-2xl shadow-sm border border-slate-200 hover:shadow-md hover:border-slate-300 transition-all duration-200"
        >
          <div className="flex items-start justify-between mb-4">
            <div className="p-3 bg-blue-50 text-blue-600 rounded-xl group-hover:bg-blue-600 group-hover:text-white transition-colors">
              <FileText size={24} />
            </div>
            {userIsAdmin && (
              <Link
                to={`/admin/stats/${form.id}`}
                className="p-2 text-slate-400 hover:text-slate-800 hover:bg-slate-100 rounded-lg transition-colors"
                title="Смотреть статистику"
              >
                <BarChart3 size={20} />
              </Link>
            )}
          </div>

          <h3 className="text-lg font-bold text-slate-900 mb-2 line-clamp-2 leading-tight">
            {form.title}
          </h3>

          <div className="mt-auto pt-4 border-t border-slate-100">
            {form.requiredFilters && form.requiredFilters.length > 0 ? (
              <p className="text-xs text-slate-500 font-medium uppercase tracking-wide mb-3">
                Требует: {form.requiredFilters.join(", ")}
              </p>
            ) : (
              <p className="text-xs text-slate-400 mb-3">Без фильтров</p>
            )}

            <Link
              to={`/form/${form.id}`}
              className="flex items-center justify-center w-full gap-2 py-2.5 rounded-lg bg-slate-50 text-slate-700 font-bold text-sm group-hover:bg-slate-900 group-hover:text-white transition-all"
            >
              Пройти опрос <ArrowRight size={16} />
            </Link>
          </div>
        </div>
      ))}

      {forms.length === 0 && (
        <div className="col-span-full p-12 text-center border-2 border-dashed border-slate-200 rounded-2xl">
          <p className="text-slate-400 font-medium">Нет доступных анкет</p>
        </div>
      )}
    </div>
  );

  // --- Рендер для АДМИНА (с сайдбаром) ---
  if (userIsAdmin) {
    return (
      <AdminLayout
        title="Дашборд"
        subtitle="Обзор всех активных опросов и анкет."
      >
        <Content />
      </AdminLayout>
    );
  }

  // --- Рендер для СТУДЕНТА (без сайдбара) ---
  return (
    <div className="min-h-screen bg-slate-50 font-display text-slate-900">
      <nav className="bg-white border-b border-slate-200 px-6 py-4">
        <div className="max-w-5xl mx-auto flex justify-between items-center">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center text-white">
              <FileText size={18} />
            </div>
            <h1 className="text-lg font-bold text-slate-900">
              Опросы Студентов
            </h1>
          </div>
          <div className="flex items-center gap-4">
            <div className="hidden sm:flex items-center gap-2 px-3 py-1.5 bg-slate-100 rounded-full text-xs font-bold text-slate-600">
              <User size={14} /> Студент
            </div>
            <div className="flex items-center gap-4">
              {/* Кнопка Мои анкеты */}
              <button
                onClick={() => navigate("/my-submissions")} // <-- Исправлено здесь
                className="text-slate-500 hover:text-primary font-medium text-sm flex items-center gap-2"
              >
                <FileText size={18} />
                <span className="hidden sm:inline">Мои анкеты</span>
              </button>

              {/* Кнопка Выйти */}
              <button
                onClick={logout}
                className="text-slate-500 hover:text-red-600 font-medium text-sm flex items-center gap-2"
              >
                <LogOut size={18} />
                <span className="hidden sm:inline">Выйти</span>
              </button>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-5xl mx-auto px-6 py-10">
        <div className="mb-8">
          <h2 className="text-2xl font-bold text-slate-900">
            Доступные анкеты
          </h2>
          <p className="text-slate-500">
            Выберите анкету из списка ниже, чтобы начать.
          </p>
        </div>
        <Content />
      </main>
    </div>
  );
};
