import React, { useEffect, useState } from "react";
import { getApiErrorMessage, usersApi, type GroupUser } from "../../api";
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
import { Plus, Users, Key, X, RefreshCw } from "lucide-react";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";

export const AdminGroupsPage = () => {
  const [groups, setGroups] = useState<GroupUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const [groupName, setGroupName] = useState("");
  const [password, setPassword] = useState("");

  const [searchQuery, setSearchQuery] = useState("");

  const [lastCreated, setLastCreated] = useState<{
    name: string;
    pass: string;
  } | null>(null);

  const loadData = async () => {
    try {
      const res = await usersApi.getGroups();
      setGroups(res.data);
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
    const pass = Math.floor(10000000 + Math.random() * 90000000).toString();
    setPassword(pass);
  };

  const openCreate = () => {
    setEditingId(null);
    setGroupName("");
    setPassword(Math.floor(10000000 + Math.random() * 90000000).toString());
    setIsFormOpen(true);
  };

  const openEdit = (g: GroupUser) => {
    setEditingId(g.id);
    setGroupName(g.login);
    setPassword("");
    setIsFormOpen(true);
  };

  useAdminPageConfig({
    title: "Настройки",
    subtitle: "Группы",
    actions: (
      <Button onClick={openCreate}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  const filteredGroups = groups.filter((g) =>
    g.login.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Вы уверены, что хотите удалить группу "${name}"?`))
      return;
    try {
      await usersApi.deleteUser(id);
      toast.success("Группа удалена");
      loadData();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка при удалении"));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!groupName.trim()) {
      toast.error("Введите логин группы");
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
          groupName.trim(),
          groupName.trim(),
        );

        if (password.trim()) {
          await usersApi.setPassword(editingId, password.trim());
        }
        toast.success("Группа обновлена");
      } else {
        await usersApi.createGroup(groupName.trim(), password.trim());
        setLastCreated({ name: groupName.trim(), pass: password.trim() });
        toast.success("Группа создана");
      }

      setIsFormOpen(false);
      setGroupName("");
      setPassword("");
      loadData();
    } catch (e) {
      toast.error(
        getApiErrorMessage(
          e,
          "Ошибка. Возможно, имя занято или содержит недопустимые символы.",
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
          data={filteredGroups}
          searchQuery={searchQuery}
          emptyText="Нет групп"
          onSearchChange={setSearchQuery}
          topContent={
            lastCreated && (
              <div className="bg-green-50 border border-green-200 p-4 flex justify-between items-center animate-in slide-in-from-top-2">
                <div className="flex gap-4 items-center">
                  <div className="w-10 h-10  bg-green-100 text-green-600 flex items-center justify-center">
                    <Key size={20} />
                  </div>
                  <div>
                    <p className="text-green-900 font-bold text-sm uppercase">
                      Группа создана
                    </p>
                    <p className="text-sm text-green-700 mt-1">
                      Логин:{" "}
                      <b className="font-mono bg-white/50 px-1 ">
                        {lastCreated.name}
                      </b>{" "}
                      • Пароль:{" "}
                      <b className="font-mono bg-white/50 px-1 ">
                        {lastCreated.pass}
                      </b>
                    </p>
                  </div>
                </div>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => setLastCreated(null)}
                  className="text-green-700 hover:bg-green-100"
                >
                  <X size={18} />
                </Button>
              </div>
            )
          }
          columns={[
            { header: "Логин / Группа" },
            { header: "Действия", className: "w-32 text-right" },
          ]}
          renderRow={(g) => (
            <TableRow key={g.id} className="group hover:bg-slate-50">
              <TableCell className="py-4">
                <div className="flex items-center gap-3">
                  <div className="p-2  bg-indigo-50 text-indigo-600">
                    <Users size={16} />
                  </div>
                  <span className="text-sm font-bold text-slate-900 font-mono">
                    {g.login}
                  </span>
                </div>
              </TableCell>
              <TableCell className="py-4 text-right">
                <AdminTableActions
                  onEdit={() => openEdit(g)}
                  onDelete={() => handleDelete(g.id, g.login)}
                />
              </TableCell>
            </TableRow>
          )}
        />
      )}

      <AdminModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingId ? "Редактирование группы" : "Новая группа"}
        onSubmit={handleSubmit}
        submitText={isSubmitting ? "Сохранение..." : "Сохранить"}
      >
        <div className="space-y-2">
          <Label htmlFor="group-login">Логин группы</Label>
          <Input
            id="group-login"
            value={groupName}
            onChange={(e) => setGroupName(e.target.value)}
            placeholder="Например: ИС-21"
            autoFocus
          />
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between gap-3">
            <Label htmlFor="group-password">
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
            id="group-password"
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
              ? "Если поле пустое, пароль группы останется прежним."
              : "При создании пароль обязателен."}
          </p>
        </div>
      </AdminModal>
    </>
  );
};
