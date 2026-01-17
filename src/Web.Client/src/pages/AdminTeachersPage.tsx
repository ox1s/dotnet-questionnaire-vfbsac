import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { dictionariesApi, type TeacherItem, type DictionaryItem } from "../api";
import { ArrowLeft, Plus, UserPlus, Users } from "lucide-react";

export const AdminTeachersPage = () => {
  const navigate = useNavigate();

  // Состояние данных
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Состояние формы добавления
  const [newName, setNewName] = useState("");
  const [selectedDept, setSelectedDept] = useState("");

  // Загрузка данных
  const loadData = async () => {
    try {
      const [teachersRes, deptsRes] = await Promise.all([
        dictionariesApi.getTeachers(),
        dictionariesApi.getDepartments(),
      ]);
      setTeachers(teachersRes.data);
      setDepartments(deptsRes.data);
    } catch (e) {
      console.error("Ошибка загрузки", e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  // Обработчик создания
  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName || !selectedDept) {
      alert("Заполните ФИО и выберите кафедру");
      return;
    }

    try {
      await dictionariesApi.createTeacher(newName, selectedDept);
      setNewName("");
      // Перезагружаем список, чтобы увидеть нового преподавателя
      loadData(); 
    } catch (e) {
      alert("Ошибка при создании. Убедитесь, что вы Админ.");
    }
  };

  // Хелпер для получения имени кафедры по ID
  const getDeptName = (id: string) => {
    return departments.find((d) => d.id === id)?.name || "Неизвестно";
  };

  if (loading) return <div className="p-8 text-center">Загрузка...</div>;

  return (
    <div className="min-h-screen bg-gray-50 pb-20">
      <header className="bg-white shadow-sm px-6 py-4 sticky top-0 z-10 border-b border-gray-200">
        <div className="max-w-4xl mx-auto flex items-center gap-4">
          <button
            onClick={() => navigate("/dashboard")}
            className="text-gray-500 hover:text-gray-800 transition"
          >
            <ArrowLeft />
          </button>
          <h1 className="text-xl font-bold text-gray-900 flex items-center gap-2">
             <Users size={20}/> Управление преподавателями
          </h1>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8 space-y-8">
        
        {/* Форма добавления */}
        <section className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h2 className="text-md font-semibold mb-4 text-blue-900 flex items-center gap-2">
            <UserPlus size={18}/> Добавить нового преподавателя
          </h2>
          <form onSubmit={handleCreate} className="flex flex-col md:flex-row gap-4 items-end">
            <div className="w-full md:w-1/2">
              <label className="block text-sm font-medium text-gray-700 mb-1">ФИО Преподавателя</label>
              <input 
                type="text" 
                className="input-field" 
                placeholder="Иванов Иван Иванович"
                value={newName}
                onChange={e => setNewName(e.target.value)}
              />
            </div>
            <div className="w-full md:w-1/3">
              <label className="block text-sm font-medium text-gray-700 mb-1">Кафедра</label>
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
            <button type="submit" className="btn-primary w-full md:w-auto">
              <Plus size={18} className="mr-1"/> Добавить
            </button>
          </form>
        </section>

        {/* Список */}
        <section className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">ФИО</th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">Кафедра</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {teachers.map((t) => (
                <tr key={t.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">{t.fullName}</td>
                  <td className="px-6 py-4 text-sm text-gray-500">{getDeptName(t.departmentId)}</td>
                </tr>
              ))}
              {teachers.length === 0 && (
                <tr>
                  <td colSpan={2} className="px-6 py-8 text-center text-gray-400">
                    Список пуст
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </section>

      </main>
    </div>
  );
};
