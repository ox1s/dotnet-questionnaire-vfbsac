import React, { useEffect, useState } from "react";
import { toast } from "sonner";
import { KeyRoundIcon, UserRoundIcon } from "lucide-react";
import { usersApi, getApiErrorMessage } from "@/api.ts";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { MIN_PASSWORD_LENGTH } from "@/utils/password";

// The signed-in admin's own login and password. The session token carries
// only the user id, so a new login takes effect without signing in again.
export const AdminAccountPage = () => {
  const [userId, setUserId] = useState<string | null>(null);
  const [login, setLogin] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSavingLogin, setIsSavingLogin] = useState(false);
  const [isSavingPassword, setIsSavingPassword] = useState(false);

  useAdminPageConfig({
    title: "Настройки",
    subtitle: "Аккаунт",
  });

  useEffect(() => {
    usersApi
      .getMe()
      .then(({ data }) => {
        setUserId(data.id);
        setLogin(data.login);
      })
      .catch((e) =>
        toast.error(getApiErrorMessage(e, "Не удалось загрузить аккаунт")),
      );
  }, []);

  const handleLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!userId || isSavingLogin) return;
    const trimmedLogin = login.trim();
    if (!trimmedLogin) return toast.error("Введите логин");

    try {
      setIsSavingLogin(true);
      await usersApi.updateUser(userId, trimmedLogin, trimmedLogin);
      setLogin(trimmedLogin);
      toast.success("Логин изменен");
    } catch (e) {
      toast.error(
        getApiErrorMessage(
          e,
          "Ошибка. Возможно, логин занят или содержит недопустимые символы.",
        ),
      );
    } finally {
      setIsSavingLogin(false);
    }
  };

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!userId || isSavingPassword) return;
    if (newPassword.length < MIN_PASSWORD_LENGTH)
      return toast.error(
        `Пароль должен содержать минимум ${MIN_PASSWORD_LENGTH} символов`,
      );
    if (newPassword !== confirmPassword)
      return toast.error("Пароли не совпадают");

    try {
      setIsSavingPassword(true);
      await usersApi.setPassword(userId, newPassword);
      setNewPassword("");
      setConfirmPassword("");
      toast.success("Ваш пароль успешно изменен!");
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка при смене пароля"));
    } finally {
      setIsSavingPassword(false);
    }
  };

  return (
    <div className="max-w-2xl space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-3">
            <div className="p-2 bg-primary/15 text-primary">
              <UserRoundIcon size={20} />
            </div>
            Логин
          </CardTitle>
          <CardDescription>
            Имя, под которым вы входите в систему
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleLoginSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="account-login">Логин</Label>
              <Input
                id="account-login"
                value={login}
                onChange={(e) => setLogin(e.target.value)}
                autoComplete="username"
              />
            </div>
            <Button type="submit" disabled={!userId || isSavingLogin}>
              {isSavingLogin ? "Сохранение..." : "Сохранить логин"}
            </Button>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-3">
            <div className="p-2 bg-chart-4/15 text-chart-4">
              <KeyRoundIcon size={20} />
            </div>
            Пароль
          </CardTitle>
          <CardDescription>
            Не менее {MIN_PASSWORD_LENGTH} символов
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handlePasswordSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="account-new-password">Новый пароль</Label>
              <Input
                id="account-new-password"
                type="password"
                autoComplete="new-password"
                placeholder="••••••••"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="account-confirm-password">Повторите пароль</Label>
              <Input
                id="account-confirm-password"
                type="password"
                autoComplete="new-password"
                placeholder="••••••••"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
            </div>
            <Button type="submit" disabled={!userId || isSavingPassword}>
              {isSavingPassword ? "Сохранение..." : "Сменить пароль"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
};
