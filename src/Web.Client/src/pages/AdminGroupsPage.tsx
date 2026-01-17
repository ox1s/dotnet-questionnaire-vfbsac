import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { usersApi, type GroupUser } from "../api";
import { ArrowLeft, Users, Plus, Key } from "lucide-react";

export const AdminGroupsPage = () => {
  const navigate = useNavigate();
  const [groups, setGroups] = useState<GroupUser[]>([]);
  const [loading, setLoading] = useState(true);

  // Форма
  const [groupName, setGroupName] = useState("");
  const [password, setPassword] = useState("");

  // Показать созданные данные (чтобы админ мог скопировать)
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
    // Простая генерация случайного пароля из 8 цифр
    const pass = Math.floor(10000000 + Math.random() * 90000000).toString();
    setPassword(pass);
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!groupName || !password) {
      alert("Введите название группы и пароль");
      return;
    }

    try {
      await usersApi.createGroup(groupName, password);

      // Сохраняем для отображения
      setLastCreated({ name: groupName, pass: password });

      // Сброс
      setGroupName("");
      setPassword("");
      loadData();
    } catch (e) {
      alert(
        "Ошибка. Возможно, группа с таким именем уже есть (или имя не 5 символов, см. валидацию).",
      );
    }
  };

  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="min-h-screen bg-gray-50 pb-20">
      <header className="bg-white shadow-sm px-6 py-4 sticky top-0 z-10 border-b border-gray-200">
        <div className="max-w-4xl mx-auto flex items-center gap-4">
          <button
            onClick={() => navigate("/dashboard")}
            className="text-gray-500 hover:text-gray-800"
          >
            <ArrowLeft />
          </button>
          <h1 className="text-xl font-bold text-gray-900 flex items-center gap-2">
            <Users size={20} /> Учетные записи групп
          </h1>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8 space-y-8">
        {/* Уведомление об успехе */}
        {lastCreated && (
          <div className="bg-green-50 border border-green-200 p-4 rounded-lg flex justify-between items-center animate-pulse-once">
            <div>
              <p className="text-green-800 font-medium">
                Группа успешно создана!
              </p>
              <p className="text-sm text-green-700">
                Логин:{" "}
                <span className="font-mono font-bold">{lastCreated.name}</span>{" "}
                <br />
                Пароль:{" "}
                <span className="font-mono font-bold">{lastCreated.pass}</span>
              </p>
            </div>
            <button
              onClick={() => setLastCreated(null)}
              className="text-green-600 hover:text-green-800"
            >
              Закрыть
            </button>
          </div>
        )}

        {/* Форма создания */}
        <section className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h2 className="text-md font-semibold mb-4 text-blue-900">
            Регистрация новой группы
          </h2>
          <form
            onSubmit={handleCreate}
            className="flex flex-col md:flex-row gap-4 items-end"
          >
            <div className="w-full md:w-1/3">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Название (например: ПО111)
              </label>
              <input
                type="text"
                className="input-field uppercase"
                placeholder="ПО111"
                maxLength={5}
                value={groupName}
                onChange={(e) => setGroupName(e.target.value.toUpperCase())}
              />
            </div>
            <div className="w-full md:w-1/3">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Пароль
              </label>
              <div className="flex gap-2">
                <input
                  type="text"
                  className="input-field font-mono"
                  placeholder="12345678"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
                <button
                  type="button"
                  onClick={generatePassword}
                  className="p-2 border border-gray-300 rounded hover:bg-gray-100"
                  title="Сгенерировать пароль"
                >
                  <Key size={18} className="text-gray-600" />
                </button>
              </div>
            </div>
            <button type="submit" className="btn-primary w-full md:w-auto">
              <Plus size={18} className="mr-1" /> Создать
            </button>
          </form>
        </section>

        {/* Список */}
        <section className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">
                  Группа (Логин)
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">
                  Отображаемое имя
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {groups.map((g) => (
                <tr key={g.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm font-bold text-gray-900 font-mono">
                    {g.login}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500">
                    {g.displayName}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      </main>
    </div>
  );
};
