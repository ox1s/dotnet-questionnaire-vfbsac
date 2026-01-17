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
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <div className="bg-white p-8 rounded-lg shadow-md w-96">
        <h1 className="text-2xl font-bold mb-6 text-center text-blue-900">
          Анкетирование
        </h1>
        <form onSubmit={handleLogin} className="space-y-4">
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
          <button type="submit" className="btn-primary w-full">
            Войти
          </button>
        </form>
        <div className="mt-4 text-xs text-gray-400 text-center">
          Для теста: Логин <b>ПО111</b>, Пароль <b>12345678</b>
        </div>
      </div>
    </div>
  );
};
