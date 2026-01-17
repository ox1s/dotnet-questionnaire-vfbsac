import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { dictionariesApi, type TeacherItem, type DictionaryItem } from "../api";
import { ArrowLeft, UserPlus, Users, Trash2, Pencil, X } from "lucide-react";

export const AdminTeachersPage = () => {
  const navigate = useNavigate();
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Состояние формы
  const [newName, setNewName] = useState("");
  const [selectedDept, setSelectedDept] = useState("");

  // Состояние редактирования (если null - значит режим создания)
  const [editingId, setEditingId] = useState<string | null>(null);

  const loadData = async () => {
    try {
      const [teachersRes, deptsRes] = await Promise.all([
        dictionariesApi.getTeachers(),
        dictionariesApi.getDepartments(),
      ]);
      setTeachers(teachersRes.data);
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

  // Нажатие на кнопку "Редактировать" в строке
  const startEdit = (t: TeacherItem) => {
    setEditingId(t.id);
    setNewName(t.fullName);
    setSelectedDept(t.departmentId);
    // Скролл наверх к форме
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  // Отмена редактирования
  const cancelEdit = () => {
    setEditingId(null);
    setNewName("");
    setSelectedDept("");
  };

  // Единый обработчик отправки формы
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName || !selectedDept) {
      alert("Заполните все поля");
      return;
    }

    try {
      if (editingId) {
        // Режим UPDATE
        await dictionariesApi.updateTeacher(editingId, newName, selectedDept);
      } else {
        // Режим CREATE
        await dictionariesApi.createTeacher(newName, selectedDept);
      }

      // Сброс и обновление
      cancelEdit();
      loadData();
    } catch (e) {
      alert("Ошибка при сохранении.");
    }
  };

  // Удаление (из прошлого шага)
  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Удалить "${name}"?`)) return;
    try {
      await dictionariesApi.deleteTeacher(id);
      loadData();
    } catch (e) {
      alert("Ошибка удаления");
    }
  };

  const getDeptName = (id: string) =>
    departments.find((d) => d.id === id)?.name || "-";

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
            <Users size={20} /> Управление преподавателями
          </h1>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8 space-y-8">
        {/* Форма (Универсальная: Create / Update) */}
        <section
          className={`p-6 rounded-lg shadow-sm border transition-colors ${editingId ? "bg-blue-50 border-blue-200" : "bg-white border-gray-200"}`}
        >
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-md font-semibold text-blue-900 flex items-center gap-2">
              {editingId ? (
                <>
                  {" "}
                  <Pencil size={18} /> Редактирование преподавателя{" "}
                </>
              ) : (
                <>
                  {" "}
                  <UserPlus size={18} /> Добавить нового преподавателя{" "}
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
            <div className="w-full md:w-1/2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                ФИО Преподавателя
              </label>
              <input
                type="text"
                className="input-field"
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
              />
            </div>
            <div className="w-full md:w-1/3">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Кафедра
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
            <button
              type="submit"
              className={`btn-primary w-full md:w-auto ${editingId ? "bg-amber-600 hover:bg-amber-700" : ""}`}
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
                  ФИО
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">
                  Кафедра
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase text-right">
                  Действия
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {teachers.map((t) => (
                <tr
                  key={t.id}
                  className={`hover:bg-gray-50 group ${editingId === t.id ? "bg-blue-50" : ""}`}
                >
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">
                    {t.fullName}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500">
                    {getDeptName(t.departmentId)}
                  </td>
                  <td className="px-6 py-4 text-sm text-right space-x-2">
                    {/* Кнопка Редактировать */}
                    <button
                      onClick={() => startEdit(t)}
                      className="text-gray-400 hover:text-blue-600 transition p-1"
                      title="Редактировать"
                    >
                      <Pencil size={18} />
                    </button>
                    {/* Кнопка Удалить */}
                    <button
                      onClick={() => handleDelete(t.id, t.fullName)}
                      className="text-gray-400 hover:text-red-600 transition p-1"
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
