import React, { useEffect, useState } from "react";
import { getApiErrorMessage, usersApi, type EmployerUser } from "../../api";
import {
  AdminModal,
  AdminTable,
  AdminTableActions,
} from "@/components/admin/admin-shared";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { TableCell, TableRow } from "@/components/ui/table";
import { toast } from "sonner";
import { Plus, Building2, Key, X, RefreshCw, Copy } from "lucide-react";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";

const PASSWORD_ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";

const generateSecurePassword = (length = 12): string => {
  const randomValues = new Uint32Array(length);
  crypto.getRandomValues(randomValues);
  return Array.from(
    randomValues,
    (value) => PASSWORD_ALPHABET[value % PASSWORD_ALPHABET.length],
  ).join("");
};

export const AdminEmployersPage = () => {
  const [employers, setEmployers] = useState<EmployerUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const [login, setLogin] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [organizationName, setOrganizationName] = useState("");
  const [password, setPassword] = useState("");

  const [searchQuery, setSearchQuery] = useState("");

  const [lastCreated, setLastCreated] = useState<{
    login: string;
    pass: string;
    link: string;
  } | null>(null);

  const loadData = async () => {
    try {
      const res = await usersApi.getEmployers();
      setEmployers(res.data);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const generatePassword = () => {
    setPassword(generateSecurePassword());
  };

  const openCreate = () => {
    setEditingId(null);
    setLogin("");
    setDisplayName("");
    setOrganizationName("");
    setPassword(generateSecurePassword());
    setIsFormOpen(true);
  };

  const openEdit = (e: EmployerUser) => {
    setEditingId(e.id);
    setLogin(e.login);
    setDisplayName(e.displayName);
    setOrganizationName(e.organizationName ?? "");
    setPassword("");
    setIsFormOpen(true);
  };

  useAdminPageConfig({
    title: "Настройки",
    subtitle: "Наниматели",
    actions: (
      <Button onClick={openCreate}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  const filteredEmployers = employers.filter(
    (e) =>
      e.login.toLowerCase().includes(searchQuery.toLowerCase()) ||
      e.displayName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (e.organizationName ?? "")
        .toLowerCase()
        .includes(searchQuery.toLowerCase()),
  );

  const handleDelete = async (id: string, name: string) => {
    if (
      !window.confirm(`Вы уверены, что хотите удалить нанимателя "${name}"?`)
    )
      return;
    try {
      await usersApi.deleteUser(id);
      toast.success("Наниматель удален");
      loadData();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка при удалении"));
    }
  };

  const handleCopyCredentials = async (created: {
    login: string;
    pass: string;
    link: string;
  }) => {
    const text = `Ссылка для входа: ${created.link}\nЛогин: ${created.login}\nПароль: ${created.pass}`;
    try {
      await navigator.clipboard.writeText(text);
      toast.success("Данные для входа скопированы");
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Не удалось скопировать"));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!login.trim() || !displayName.trim() || !organizationName.trim()) {
      toast.error("Заполните логин, наименование и организацию");
      return;
    }

    if (!editingId && !password.trim()) {
      toast.error("Пароль обязателен при создании");
      return;
    }

    try {
      setIsSubmitting(true);

      if (editingId) {
        await usersApi.updateUser(
          editingId,
          login.trim(),
          displayName.trim(),
          organizationName.trim(),
        );

        if (password.trim()) {
          await usersApi.setPassword(editingId, password.trim());
        }
        toast.success("Наниматель обновлен");
      } else {
        await usersApi.createEmployer(
          login.trim(),
          displayName.trim(),
          organizationName.trim(),
          password.trim(),
        );
        setLastCreated({
          login: login.trim(),
          pass: password.trim(),
          link: `${window.location.origin}/login`,
        });
        toast.success("Наниматель создан");
      }

      setIsFormOpen(false);
      setLogin("");
      setDisplayName("");
      setOrganizationName("");
      setPassword("");
      loadData();
    } catch (e) {
      toast.error(
        getApiErrorMessage(
          e,
          "Ошибка. Возможно, логин занят или содержит недопустимые символы.",
        ),
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <>
      {loading ? (
        <div className="p-8 text-center text-slate-500">Загрузка данных...</div>
      ) : (
        <AdminTable
          data={filteredEmployers}
          searchQuery={searchQuery}
          emptyText="Нет нанимателей"
          onSearchChange={setSearchQuery}
          searchPlaceholder="Поиск нанимателя..."
          topContent={
            lastCreated && (
              <div className="bg-green-50 border border-green-200 p-4 flex justify-between items-center animate-in slide-in-from-top-2">
                <div className="flex gap-4 items-center">
                  <div className="w-10 h-10  bg-green-100 text-green-600 flex items-center justify-center">
                    <Key size={20} />
                  </div>
                  <div>
                    <p className="text-green-900 font-bold text-sm uppercase">
                      Наниматель создан
                    </p>
                    <p className="text-sm text-green-700 mt-1">
                      Логин:{" "}
                      <b className="font-mono bg-white/50 px-1 ">
                        {lastCreated.login}
                      </b>{" "}
                      • Пароль:{" "}
                      <b className="font-mono bg-white/50 px-1 ">
                        {lastCreated.pass}
                      </b>{" "}
                      • Ссылка:{" "}
                      <b className="font-mono bg-white/50 px-1 ">
                        {lastCreated.link}
                      </b>
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => handleCopyCredentials(lastCreated)}
                    className="text-green-700 hover:bg-green-100"
                    title="Скопировать логин, пароль и ссылку"
                  >
                    <Copy size={18} />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setLastCreated(null)}
                    className="text-green-700 hover:bg-green-100"
                  >
                    <X size={18} />
                  </Button>
                </div>
              </div>
            )
          }
          columns={[
            { header: "Наниматель" },
            { header: "Организация" },
            { header: "Действия", className: "w-32 text-right" },
          ]}
          renderRow={(e) => (
            <TableRow key={e.id} className="group hover:bg-slate-50">
              <TableCell className="py-4">
                <div className="flex items-center gap-3">
                  <div className="p-2  bg-indigo-50 text-indigo-600">
                    <Building2 size={16} />
                  </div>
                  <div>
                    <div className="text-sm font-bold text-slate-900">
                      {e.displayName}
                    </div>
                    <div className="text-xs text-slate-500 font-mono">
                      {e.login}
                    </div>
                  </div>
                </div>
              </TableCell>
              <TableCell className="py-4 text-sm text-slate-700">
                {e.organizationName || "—"}
              </TableCell>
              <TableCell className="py-4 text-right">
                <AdminTableActions
                  onEdit={() => openEdit(e)}
                  onDelete={() => handleDelete(e.id, e.displayName)}
                />
              </TableCell>
            </TableRow>
          )}
        />
      )}

      <AdminModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingId ? "Редактирование нанимателя" : "Новый наниматель"}
        onSubmit={handleSubmit}
        submitText={isSubmitting ? "Сохранение..." : "Сохранить"}
      >
        <div className="space-y-2">
          <Label htmlFor="employer-login">Логин</Label>
          <Input
            id="employer-login"
            value={login}
            onChange={(e) => setLogin(e.target.value)}
            placeholder="Например: EMPLOYER01"
            autoFocus
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="employer-display-name">Контактное лицо</Label>
          <Input
            id="employer-display-name"
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
            placeholder="Например: Иванов И.И."
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="employer-organization">Организация</Label>
          <Input
            id="employer-organization"
            value={organizationName}
            onChange={(e) => setOrganizationName(e.target.value)}
            placeholder="Например: ООО «Пример»"
          />
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between gap-3">
            <Label htmlFor="employer-password">
              {editingId ? "Новый пароль" : "Пароль"}
            </Label>
            <Button
              type="button"
              variant="outline"
              onClick={generatePassword}
              className="h-8 px-3"
            >
              <RefreshCw size={14} className="mr-2" />
              Сгенерировать
            </Button>
          </div>
          <Input
            id="employer-password"
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
              ? "Если поле пустое, пароль нанимателя останется прежним."
              : "При создании пароль обязателен. После сохранения появится ссылка для входа."}
          </p>
        </div>
      </AdminModal>
    </>
  );
};
