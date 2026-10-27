import { useState } from "react";
import { usersApi, type GroupUser } from "@/api.ts";
import {
  AdminModal,
  AdminTable,
  AdminTableActions,
  AdminTableIconCell,
  AdminTableRow,
} from "@/components/admin/admin-shared";
import {
  CredentialsBanner,
  type IssuedCredentials,
} from "@/components/admin/credentials-banner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { TableCell } from "@/components/ui/table";
import { toast } from "sonner";
import { Plus, RefreshCw, ShieldCheck } from "lucide-react";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { useDictionaryCrud } from "@/hooks/use-dictionary-crud";
import { getUserInfo } from "@/utils/auth.ts";
import { generateSecurePassword, MIN_PASSWORD_LENGTH } from "@/utils/password";

// Everyone who can sign in to the admin area. An admin's login is also their
// display name. There is no deletion: an admin can only rename another admin
// or give them a new password.
export const AdminAdminsPage = () => {
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const [lastIssued, setLastIssued] = useState<IssuedCredentials | null>(null);
  const currentUserId = getUserInfo()?.sub;

  const issueCredentials = (issuedLogin: string, issuedPassword: string) =>
    setLastIssued({
      login: issuedLogin,
      pass: issuedPassword,
      link: `${window.location.origin}/login`,
    });

  const {
    filteredItems: filteredAdmins,
    loading,
    isSubmitting,
    searchQuery,
    setSearchQuery,
    isFormOpen,
    closeModal,
    openModal,
    editingId,
    handleSubmit,
  } = useDictionaryCrud<GroupUser>({
    fetch: async () => ({ items: (await usersApi.getAdmins()).data }),
    searchText: (a) => [a.login],
    fillForm: (a) => {
      setLogin(a?.login ?? "");
      setPassword(a ? "" : generateSecurePassword());
    },
    validate: () => {
      if (!login.trim()) {
        toast.error("Введите логин");
        return false;
      }

      if (!editingId && !password.trim()) {
        toast.error("Пароль обязателен при создании");
        return false;
      }

      if (password.trim() && password.trim().length < MIN_PASSWORD_LENGTH) {
        toast.error(
          `Пароль должен содержать не менее ${MIN_PASSWORD_LENGTH} символов`,
        );
        return false;
      }

      return true;
    },
    submit: async (id) => {
      const trimmedLogin = login.trim();
      const trimmedPassword = password.trim();

      if (id) {
        await usersApi.updateUser(id, trimmedLogin, trimmedLogin);

        if (trimmedPassword) {
          await usersApi.setPassword(id, trimmedPassword);
          issueCredentials(trimmedLogin, trimmedPassword);
        }
        toast.success("Администратор обновлен");
      } else {
        await usersApi.createAdmin(trimmedLogin, trimmedPassword);
        issueCredentials(trimmedLogin, trimmedPassword);
        toast.success("Администратор добавлен");
      }

      setLogin("");
      setPassword("");
    },
    saveErrorMessage:
      "Ошибка. Возможно, логин занят или содержит недопустимые символы.",
  });

  useAdminPageConfig({
    title: "Настройки",
    subtitle: "Администраторы",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  return (
    <>
      {loading ? (
        <div className="p-8 text-center text-muted-foreground">
          Загрузка данных...
        </div>
      ) : (
        <AdminTable
          data={filteredAdmins}
          searchQuery={searchQuery}
          emptyText="Нет администраторов"
          onSearchChange={setSearchQuery}
          searchPlaceholder="Поиск по логину..."
          topContent={
            lastIssued && (
              <CredentialsBanner
                title="Доступ администратора"
                credentials={lastIssued}
                onClose={() => setLastIssued(null)}
              />
            )
          }
          columns={[
            { header: "Администратор" },
            { header: "Действия", className: "w-32 text-right" },
          ]}
          renderRow={(a) => (
            <AdminTableRow key={a.id}>
              <TableCell className="align-top">
                <AdminTableIconCell
                  icon={<ShieldCheck size={16} />}
                  iconColorClass="bg-chart-4/15 text-chart-4"
                  title={a.login}
                  subtitle={a.id === currentUserId ? "Ваш аккаунт" : undefined}
                />
              </TableCell>
              <TableCell className="align-top text-right">
                <AdminTableActions onEdit={() => openModal(a)} />
              </TableCell>
            </AdminTableRow>
          )}
        />
      )}

      <AdminModal
        isOpen={isFormOpen}
        onClose={closeModal}
        title={
          editingId ? "Редактирование администратора" : "Новый администратор"
        }
        onSubmit={handleSubmit}
        submitText={isSubmitting ? "Сохранение..." : "Сохранить"}
      >
        <div className="space-y-2">
          <Label htmlFor="admin-login">Логин</Label>
          <Input
            id="admin-login"
            value={login}
            onChange={(e) => setLogin(e.target.value)}
            placeholder="Например: ADMIN2"
            autoFocus
          />
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between gap-3">
            <Label htmlFor="admin-password">
              {editingId ? "Новый пароль" : "Пароль"}
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
            id="admin-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder={
              editingId
                ? "Оставьте пустым, чтобы не менять пароль"
                : "Введите или сгенерируйте пароль"
            }
          />
          <p className="text-xs text-muted-foreground">
            {editingId
              ? "Если поле пустое, пароль администратора останется прежним."
              : `Не менее ${MIN_PASSWORD_LENGTH} символов. После сохранения появятся данные для входа.`}
          </p>
        </div>
      </AdminModal>
    </>
  );
};
