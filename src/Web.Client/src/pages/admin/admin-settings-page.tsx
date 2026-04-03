import React, { useState } from "react";
import {
  ShieldAlert,
  KeyRound,
  PowerOff,
  AlertTriangle,
  LockKeyholeIcon,
  LockKeyholeOpenIcon,
  CheckIcon,
} from "lucide-react";
import { usersApi, settingsApi } from "../../api";
import { getUserInfo } from "../../utils/auth";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";

export const AdminSettingsPage = () => {
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [isClosing, setIsClosing] = useState(false);
  const [isOpening, setIsOpening] = useState(false);

  useAdminPageConfig({
    title: "Настройки",
    subtitle: "Общее",
  });

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword.length < 8)
      return toast.error("Пароль должен содержать минимум 8 символов");
    if (newPassword !== confirmPassword)
      return toast.error("Пароли не совпадают");

    try {
      setIsSaving(true);
      const user = getUserInfo();
      if (!user?.sub) throw new Error("Пользователь не найден");
      await usersApi.setPassword(user.sub, newPassword);
      toast.success("Ваш пароль успешно изменен!");
      setNewPassword("");
      setConfirmPassword("");
    } catch (e) {
      toast.error("Ошибка при смене пароля");
    } finally {
      setIsSaving(false);
    }
  };

  const handleCloseSemester = async () => {
    if (
      window.confirm("ВНИМАНИЕ!\nВы уверены, что хотите завершить семестр?")
    ) {
      try {
        setIsClosing(true);
        await settingsApi.closeSemester();
        toast.success("Семестр успешно закрыт. Все анкеты деактивированы.");
      } catch (e) {
        toast.error("Не удалось закрыть семестр.");
      } finally {
        setIsClosing(false);
      }
    }
  };

  const handleOpenSemester = async () => {
    if (
      window.confirm(
        "Вы уверены, что хотите открыть новый семестр? Это действие может привести к потере данных о текущем семестре.",
      )
    ) {
      try {
        setIsOpening(true);
        await settingsApi.openSemester();
        toast.success("Новый семестр успешно открыт.");
      } catch (e) {
        toast.error("Не удалось открыть новый семестр.");
      } finally {
        setIsOpening(false);
      }
    }
  };

  return (
    <>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <div className="bg-primary/15 p-2 text-primary">
                <KeyRound size={20} />
              </div>
              Смена пароля
            </CardTitle>
            <CardDescription>Обновите пароль администратора</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handlePasswordChange} className="space-y-4">
              <div className="space-y-2">
                <Label>Новый пароль</Label>
                <Input
                  type="password"
                  placeholder="••••••••"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label>Повторите пароль</Label>
                <Input
                  type="password"
                  placeholder="••••••••"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required
                />
              </div>
              <Button
                type="submit"
                disabled={isSaving || !newPassword || !confirmPassword}
                className="w-full sm:w-auto mt-2"
              >
                {isSaving ? "Сохранение..." : "Сохранить новый пароль"}
              </Button>
            </form>
          </CardContent>
        </Card>

        <Card className="border-destructive/20">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <div className="p-2 bg-chart-1/20 text-chart-4">
                <ShieldAlert size={20} />
              </div>
              Управление доступом
            </CardTitle>
            <CardDescription>
              Изменить доступность анкет для прохождения
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col items-start justify-between gap-4 border border-destructive/20 bg-destructive/5 p-5 sm:flex-row sm:items-center grid-rows-2">
              <div>
                <h4 className="font-bold text-foreground flex items-center gap-2">
                  <AlertTriangle size={18} className="text-destructive" />
                  Закрыть текущий семестр
                </h4>
                <p className="text-sm text-muted-foreground mt-1 leading-relaxed max-w-sm">
                  Студенты потеряют доступ к анкетам. Используйте эту кнопку
                  только по окончании периода опросов.
                </p>
              </div>
              <Button
                variant="destructive"
                onClick={handleCloseSemester}
                disabled={isClosing}
                className="w-full sm:w-auto shrink-0"
              >
                <LockKeyholeIcon size={16} className="mr-2" />
                {isClosing ? "Завершение..." : "Завершить"}
              </Button>
            </div>
            <div className="mt-5 flex flex-col items-start justify-between gap-4 border border-chart-5/20 bg-chart-1/5 p-5 sm:flex-row sm:items-center">
              <div>
                <h4 className="font-bold text-foreground flex items-center gap-2">
                  <CheckIcon size={18} />
                  Открыть текущий семестр
                </h4>
                <p className="text-sm text-muted-foreground mt-1 leading-relaxed max-w-sm">
                  Студенты получат доступ к анкетам. Используйте эту кнопку
                  только по началу периода опросов.
                </p>
              </div>
              <Button
                variant="secondary"
                onClick={handleOpenSemester}
                disabled={isOpening}
                className="w-full sm:w-auto shrink-0"
              >
                <LockKeyholeOpenIcon size={16} className="mr-2" />
                {isOpening ? "Открытие..." : "Открыть"}
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </>
  );
};
