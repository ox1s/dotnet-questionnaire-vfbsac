import React, { useState } from "react";
import { AdminLayout } from "../layouts/AdminLayout";
import { ShieldAlert, KeyRound, PowerOff, AlertTriangle } from "lucide-react";
import { usersApi } from "../api";
import { getUserInfo } from "../utils/auth";
import toast from "react-hot-toast";

export const AdminSettingsPage = () => {
  // Состояния для смены пароля
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  // Обработчик смены пароля
  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();

    if (newPassword.length < 8) {
      toast.error("Пароль должен содержать минимум 8 символов");
      return;
    }
    if (newPassword !== confirmPassword) {
      toast.error("Пароли не совпадают");
      return;
    }

    try {
      setIsSaving(true);
      const user = getUserInfo();
      if (!user?.sub) throw new Error("Пользователь не найден");

      // Вызываем API для изменения пароля (ID берется из токена текущего админа)
      await usersApi.setPassword(user.sub, newPassword);

      toast.success("Ваш пароль успешно изменен!");
      setNewPassword("");
      setConfirmPassword("");
    } catch (e) {
      console.error(e);
      toast.error("Ошибка при смене пароля");
    } finally {
      setIsSaving(false);
    }
  };

  // Обработчик закрытия семестра
  const handleCloseSemester = () => {
    if (
      window.confirm(
        "ВНИМАНИЕ!\nВы уверены, что хотите завершить семестр? Все текущие анкеты будут скрыты от студентов, но статистика сохранится.",
      )
    ) {
      // Здесь в будущем будет вызов API (например: await api.post('/settings/close-semester'))
      toast.success("Семестр успешно закрыт. Анкеты скрыты.");
    }
  };

  return (
    <AdminLayout
      title="Настройки системы"
      subtitle="Управление безопасностью и глобальными параметрами."
    >
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 items-start">
        <div className="bg-surface-light p-6 md:p-8 rounded-2xl shadow-sm border border-slate-200">
          <div className="flex items-center gap-3 mb-6">
            <div className="p-3 bg-primary/10 text-primary rounded-xl">
              <KeyRound size={24} />
            </div>
            <div>
              <h3 className="text-lg font-bold text-slate-900">Смена пароля</h3>
              <p className="text-sm text-secondary">
                Обновите пароль администратора
              </p>
            </div>
          </div>

          <form onSubmit={handlePasswordChange} className="space-y-5">
            <div>
              <label className="block text-sm font-bold text-slate-700 mb-2">
                Новый пароль
              </label>
              <input
                type="password"
                className="input-field p-3 bg-background-light rounded-xl font-mono text-sm"
                placeholder="••••••••"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
              />
            </div>
            <div>
              <label className="block text-sm font-bold text-slate-700 mb-2">
                Повторите пароль
              </label>
              <input
                type="password"
                className="input-field p-3 bg-background-light rounded-xl font-mono text-sm"
                placeholder="••••••••"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
            </div>

            <button
              type="submit"
              disabled={isSaving || !newPassword || !confirmPassword}
              className="w-full sm:w-auto px-6 py-3 bg-primary text-white rounded-xl font-bold text-sm hover:bg-primary-hover transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-md shadow-primary/20 mt-2"
            >
              {isSaving ? "Сохранение..." : "Сохранить новый пароль"}
            </button>
          </form>
        </div>

        <div className="bg-surface-light p-6 md:p-8 rounded-2xl shadow-sm border border-accent/20">
          <div className="flex items-center gap-3 mb-6">
            <div className="p-3 bg-accent/10 text-accent rounded-xl">
              <ShieldAlert size={24} />
            </div>
            <div>
              <h3 className="text-lg font-bold text-slate-900">
                Управление доступом
              </h3>
              <p className="text-sm text-secondary">
                Изменить доступность анкет для прохождения
              </p>
            </div>
          </div>

          <div className="p-5 bg-red-50 border border-red-100 rounded-xl flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
            <div>
              <h4 className="font-bold text-slate-900 flex items-center gap-2">
                <AlertTriangle size={18} className="text-accent" />
                Закрыть текущий семестр
              </h4>
              <p className="text-sm text-slate-600 mt-1 leading-relaxed max-w-sm">
                Студенты потеряют доступ к анкетам. Используйте эту кнопку
                только по окончании периода опросов.
              </p>
            </div>

            <button
              onClick={handleCloseSemester}
              className="w-full sm:w-auto shrink-0 px-6 py-3 bg-white border-2 border-accent text-accent rounded-xl font-bold text-sm hover:bg-accent hover:text-white transition-all shadow-sm"
            >
              <span className="flex items-center justify-center gap-2">
                <PowerOff size={18} /> Завершить
              </span>
            </button>
          </div>
        </div>
      </div>
    </AdminLayout>
  );
};
