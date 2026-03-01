import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api";

export const LoginPage = () => {
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await api.post("/users/login", { login, password });
      localStorage.setItem("token", response.data);
      navigate("/dashboard");
    } catch (err) {
      setError("Неверный логин или пароль");
    }
  };

  return (
    <div className="page-gradient min-h-screen flex items-center justify-center px-4 py-10">
      <div className="glass-card grid w-full max-w-4xl overflow-hidden lg:grid-cols-[1.1fr_1fr]">
        <div className="hidden lg:flex flex-col justify-between bg-primary p-10 text-white">
          <div>
            <p className="text-xs uppercase tracking-[0.2em] font-semibold text-blue-100">
              ВФБАС
            </p>
            <h1 className="mt-6 text-4xl font-bold leading-tight">
              Система анкетирования для студентов
            </h1>
            <p className="mt-4 text-blue-100 text-sm leading-relaxed">
              Вход в единую панель опросов, статистики и управления учебными
              формами.
            </p>
          </div>
        </div>

        <div className="bg-white p-8 sm:p-10">
          <div className="mb-8">
            <h2 className="text-3xl font-bold text-slate-900">Добро пожаловать</h2>
            <p className="mt-2 text-sm text-slate-500">
              Введите данные группы, чтобы открыть личный кабинет.
            </p>
          </div>

          <form onSubmit={handleLogin} className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Логин (Группа)
              </label>
              <input
                type="text"
                className="input-field mt-1"
                value={login}
                onChange={(e) => setLogin(e.target.value)}
                placeholder="Например: ПО111"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Пароль
              </label>
              <input
                type="password"
                className="input-field mt-1"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <button type="submit" className="btn-primary w-full py-2.5 text-base">
              Войти
            </button>
          </form>

        </div>
      </div>
    </div>
  );
};
