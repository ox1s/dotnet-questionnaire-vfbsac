import React, { useEffect, useState } from "react";
import {
  dictionariesApi,
  getApiErrorMessage,
  type TeacherItem,
} from "../api";
import { AdminLayout } from "../layouts/AdminLayout";
import { Plus, Search, Edit2, Trash2, RotateCcw } from "lucide-react";

export const AdminTeachersPage = () => {
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const [newName, setNewName] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);

  const loadData = async () => {
    try {
      const teachersRes = await dictionariesApi.getTeachers();
      setTeachers(teachersRes.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const filteredTeachers = teachers.filter((t) =>
    t.fullName.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  const openCreateModal = () => {
    setEditingId(null);
    setNewName("");
    setIsFormOpen(true);
  };

  const openEditModal = (t: TeacherItem) => {
    setEditingId(t.id);
    setNewName(t.fullName);
    setIsFormOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Вы уверены, что хотите удалить этого преподавателя?"))
      return;
    try {
      await dictionariesApi.deleteTeacher(id);
      loadData();
    } catch (e) {
      alert(getApiErrorMessage(e, "Ошибка удаления"));
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await dictionariesApi.restoreTeacher(id);
      loadData();
    } catch (e) {
      alert(getApiErrorMessage(e, "Ошибка восстановления"));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) await dictionariesApi.updateTeacher(editingId, newName);
      else await dictionariesApi.createTeacher(newName);

      setIsFormOpen(false);
      loadData();
    } catch (e) {
      alert(getApiErrorMessage(e, "Ошибка сохранения"));
    }
  };

  const truncateFirstWord = (fullName: string, maxLen: number = 10) => {
    const words = fullName.split(" ");
    if (words.length > 0 && words[0].length > maxLen) {
      words[0] = words[0].substring(0, maxLen) + "...";
    }
    return words.join(" ");
  };

  return (
    <AdminLayout
      title="Преподаватели"
      subtitle="Управление списком преподавателей."
      actions={
        <button
          onClick={openCreateModal}
          className="flex items-center gap-2 px-5 py-2.5 bg-slate-800 text-white rounded-xl hover:bg-slate-900 transition-all text-sm font-bold shadow-lg shadow-slate-800/20 active:scale-95"
        >
          <Plus size={18} />
          Добавить
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
            className="w-full pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all placeholder:text-slate-400"
            placeholder="Поиск по ФИО..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50/50 border-b border-slate-200">
              <th className="py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase tracking-wider">
                ФИО
              </th>
              <th className="py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase tracking-wider text-right w-12 md:w-24"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filteredTeachers.map((t) => (
              <tr
                key={t.id}
                className={`group transition-colors ${
                  t.isDeleted ? "bg-slate-50/70 text-slate-400" : "hover:bg-slate-50"
                }`}
              >
                <td className="py-3 px-3 md:py-4 md:px-6 align-top">
                  <div className="flex items-start gap-2 md:gap-3">
                    <div className="h-7 w-7 md:h-9 md:w-9 shrink-0 rounded-full bg-primary/10 text-primary flex items-center justify-center text-xs font-bold mt-0.5">
                      {t.fullName.substring(0, 1)}
                    </div>

                    <span
                      className={`text-xs md:text-sm font-bold leading-snug pt-1 ${
                        t.isDeleted ? "text-slate-500" : "text-slate-900"
                      }`}
                      title={t.fullName}
                    >
                      {truncateFirstWord(t.fullName, 10)}
                    </span>
                    {t.isDeleted && (
                      <span className="inline-flex items-center rounded-full bg-slate-200 px-2 py-1 text-[10px] font-bold uppercase tracking-wide text-slate-600">
                        Удалено
                      </span>
                    )}
                  </div>
                </td>
                <td className="py-3 px-3 md:py-4 md:px-6 align-top text-right">
                  <div className="flex flex-col sm:flex-row items-end justify-end gap-1 opacity-100 lg:opacity-0 lg:group-hover:opacity-100 transition-opacity">
                    {t.isDeleted ? (
                      <button
                        onClick={() => handleRestore(t.id)}
                        className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 transition-colors"
                      >
                        <RotateCcw
                          size={16}
                          className="md:w-[18px] md:h-[18px]"
                        />
                      </button>
                    ) : (
                      <>
                        <button
                          onClick={() => openEditModal(t)}
                          className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-primary hover:bg-primary/10 transition-colors"
                        >
                          <Edit2
                            size={16}
                            className="md:w-[18px] md:h-[18px]"
                          />
                        </button>
                        <button
                          onClick={() => handleDelete(t.id)}
                          className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-accent hover:bg-accent/10 transition-colors"
                        >
                          <Trash2
                            size={16}
                            className="md:w-[18px] md:h-[18px]"
                          />
                        </button>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {filteredTeachers.length === 0 && (
              <tr>
                <td
                  colSpan={2}
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
          <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6 animate-in fade-in zoom-in-95 duration-200">
            <h3 className="text-lg font-bold text-slate-900 mb-4">
              {editingId ? "Редактирование" : "Новый преподаватель"}
            </h3>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                  ФИО
                </label>
                <input
                  className="w-full p-3 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none"
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  placeholder="Например: Иванов И.И."
                />
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => setIsFormOpen(false)}
                  className="flex-1 py-2.5 rounded-lg border border-slate-200 text-slate-600 font-bold text-sm hover:bg-slate-50"
                >
                  Отмена
                </button>
                <button
                  type="submit"
                  className="flex-1 py-2.5 rounded-lg bg-slate-800 text-white font-bold text-sm hover:bg-primary-hover shadow-lg shadow-primary/20"
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
