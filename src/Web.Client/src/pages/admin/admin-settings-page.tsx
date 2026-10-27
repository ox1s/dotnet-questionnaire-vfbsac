import { useState } from "react";
import {
  ShieldAlert,
  AlertTriangle,
  LockKeyholeIcon,
  LockKeyholeOpenIcon,
  CheckIcon,
} from "lucide-react";
import { settingsApi, getApiErrorMessage } from "@/api.ts";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { toast } from "sonner";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";

export const AdminSettingsPage = () => {
  const [isClosing, setIsClosing] = useState(false);
  const [isOpening, setIsOpening] = useState(false);

  useAdminPageConfig({
    title: "Настройки",
    subtitle: "Семестр",
  });

  const handleCloseSemester = async () => {
    if (
      window.confirm("ВНИМАНИЕ!\nВы уверены, что хотите завершить семестр?")
    ) {
      try {
        setIsClosing(true);
        await settingsApi.closeSemester();
        toast.success("Семестр успешно закрыт. Все анкеты деактивированы.");
      } catch (e) {
        toast.error(getApiErrorMessage(e, "Не удалось закрыть семестр."));
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
        toast.error(getApiErrorMessage(e, "Не удалось открыть новый семестр."));
      } finally {
        setIsOpening(false);
      }
    }
  };

  return (
    <>
      <div className="max-w-2xl">
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
                  Учащиеся потеряют доступ к анкетам. Используйте эту кнопку
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
                  Учащиеся получат доступ к анкетам. Используйте эту кнопку
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
