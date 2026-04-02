import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

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
    <div className="flex min-h-svh w-full items-center justify-center p-6 md:p-10">
      <div className="w-full max-w-sm">
        <div className="flex flex-col gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Войдите в систему</CardTitle>
              <CardDescription>
                Введите свои учетные данные для доступа к панели управления
                анкетами. Если у вас нет учетной записи, пожалуйста, свяжитесь с
                администратором.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleLogin}>
                <FieldGroup>
                  <Field>
                    <FieldLabel>Логин (Группа)</FieldLabel>
                    <Input
                      type="text"
                      value={login}
                      onChange={(e) => setLogin(e.target.value)}
                      placeholder="Например: ПО111"
                    />
                  </Field>
                  <Field>
                    <div className="flex items-center">
                      <FieldLabel>Пароль</FieldLabel>
                      <a
                        href="#"
                        className="ml-auto inline-block text-sm underline-offset-4 hover:underline"
                      >
                        Забыли пароль?
                      </a>
                    </div>
                    <Input
                      type="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                    />
                  </Field>
                  <Field>
                    {error && (
                      <p className="text-destructive text-sm font-medium">
                        {error}
                      </p>
                    )}
                    <Button type="submit">Войти</Button>
                    <FieldDescription className="text-center">
                      Нет учетной записи?{" "}
                      <a href="#">Свяжитесь с администратором</a>
                    </FieldDescription>
                  </Field>
                </FieldGroup>
              </form>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};
