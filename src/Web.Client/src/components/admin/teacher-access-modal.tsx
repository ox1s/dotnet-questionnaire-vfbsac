import { useState } from "react";
import { RefreshCw } from "lucide-react";
import { toast } from "sonner";
import { getApiErrorMessage, usersApi, type TeacherUser } from "@/api.ts";
import { AdminModal } from "@/components/admin/admin-shared";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { IssuedCredentials } from "@/components/admin/credentials-banner";
import { generateSecurePassword, MIN_PASSWORD_LENGTH } from "@/utils/password";

interface TeacherAccessModalProps {
  teacher: { id: string; fullName: string };
  /** The teacher's existing login, or null when they cannot sign in yet. */
  account: TeacherUser | null;
  onClose: () => void;
  onSaved: (credentials: IssuedCredentials) => void;
}

// Creates a teacher's login, or issues a new password when one already exists.
export const TeacherAccessModal = ({
  teacher,
  account,
  onClose,
  onSaved,
}: TeacherAccessModalProps) => {
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState(() => generateSecurePassword());
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!account && !login.trim()) {
      toast.error("Введите логин");
      return;
    }
    if (password.trim().length < MIN_PASSWORD_LENGTH) {
      toast.error(
        `Пароль должен содержать не менее ${MIN_PASSWORD_LENGTH} символов`,
      );
      return;
    }

    setIsSubmitting(true);
    try {
      if (account) {
        await usersApi.setPassword(account.id, password.trim());
      } else {
        await usersApi.createTeacherUser(
          teacher.id,
          login.trim(),
          password.trim(),
        );
      }
      onSaved({
        login: account ? account.login : login.trim(),
        pass: password.trim(),
        link: `${window.location.origin}/login`,
      });
    } catch (error) {
      toast.error(
        getApiErrorMessage(
          error,
          "Ошибка. Возможно, логин занят или содержит недопустимые символы.",
        ),
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AdminModal
      isOpen
      onClose={onClose}
      title={
        account ? "Новый пароль преподавателя" : "Доступ для преподавателя"
      }
      onSubmit={handleSubmit}
      submitText={isSubmitting ? "Сохранение..." : "Сохранить"}
    >
      <p className="text-sm text-muted-foreground">{teacher.fullName}</p>

      {account ? (
        <div className="space-y-2">
          <Label>Логин</Label>
          <p className="font-mono text-sm">{account.login}</p>
        </div>
      ) : (
        <div className="space-y-2">
          <Label htmlFor="teacher-login">Логин</Label>
          <Input
            id="teacher-login"
            value={login}
            onChange={(e) => setLogin(e.target.value)}
            placeholder="Например: IVANOVA"
            autoFocus
          />
        </div>
      )}

      <div className="space-y-2">
        <div className="flex items-center justify-between gap-3">
          <Label htmlFor="teacher-password">
            {account ? "Новый пароль" : "Пароль"}
          </Label>
          <Button
            type="button"
            variant="outline"
            onClick={() => setPassword(generateSecurePassword())}
            className="h-8 px-3"
          >
            <RefreshCw size={14} className="mr-2" />
            Сгенерировать
          </Button>
        </div>
        <Input
          id="teacher-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <p className="text-xs text-muted-foreground">
          Не менее 12 символов. После сохранения появятся данные для входа.
        </p>
      </div>
    </AdminModal>
  );
};
