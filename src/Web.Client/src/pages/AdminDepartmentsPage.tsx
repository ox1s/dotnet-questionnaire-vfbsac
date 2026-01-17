import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { dictionariesApi, type DictionaryItem } from "../api";
import { ArrowLeft, Plus, Building2 } from "lucide-react";

export const AdminDepartmentsPage = () => {
  const navigate = useNavigate();
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Поле ввода
  const [newName, setNewName] = useState("");

  const loadData = async () => {
    try {
      const res = await dictionariesApi.getDepartments();
      setDepartments(res.data);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName.trim()) return;

    try {
      await dictionariesApi.createDepartment(newName);
      setNewName("");
      loadData();
    } catch (e) {
      alert("Ошибка. Возможно, такая кафедра уже есть.");
    }
  };

  if (loading) return <div className="p-8 text-center">Загрузка...</div>;

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
            <Building2 size={20} /> Управление кафедрами
          </h1>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8 space-y-8">
        {/* Форма добавления */}
        <section className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h2 className="text-md font-semibold mb-4 text-blue-900">
            Создать кафедру
          </h2>
          <form
            onSubmit={handleCreate}
            className="flex flex-col md:flex-row gap-4 items-end"
          >
            <div className="w-full">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Название / Аббревиатура
              </label>
              <input
                type="text"
                className="input-field"
                placeholder="Например: ИКТ или Кафедра информационных сетей"
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
              />
            </div>
            <button
              type="submit"
              className="btn-primary w-full md:w-auto flex-shrink-0"
            >
              <Plus size={18} className="mr-1" /> Добавить
            </button>
          </form>
        </section>

        {/* Список */}
        <section className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">
                  Название
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase w-20">
                  ID
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {departments.map((d) => (
                <tr key={d.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">
                    {d.name}
                  </td>
                  <td
                    className="px-6 py-4 text-sm text-gray-400 font-mono text-xs truncate max-w-[100px]"
                    title={d.id}
                  >
                    {d.id.substring(0, 8)}...
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
