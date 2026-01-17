import React, { useEffect, useState } from "react";
import { usersApi, type GroupUser } from "../api";
import { AdminLayout } from "../layouts/AdminLayout";
import { Plus, Users, Key } from "lucide-react";

export const AdminGroupsPage = () => {
  const [groups, setGroups] = useState<GroupUser[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
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
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const generatePassword = () =>
    setPassword(Math.floor(10000000 + Math.random() * 90000000).toString());

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await usersApi.createGroup(groupName, password);
      setLastCreated({ name: groupName, pass: password });
      setGroupName("");
      setPassword("");
      loadData();
      setIsFormOpen(false);
    } catch (e) {
      alert("Ошибка");
    }
  };

  return (
    <AdminLayout
      title="Студенческие группы"
      subtitle="Управление учетными записями групп."
      actions={
        <button
          onClick={() => setIsFormOpen(true)}
          className="flex items-center gap-2 px-5 py-2.5 bg-slate-800 text-white rounded-xl hover:bg-slate-900 font-bold shadow-lg shadow-slate-800/20 text-sm active:scale-95 transition-all"
        >
          <Plus size={18} /> Создать группу
        </button>
      }
    >
      {lastCreated && (
        <div className="bg-green-50 border border-green-200 p-4 rounded-xl flex justify-between items-center mb-6">
          <div className="flex gap-4 items-center">
            <div className="w-10 h-10 rounded-full bg-green-100 text-green-600 flex items-center justify-center">
              <Key size={20} />
            </div>
            <div>
              <p className="text-green-900 font-bold">
                Группа успешно создана!
              </p>
              <p className="text-sm text-green-700">
                Логин: <b>{lastCreated.name}</b> / Пароль:{" "}
                <b>{lastCreated.pass}</b>
              </p>
            </div>
          </div>
          <button
            onClick={() => setLastCreated(null)}
            className="text-green-600 font-bold text-sm"
          >
            Закрыть
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
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase">
                Отображаемое имя
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {groups.map((g) => (
              <tr key={g.id} className="hover:bg-slate-50">
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
                <td className="py-4 px-6 text-sm text-slate-600">
                  {g.displayName}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {isFormOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div
            className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm"
            onClick={() => setIsFormOpen(false)}
          ></div>
          <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6">
            <h3 className="text-lg font-bold text-slate-900 mb-4">
              Регистрация группы
            </h3>
            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                  Название (5 символов)
                </label>
                <input
                  className="w-full p-3 bg-slate-50 border border-slate-200 rounded-lg text-sm uppercase"
                  maxLength={5}
                  placeholder="ПО111"
                  value={groupName}
                  onChange={(e) => setGroupName(e.target.value.toUpperCase())}
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                  Пароль
                </label>
                <div className="flex gap-2">
                  <input
                    className="w-full p-3 bg-slate-50 border border-slate-200 rounded-lg text-sm font-mono"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                  <button
                    type="button"
                    onClick={generatePassword}
                    className="px-3 bg-slate-100 rounded-lg text-slate-600 hover:bg-slate-200"
                  >
                    <Key size={18} />
                  </button>
                </div>
              </div>

              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => setIsFormOpen(false)}
                  className="..."
                >
                  Отмена
                </button>
                <button
                  type="submit"
                  className="flex-1 py-2.5 rounded-lg bg-slate-800 text-white font-bold text-sm hover:bg-slate-900 shadow-lg"
                >
                  Создать
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};
