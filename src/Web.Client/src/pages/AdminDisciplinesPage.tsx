import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { dictionariesApi, type DictionaryItem } from "../api";
import { ArrowLeft, Plus, BookOpen } from "lucide-react";

export const AdminDisciplinesPage = () => {
  const navigate = useNavigate();

  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Поля формы
  const [newName, setNewName] = useState("");
  const [selectedDept, setSelectedDept] = useState("");

  // Загрузка
  const loadData = async () => {
    try {
      const [discRes, deptsRes] = await Promise.all([
        dictionariesApi.getDisciplines(),
        dictionariesApi.getDepartments(),
      ]);
      setDisciplines(discRes.data);
      setDepartments(deptsRes.data);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  // Создание
  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName || !selectedDept) {
      alert("Заполните название и выберите кафедру");
      return;
    }

    try {
      await dictionariesApi.createDiscipline(newName, selectedDept);
      setNewName("");
      loadData(); // Обновить таблицу
    } catch (e) {
      alert("Ошибка при создании.");
    }
  };

  const getDeptName = (deptId?: string) => {
    if (!deptId) return "-";
    return departments.find((d) => d.id === deptId)?.name || "Неизвестно";
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
            <BookOpen size={20} /> Управление дисциплинами
          </h1>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8 space-y-8">
        {/* Форма */}
        <section className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h2 className="text-md font-semibold mb-4 text-blue-900">
            Добавить дисциплину
          </h2>
          <form
            onSubmit={handleCreate}
            className="flex flex-col md:flex-row gap-4 items-end"
          >
            <div className="w-full md:w-1/2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Название предмета
              </label>
              <input
                type="text"
                className="input-field"
                placeholder="Например: Высшая математика"
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
              />
            </div>
            <div className="w-full md:w-1/3">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Читающая кафедра
              </label>
              <select
                className="input-field"
                value={selectedDept}
                onChange={(e) => setSelectedDept(e.target.value)}
              >
                <option value="">-- Выберите --</option>
                {departments.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
            </div>
            <button type="submit" className="btn-primary w-full md:w-auto">
              <Plus size={18} className="mr-1" /> Добавить
            </button>
          </form>
        </section>

        {/* Таблица */}
        <section className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">
                  Название
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">
                  Кафедра
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {disciplines.map((d: any) => (
                <tr key={d.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">
                    {d.name}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500">
                    {getDeptName(d.departmentId)}
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
