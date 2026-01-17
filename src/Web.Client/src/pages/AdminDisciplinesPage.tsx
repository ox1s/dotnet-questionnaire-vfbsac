import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { dictionariesApi, type DictionaryItem } from "../api";
// 1. Добавляем иконки Pencil и X
import { ArrowLeft, Plus, BookOpen, Trash2, Pencil, X } from "lucide-react";

export const AdminDisciplinesPage = () => {
  const navigate = useNavigate();

  // Состояние данных
  // Важно: предполагаем, что интерфейс DictionaryItem в api.ts уже расширен полем departmentId?: string
  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Состояние формы
  const [newName, setNewName] = useState("");
  const [selectedDept, setSelectedDept] = useState("");

  // 2. Состояние редактирования (null = режим создания)
  const [editingId, setEditingId] = useState<string | null>(null);

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

  // --- Хендлеры ---

const startEdit = (d: DictionaryItem) => {
    setEditingId(d.id);
    setNewName(d.name);
    setSelectedDept(d.departmentId || "");
    
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Отмена редактирования
  const cancelEdit = () => {
    setEditingId(null);
    setNewName("");
    setSelectedDept("");
  };

  // Единый метод отправки (Create + Update)
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName || !selectedDept) {
      alert("Заполните название и выберите кафедру");
      return;
    }

    try {
      if (editingId) {
        // РЕЖИМ РЕДАКТИРОВАНИЯ
        await dictionariesApi.updateDiscipline(editingId, newName, selectedDept);
      } else {
        // РЕЖИМ СОЗДАНИЯ
        await dictionariesApi.createDiscipline(newName, selectedDept);
      }

      // Сброс
      cancelEdit();
      loadData();
    } catch (e) {
      alert("Ошибка при сохранении.");
    }
  };

  // Удаление
  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Удалить дисциплину "${name}"?`)) return;
    try {
      await dictionariesApi.deleteDiscipline(id);
      loadData();
    } catch (e) {
      alert("Ошибка при удалении.");
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
          <button onClick={() => navigate("/dashboard")} className="text-gray-500 hover:text-gray-800">
            <ArrowLeft />
          </button>
          <h1 className="text-xl font-bold text-gray-900 flex items-center gap-2">
            <BookOpen size={20}/> Управление дисциплинами
          </h1>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8 space-y-8">
        
        {/* Форма (Create / Update) */}
        <section className={`p-6 rounded-lg shadow-sm border transition-colors ${editingId ? "bg-blue-50 border-blue-200" : "bg-white border-gray-200"}`}>
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-md font-semibold text-blue-900 flex items-center gap-2">
               {editingId ? (
                  <> <Pencil size={18}/> Редактирование дисциплины </>
               ) : (
                  <> <Plus size={18}/> Добавить дисциплину </>
               )}
            </h2>
            {editingId && (
                <button onClick={cancelEdit} className="text-sm text-gray-500 hover:text-gray-700 flex items-center gap-1">
                    <X size={16}/> Отмена
                </button>
            )}
          </div>

          <form onSubmit={handleSubmit} className="flex flex-col md:flex-row gap-4 items-end">
            <div className="w-full md:w-1/2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Название предмета</label>
              <input 
                type="text" 
                className="input-field" 
                placeholder="Например: Высшая математика"
                value={newName}
                onChange={e => setNewName(e.target.value)}
              />
            </div>
            <div className="w-full md:w-1/3">
              <label className="block text-sm font-medium text-gray-700 mb-1">Читающая кафедра</label>
              <select 
                className="input-field"
                value={selectedDept}
                onChange={e => setSelectedDept(e.target.value)}
              >
                <option value="">-- Выберите --</option>
                {departments.map(d => (
                  <option key={d.id} value={d.id}>{d.name}</option>
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

        {/* Таблица */}
        <section className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">Название</th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">Кафедра</th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase text-right">Действия</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {disciplines.map((d) => (
                <tr key={d.id} className={`hover:bg-gray-50 group ${editingId === d.id ? "bg-blue-50" : ""}`}>
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">{d.name}</td>
                  <td className="px-6 py-4 text-sm text-gray-500">{getDeptName(d.departmentId)}</td>
                  
                  <td className="px-6 py-4 text-sm text-right space-x-2">
                    {/* Кнопка Редактировать */}
                    <button 
                       onClick={() => startEdit(d)}
                       className="text-gray-400 hover:text-blue-600 transition p-1"
                       title="Редактировать"
                    >
                      <Pencil size={18} />
                    </button>
                    
                    {/* Кнопка Удалить */}
                    <button 
                       onClick={() => handleDelete(d.id, d.name)}
                       className="text-gray-400 hover:text-red-600 transition p-1"
                       title="Удалить"
                    >
                      <Trash2 size={18} />
                    </button>
                  </td>
                </tr>
              ))}
              {disciplines.length === 0 && (
                 <tr><td colSpan={3} className="text-center p-4 text-gray-400">Список пуст</td></tr>
              )}
            </tbody>
          </table>
        </section>

      </main>
    </div>
  );
};
