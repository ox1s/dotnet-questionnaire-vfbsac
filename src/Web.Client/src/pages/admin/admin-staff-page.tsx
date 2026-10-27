import { useState } from "react";
import { usersApi, type GroupUser } from "@/api.ts";
import {
  AdminModal,
  AdminTable,
  AdminTableActions,
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
import { Plus, User, RefreshCw } from "lucide-react";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { useDictionaryCrud } from "@/hooks/use-dictionary-crud";
import { generateSecurePassword, MIN_PASSWORD_LENGTH } from "@/utils/password";

// Logins for college staff who are not teachers ("Сотрудник"): they fill in
// Staff-targeted questionnaires such as the personnel satisfaction survey.
export const AdminStaffPage = () => {
  const [login, setLogin] = useState("");
  const [fullName, setFullName] = useState("");
  const [password, setPassword] = useState("");
  const [lastCreated, setLastCreated] = useState<IssuedCredentials | null>(
    null,
  );

  const {
    filteredItems: filteredStaff,
    loading,
    isSubmitting,
    searchQuery,
    setSearchQuery,
    isFormOpen,
    closeModal,
    openModal,
    editingId,
    handleDelete,
    handleSubmit,
  } = useDictionaryCrud<GroupUser>({
    fetch: async () => ({ items: (await usersApi.getStaff()).data }),
    searchText: (s) => [s.login, s.displayName],
    fillForm: (s) => {
      setLogin(s?.login ?? "");
      setFullName(s?.displayName ?? "");
      setPassword(s ? "" : generateSecurePassword());
    },
    validate: () => {
      if (!login.trim() || !fullName.trim()) {
        toast.error("Заполните логин и ФИО");
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
      if (id) {
        await usersApi.updateUser(id, login.trim(), fullName.trim());

        if (password.trim()) {
          await usersApi.setPassword(id, password.trim());
          setLastCreated({
            login: login.trim(),
            pass: password.trim(),
            link: `${window.location.origin}/login`,
          });
        }
        toast.success("Сотрудник обновлен");
      } else {
        await usersApi.createStaff(
          login.trim(),
          fullName.trim(),
          password.trim(),
        );
        setLastCreated({
          login: login.trim(),
          pass: password.trim(),
          link: `${window.location.origin}/login`,
        });
        toast.success("Сотрудник создан");
      }

      setLogin("");
      setFullName("");
      setPassword("");
    },
    remove: usersApi.deleteUser,
    confirmDelete: (s) =>
      `Вы уверены, что хотите удалить сотрудника "${s.displayName}"?`,
    deleteSuccessMessage: "Сотрудник удален",
    deleteErrorMessage: "Ошибка при удалении",
    saveErrorMessage:
      "Ошибка. Возможно, логин занят или содержит недопустимые символы.",
  });

  useAdminPageConfig({
    title: "Настройки",
    subtitle: "Сотрудники",
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
          data={filteredStaff}
          searchQuery={searchQuery}
          emptyText="Нет сотрудников"
          onSearchChange={setSearchQuery}
          searchPlaceholder="Поиск сотрудника..."
          topContent={
            lastCreated && (
              <CredentialsBanner
                title="Доступ сотрудника"
                credentials={lastCreated}
                onClose={() => setLastCreated(null)}
              />
            )
          }
          columns={[
            { header: "Сотрудник" },
            { header: "Действия", className: "w-32 text-right" },
          ]}
          renderRow={(s) => (
            <AdminTableRow key={s.id}>
              <TableCell className="py-4">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-chart-3/15 text-chart-3">
                    <User size={16} />
                  </div>
                  <div>
                    <div className="text-sm font-bold text-foreground">
                      {s.displayName}
                    </div>
                    <div className="text-xs text-muted-foreground font-mono">
                      {s.login}
                    </div>
                  </div>
                </div>
              </TableCell>
              <TableCell className="py-4 text-right">
                <AdminTableActions
                  onEdit={() => openModal(s)}
                  onDelete={() => handleDelete(s)}
                />
              </TableCell>
            </AdminTableRow>
          )}
        />
      )}

      <AdminModal
        isOpen={isFormOpen}
        onClose={closeModal}
        title={editingId ? "Редактирование сотрудника" : "Новый сотрудник"}
        onSubmit={handleSubmit}
        submitText={isSubmitting ? "Сохранение..." : "Сохранить"}
      >
        <div className="space-y-2">
          <Label htmlFor="staff-login">Логин</Label>
          <Input
            id="staff-login"
            value={login}
            onChange={(e) => setLogin(e.target.value)}
            placeholder="Например: PETROV"
            autoFocus
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="staff-full-name">ФИО</Label>
          <Input
            id="staff-full-name"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="Например: Петров Пётр Петрович"
          />
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between gap-3">
            <Label htmlFor="staff-password">
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
            id="staff-password"
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
              ? "Если поле пустое, пароль сотрудника останется прежним."
              : "Не менее 12 символов. После сохранения появятся данные для входа."}
          </p>
        </div>
      </AdminModal>
    </>
  );
};
