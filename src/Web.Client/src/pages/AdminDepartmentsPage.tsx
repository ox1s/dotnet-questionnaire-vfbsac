import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { dictionariesApi, type DictionaryItem } from "../api";
// 1. Добавляем иконки Pencil и X
import { ArrowLeft, Plus, Building2, Trash2, Pencil, X } from "lucide-react";

export const AdminDepartmentsPage = () => {
  const navigate = useNavigate();
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Поле ввода
  const [newName, setNewName] = useState("");

  // 2. Состояние редактирования
  const [editingId, setEditingId] = useState<string | null>(null);

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

  // --- Хендлеры ---

  const startEdit = (d: DictionaryItem) => {
    setEditingId(d.id);
    setNewName(d.name);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const cancelEdit = () => {
    setEditingId(null);
    setNewName("");
  };

  // Единый метод (Create + Update)
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName.trim()) return;

    try {
      if (editingId) {
        // UPDATE
        await dictionariesApi.updateDepartment(editingId, newName);
      } else {
        // CREATE
        await dictionariesApi.createDepartment(newName);
      }

      cancelEdit();
      loadData();
    } catch (e) {
      alert("Ошибка. Возможно, такая кафедра уже есть.");
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Вы уверены, что хотите удалить кафедру "${name}"?`)) {
      return;
    }

    try {
      await dictionariesApi.deleteDepartment(id);
      loadData();
    } catch (e) {
      console.error(e);
      alert(
        "Не удалось удалить кафедру. Возможно, к ней привязаны преподаватели или дисциплины.",
      );
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
        {/* Форма (Create / Update) */}
        <section
          className={`p-6 rounded-lg shadow-sm border transition-colors ${editingId ? "bg-blue-50 border-blue-200" : "bg-white border-gray-200"}`}
        >
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-md font-semibold text-blue-900 flex items-center gap-2">
              {editingId ? (
                <>
                  {" "}
                  <Pencil size={18} /> Редактирование кафедры{" "}
                </>
              ) : (
                <>
                  {" "}
                  <Plus size={18} /> Создать кафедру{" "}
                </>
              )}
            </h2>
            {editingId && (
              <button
                onClick={cancelEdit}
                className="text-sm text-gray-500 hover:text-gray-700 flex items-center gap-1"
              >
                <X size={16} /> Отмена
              </button>
            )}
          </div>

          <form
            onSubmit={handleSubmit}
            className="flex flex-col md:flex-row gap-4 items-end"
          >
            <div className="w-full">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Название / Аббревиатура
              </label>
              <input
                type="text"
                className="input-field"
                placeholder="Например: ИКТ"
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
              />
            </div>
            <button
              type="submit"
              className={`btn-primary w-full md:w-auto flex-shrink-0 ${editingId ? "bg-amber-600 hover:bg-amber-700" : ""}`}
            >
              {editingId ? "Сохранить" : "Добавить"}
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
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase w-32 text-right">
                  Действия
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {departments.map((d) => (
                <tr
                  key={d.id}
                  className={`hover:bg-gray-50 group ${editingId === d.id ? "bg-blue-50" : ""}`}
                >
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">
                    {d.name}
                  </td>
                  <td
                    className="px-6 py-4 text-sm text-gray-400 font-mono text-xs truncate max-w-[100px]"
                    title={d.id}
                  >
                    {d.id.substring(0, 8)}...
                  </td>
                  <td className="px-6 py-4 text-sm text-right space-x-2">
                    {/* Редактировать */}
                    <button
                      onClick={() => startEdit(d)}
                      className="text-gray-400 hover:text-blue-600 transition-colors p-1"
                      title="Редактировать"
                    >
                      <Pencil size={18} />
                    </button>

                    {/* Удалить */}
                    <button
                      onClick={() => handleDelete(d.id, d.name)}
                      className="text-gray-400 hover:text-red-600 transition-colors p-1"
                      title="Удалить"
                    >
                      <Trash2 size={18} />
                    </button>
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
