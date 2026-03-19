import React, { useEffect, useState } from "react";
import { usersApi, type GroupUser } from "../api";
import { AdminLayout } from "../layouts/AdminLayout";
import { Plus, Users, Key, Edit2, Trash2, X } from "lucide-react";

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
        <button
          onClick={openCreate}
          className="flex items-center gap-2 px-5 py-2.5 bg-slate-800 text-white rounded-xl hover:bg-slate-900 font-bold shadow-lg shadow-slate-800/20 text-sm active:scale-95 transition-all"
        >
          <Plus size={18} /> Создать группу
        </button>
      }
    >
      {loading ? (
        <div className="p-8 text-center text-slate-500">Загрузка данных...</div>
      ) : (
        <>
          {lastCreated && (
            <div className="bg-green-50 border border-green-200 p-4 rounded-xl flex justify-between items-center mb-6 animate-in slide-in-from-top-2">
              <div className="flex gap-4 items-center">
                <div className="w-10 h-10 rounded-full bg-green-100 text-green-600 flex items-center justify-center shadow-sm">
                  <Key size={20} />
                </div>
                <div>
                  <p className="text-green-900 font-bold text-sm uppercase tracking-wide">
                    Группа создана
                  </p>
                  <p className="text-sm text-green-700 mt-1">
                    Логин:{" "}
                    <b className="font-mono bg-white/50 px-1 rounded">
                      {lastCreated.name}
                    </b>{" "}
                    &nbsp;•&nbsp; Пароль:{" "}
                    <b className="font-mono bg-white/50 px-1 rounded">
                      {lastCreated.pass}
                    </b>
                  </p>
                </div>
              </div>
              <button
                onClick={() => setLastCreated(null)}
                className="p-2 hover:bg-green-100 rounded-lg text-green-700 transition-colors"
              >
                <X size={18} />
              </button>
            </div>
          )}

          <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-slate-50/50 border-b border-slate-200">
                  <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase">
                    Логин / Группа
                  </th>
                  <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase w-32 text-right">
                    Действия
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {groups.map((g) => (
                  <tr
                    key={g.id}
                    className="group hover:bg-slate-50 transition-colors"
                  >
                    <td className="py-4 px-6">
                      <div className="flex items-center gap-3">
                        <div className="p-2 rounded-lg bg-indigo-50 text-indigo-600">
                          <Users size={16} />
                        </div>
                        <span className="text-sm font-bold text-slate-900 font-mono">
                          {g.login}
                        </span>
                      </div>
                    </td>
                    <td className="py-4 px-6 text-right">
                      <div className="flex items-center justify-end gap-2 opacity-100 lg:opacity-0 lg:group-hover:opacity-100 transition-opacity">
                        {" "}
                        <button
                          onClick={() => openEdit(g)}
                          className="p-1.5 rounded-md text-slate-400 hover:text-primary hover:bg-primary/10 transition-colors"
                          title="Редактировать / Сменить пароль"
                        >
                          <Edit2 size={18} />
                        </button>
                        <button
                          onClick={() => handleDelete(g.id, g.login)}
                          className="p-1.5 rounded-md text-slate-400 hover:text-red-600 hover:bg-red-50 transition-colors"
                          title="Удалить"
                        >
                          <Trash2 size={18} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {groups.length === 0 && (
                  <tr>
                    <td colSpan={2} className="p-8 text-center text-slate-400">
                      Нет групп
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {isFormOpen && (
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
              <div
                className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm"
                onClick={() => setIsFormOpen(false)}
              ></div>
              <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6 animate-in fade-in zoom-in-95 duration-200">
                <div className="flex justify-between items-center mb-6">
                  <h3 className="text-lg font-bold text-slate-900">
                    {editingId ? "Редактирование группы" : "Новая группа"}
                  </h3>
                  <button
                    onClick={() => setIsFormOpen(false)}
                    className="text-slate-400 hover:text-slate-600"
                  >
                    <X size={20} />
                  </button>
                </div>

                <form onSubmit={handleSubmit} className="space-y-5">
                  <div>
                    <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                      Название (Логин)
                    </label>
                    <input
                      className="w-full p-3 bg-slate-50 border border-slate-200 rounded-xl text-sm font-bold uppercase placeholder:font-normal"
                      maxLength={5}
                      placeholder="ПО111"
                      value={groupName}
                      onChange={(e) =>
                        setGroupName(e.target.value.toUpperCase())
                      }
                    />
                    <p className="text-[10px] text-slate-400 mt-1 ml-1">
                      Ровно 5 символов
                    </p>
                  </div>

                  <div>
                    <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                      {editingId
                        ? "Новый пароль (оставьте пустым, чтобы не менять)"
                        : "Пароль"}
                    </label>
                    <div className="flex gap-2">
                      <input
                        className="w-full p-3 bg-slate-50 border border-slate-200 rounded-xl text-sm font-mono"
                        value={password}
                        placeholder={editingId ? "••••••••" : ""}
                        onChange={(e) => setPassword(e.target.value)}
                      />
                      <button
                        type="button"
                        onClick={generatePassword}
                        className="px-3 bg-slate-100 border border-slate-200 rounded-xl text-slate-600 hover:bg-slate-200 hover:border-slate-300 transition-colors"
                        title="Сгенерировать"
                      >
                        <Key size={18} />
                      </button>
                    </div>
                  </div>

                  <div className="flex gap-3 pt-2">
                    <button
                      type="button"
                      onClick={() => setIsFormOpen(false)}
                      className="flex-1 py-3 rounded-xl border border-slate-200 text-slate-600 font-bold text-sm hover:bg-slate-50 transition-colors"
                    >
                      Отмена
                    </button>
                    <button
                      type="submit"
                      className="flex-1 py-3 rounded-xl bg-slate-800 text-white font-bold text-sm hover:bg-slate-900 shadow-lg shadow-slate-800/20 transition-all flex items-center justify-center gap-2"
                    >
                      {editingId ? "Сохранить" : "Создать"}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          )}
        </>
      )}
    </AdminLayout>
  );
};
