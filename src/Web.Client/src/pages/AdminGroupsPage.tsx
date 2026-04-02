import React, { useEffect, useState } from "react";
import { usersApi, type GroupUser } from "../api";
import {
  AdminLayout,
  AdminTable,
  AdminTableActions,
} from "@/components/AdminShared";
import { Button } from "@/components/ui/button";
import { TableCell, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";

import { Plus, Users, Key, X } from "lucide-react";

export const AdminGroupsPage = () => {
  const [groups, setGroups] = useState<GroupUser[]>([]);
  const [loading, setLoading] = useState(true);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const [groupName, setGroupName] = useState("");
  const [password, setPassword] = useState("");

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
    setPassword("");
    setIsFormOpen(true);
  };

  const openEdit = (g: GroupUser) => {
    setEditingId(g.id);
    setGroupName(g.login);
    setPassword("");
    setIsFormOpen(true);
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Вы уверены, что хотите удалить группу "${name}"?`))
      return;
    try {
      await usersApi.deleteUser(id);
      loadData();
    } catch (e) {
      alert("Ошибка при удалении");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await usersApi.updateUser(editingId, groupName, groupName);

        if (password) {
          await usersApi.setPassword(editingId, password);
        }
        alert("Группа обновлена");
      } else {
        if (!password) {
          alert("Пароль обязателен при создании");
          return;
        }
        await usersApi.createGroup(groupName, password);
        setLastCreated({ name: groupName, pass: password });
      }

      setIsFormOpen(false);
      setGroupName("");
      setPassword("");
      loadData();
    } catch (e) {
      alert("Ошибка. Возможно, имя занято или недопустимо.");
    }
  };

  return (
    <AdminLayout
      title="Студенческие группы"
      subtitle="Управление учетными записями групп."
      actions={
        <Button
          onClick={openCreate}
          className="gap-2 bg-slate-800 hover:bg-slate-900"
        >
          <Plus size={18} /> Создать группу
        </Button>
      }
    >
      {loading ? (
        <div className="p-8 text-center text-slate-500">Загрузка данных...</div>
      ) : (
        <AdminTable
          data={groups}
          emptyText="Нет групп"
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

      {/* Модалка */}
    </AdminLayout>
  );
};
