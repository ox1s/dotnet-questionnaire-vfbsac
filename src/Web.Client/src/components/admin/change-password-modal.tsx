import React, { useState } from "react";
import { createPortal } from "react-dom";
import { toast } from "sonner";
import { usersApi, getApiErrorMessage } from "@/api.ts";
import { getUserInfo } from "@/utils/auth.ts";
import { AdminModal } from "@/components/admin/admin-shared";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { MIN_PASSWORD_LENGTH } from "@/utils/password";

type ChangePasswordModalProps = {
  isOpen: boolean;
  onClose: () => void;
};

// Changes the signed-in admin's own password. Opened from the sidebar footer:
// it is about the current account, not a system-wide setting. Portaled to the
// body because the sidebar is a transformed off-canvas sheet on mobile, which
// would otherwise trap the modal's fixed positioning inside it.
export const ChangePasswordModal = ({
  isOpen,
  onClose,
}: ChangePasswordModalProps) => {
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  const close = () => {
    setNewPassword("");
    setConfirmPassword("");
    onClose();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isSaving) return;
    if (newPassword.length < MIN_PASSWORD_LENGTH)
      return toast.error(
        `Пароль должен содержать минимум ${MIN_PASSWORD_LENGTH} символов`,
      );
    if (newPassword !== confirmPassword)
      return toast.error("Пароли не совпадают");

    try {
      setIsSaving(true);
      const user = getUserInfo();
      if (!user?.sub) throw new Error("Пользователь не найден");
      await usersApi.setPassword(user.sub, newPassword);
      toast.success("Ваш пароль успешно изменен!");
      close();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка при смене пароля"));
    } finally {
      setIsSaving(false);
    }
  };

  return createPortal(
    <AdminModal
      isOpen={isOpen}
      onClose={close}
      title="Смена пароля"
      onSubmit={handleSubmit}
      submitText={isSaving ? "Сохранение..." : "Сохранить"}
    >
      <div className="space-y-2">
        <Label htmlFor="new-password">Новый пароль</Label>
        <Input
          id="new-password"
          type="password"
          autoComplete="new-password"
          placeholder="••••••••"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          required
        />
      </div>
      <div className="space-y-2">
        <Label htmlFor="confirm-password">Повторите пароль</Label>
        <Input
          id="confirm-password"
          type="password"
          autoComplete="new-password"
          placeholder="••••••••"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          required
        />
      </div>
    </AdminModal>,
    document.body,
  );
};
