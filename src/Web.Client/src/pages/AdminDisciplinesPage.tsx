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

      <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50/50 border-b border-slate-200">
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase">
                Название
              </th>
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase">
                Кафедра
              </th>
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase text-right">
                Действия
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filteredDisciplines.map((d) => (
              <tr
                key={d.id}
                className="group hover:bg-slate-50 transition-colors"
              >
                <td className="py-4 px-6">
                  <div className="flex items-center gap-3">
                    <div className="p-2 rounded-lg bg-blue-50 text-blue-600">
                      <Book size={16} />
                    </div>
                    <span className="text-sm font-bold text-slate-900">
                      {d.name}
                    </span>
                  </div>
                </td>
                <td className="py-4 px-6">
                  <span className="text-sm text-slate-600 bg-slate-100 px-2 py-1 rounded">
                    {getDeptName(d.departmentId)}
                  </span>
                </td>
                <td className="py-4 px-6 text-right">
                  <div className="flex items-center justify-end gap-2 opacity-100 lg:opacity-0 lg:group-hover:opacity-100 transition-opacity">
                    {" "}
                    <button
                      onClick={() => openModal(d)}
                      className="p-1.5 rounded text-slate-400 hover:text-primary"
                    >
                      <Edit2 size={18} />
                    </button>
                    <button
                      onClick={() => handleDelete(d.id)}
                      className="p-1.5 rounded text-slate-400 hover:text-red-600"
                    >
                      <Trash2 size={18} />
                    </button>
                  </div>
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
