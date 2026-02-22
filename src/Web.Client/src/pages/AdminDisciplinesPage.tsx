import React, { useEffect, useState } from "react";
import { dictionariesApi, type DictionaryItem } from "../api";
import { AdminLayout } from "../layouts/AdminLayout";
import { Plus, Search, Edit2, Trash2, Book } from "lucide-react";

export const AdminDisciplinesPage = () => {
  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const [newName, setNewName] = useState("");
  const [selectedDept, setSelectedDept] = useState("");
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
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const filteredDisciplines = disciplines.filter((d) =>
    d.name.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  const getDeptName = (deptId?: string) =>
    departments.find((d) => d.id === deptId)?.name || "-";

  const openModal = (d?: DictionaryItem) => {
    if (d) {
      setEditingId(d.id);
      setNewName(d.name);
      setSelectedDept(d.departmentId || "");
    } else {
      setEditingId(null);
      setNewName("");
      setSelectedDept("");
    }
    setIsFormOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Удалить дисциплину?")) return;
    try {
      await dictionariesApi.deleteDiscipline(id);
      loadData();
    } catch (e) {
      alert("Ошибка");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId)
        await dictionariesApi.updateDiscipline(
          editingId,
          newName,
          selectedDept,
        );
      else await dictionariesApi.createDiscipline(newName, selectedDept);
      setIsFormOpen(false);
      loadData();
    } catch (e) {
      alert("Ошибка");
    }
  };

  return (
    <AdminLayout
      title="Дисциплины"
      subtitle="Справочник учебных предметов и читающих кафедр."
      actions={
        <button
          onClick={() => openModal()}
          className="flex items-center gap-2 px-5 py-2.5 bg-slate-800 text-white rounded-xl hover:bg-slate-900 font-bold shadow-lg shadow-slate-800/20 text-sm active:scale-95 transition-all"
        >
          <Plus size={18} /> Добавить
        </button>
      }
    >
      <div className="bg-white p-4 rounded-xl shadow-sm border border-slate-200 mb-6">
        <div className="relative w-full md:max-w-md">
          <Search
            className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            size={18}
          />
          <input
            className="w-full pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:ring-2 focus:ring-primary/20 outline-none"
            placeholder="Поиск предмета..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
      </div>

      {/* Таблица */}
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50/50 border-b border-slate-200">
              <th className="py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase">
                Название
              </th>
              <th className="py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase">
                Кафедра
              </th>
              <th className="py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase text-right w-12 md:w-24"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filteredDisciplines.map((d) => (
              <tr
                key={d.id}
                className="group hover:bg-slate-50 transition-colors"
              >
                <td className="py-3 px-3 md:py-4 md:px-6 align-top">
                  <div className="flex items-start gap-2 md:gap-3">
                    <div className="p-1.5 md:p-2 rounded-lg bg-blue-50 text-blue-600 shrink-0 mt-0.5">
                      <Book size={14} className="md:w-[16px] md:h-[16px]" />
                    </div>
                    {/* Перенос названия на 3 строки */}
                    <span className="text-xs md:text-sm font-bold text-slate-900 line-clamp-3 leading-snug">
                      {d.name}
                    </span>
                  </div>
                </td>
                <td className="py-3 px-3 md:py-4 md:px-6 align-top">
                  <span className="inline-block px-2 py-1 rounded text-[10px] md:text-xs font-medium bg-slate-100 text-slate-600 border border-slate-200 line-clamp-3 leading-tight">
                    {getDeptName(d.departmentId)}
                  </span>
                </td>
                <td className="py-3 px-3 md:py-4 md:px-6 align-top text-right">
                  <div className="flex flex-col sm:flex-row items-end justify-end gap-1 opacity-100 lg:opacity-0 lg:group-hover:opacity-100 transition-opacity">
                    <button
                      onClick={() => openModal(d)}
                      className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-primary hover:bg-primary/10 transition-colors"
                    >
                      <Edit2 size={16} className="md:w-[18px] md:h-[18px]" />
                    </button>
                    <button
                      onClick={() => handleDelete(d.id)}
                      className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-accent hover:bg-accent/10 transition-colors"
                    >
                      <Trash2 size={16} className="md:w-[18px] md:h-[18px]" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {filteredDisciplines.length === 0 && (
              <tr>
                <td
                  colSpan={3}
                  className="p-8 text-center text-slate-400 text-sm"
                >
                  Ничего не найдено
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
          <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6">
            <h3 className="text-lg font-bold text-slate-900 mb-4">
              {editingId ? "Редактирование" : "Новая дисциплина"}
            </h3>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                  Название
                </label>
                <input
                  className="w-full p-3 bg-slate-50 border border-slate-200 rounded-lg text-sm"
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                  Кафедра
                </label>
                <select
                  className="w-full p-3 bg-slate-50 border border-slate-200 rounded-lg text-sm"
                  value={selectedDept}
                  onChange={(e) => setSelectedDept(e.target.value)}
                >
                  <option value="">Выберите...</option>
                  {departments.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => setIsFormOpen(false)}
                  className="flex-1 py-2.5 rounded-lg border border-slate-200 text-slate-600 font-bold text-sm"
                >
                  Отмена
                </button>
                <button
                  type="submit"
                  className="flex-1 py-2.5 rounded-lg bg-slate-800 text-white font-bold text-sm"
                >
                  Сохранить
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};
