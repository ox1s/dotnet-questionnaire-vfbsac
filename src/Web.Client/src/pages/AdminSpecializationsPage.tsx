import React, { useEffect, useState } from "react";
import { dictionariesApi, type DictionaryItem } from "../api";
import { AdminLayout } from "../layouts/AdminLayout";
import { Plus, Search, Edit2, Trash2, Layers3 } from "lucide-react";

type SpecializationItem = DictionaryItem & {
  specialityId?: string;
};

export const AdminSpecializationsPage = () => {
  const [specializations, setSpecializations] = useState<SpecializationItem[]>(
    [],
  );
  const [specialities, setSpecialities] = useState<DictionaryItem[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [newName, setNewName] = useState("");
  const [selectedSpeciality, setSelectedSpeciality] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);

  const loadData = async () => {
    try {
      const [specializationsRes, specialitiesRes] = await Promise.all([
        dictionariesApi.getSpecializations(),
        dictionariesApi.getSpecialities(),
      ]);

      setSpecializations(specializationsRes.data as SpecializationItem[]);
      setSpecialities(specialitiesRes.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const filteredSpecializations = specializations.filter((specialization) =>
    specialization.name.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  const getSpecialityName = (specialityId?: string) =>
    specialities.find((speciality) => speciality.id === specialityId)?.name ||
    "-";

  const openModal = (specialization?: SpecializationItem) => {
    if (specialization) {
      setEditingId(specialization.id);
      setNewName(specialization.name);
      setSelectedSpeciality(
        specialization.specialityId || specialization.departmentId || "",
      );
    } else {
      setEditingId(null);
      setNewName("");
      setSelectedSpeciality("");
    }

    setIsFormOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Удалить специализацию?")) return;

    try {
      await dictionariesApi.deleteSpecialization(id);
      loadData();
    } catch (e) {
      alert("Ошибка");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingId) {
        await dictionariesApi.updateSpecialization(
          editingId,
          newName,
          selectedSpeciality,
        );
      } else {
        await dictionariesApi.createSpecialization(newName, selectedSpeciality);
      }

      setIsFormOpen(false);
      loadData();
    } catch (e) {
      alert("Ошибка");
    }
  };

  return (
    <AdminLayout
      title="Специализации"
      subtitle="Управление специализациями и их привязкой к специальностям."
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
            placeholder="Поиск специализации..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50/50 border-b border-slate-200">
              <th className="py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase">
                Название
              </th>
              <th className="py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase">
                Специальность
              </th>
              <th className="py-3 px-3 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase text-right w-12 md:w-24"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filteredSpecializations.map((specialization) => (
              <tr
                key={specialization.id}
                className="group hover:bg-slate-50 transition-colors"
              >
                <td className="py-3 px-3 md:py-4 md:px-6 align-top">
                  <div className="flex items-start gap-2 md:gap-3">
                    <div className="p-1.5 md:p-2 rounded-lg bg-violet-50 text-violet-600 shrink-0 mt-0.5">
                      <Layers3 size={14} className="md:w-[16px] md:h-[16px]" />
                    </div>
                    <span className="text-xs md:text-sm font-bold text-slate-900 line-clamp-3 leading-snug">
                      {specialization.name}
                    </span>
                  </div>
                </td>
                <td className="py-3 px-3 md:py-4 md:px-6 align-top">
                  <span className="inline-block px-2 py-1 rounded text-[10px] md:text-xs font-medium bg-slate-100 text-slate-600 border border-slate-200 line-clamp-3 leading-tight">
                    {getSpecialityName(
                      specialization.specialityId || specialization.departmentId,
                    )}
                  </span>
                </td>
                <td className="py-3 px-3 md:py-4 md:px-6 align-top text-right">
                  <div className="flex flex-col sm:flex-row items-end justify-end gap-1 opacity-100 lg:opacity-0 lg:group-hover:opacity-100 transition-opacity">
                    <button
                      onClick={() => openModal(specialization)}
                      className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-primary hover:bg-primary/10 transition-colors"
                    >
                      <Edit2 size={16} className="md:w-[18px] md:h-[18px]" />
                    </button>
                    <button
                      onClick={() => handleDelete(specialization.id)}
                      className="p-1.5 md:p-2 rounded-md text-slate-400 hover:text-accent hover:bg-accent/10 transition-colors"
                    >
                      <Trash2 size={16} className="md:w-[18px] md:h-[18px]" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {filteredSpecializations.length === 0 && (
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
              {editingId ? "Редактирование" : "Новая специализация"}
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
                  placeholder="Например: Разработка и сопровождение программного обеспечения"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                  Специальность
                </label>
                <select
                  className="w-full p-3 bg-slate-50 border border-slate-200 rounded-lg text-sm"
                  value={selectedSpeciality}
                  onChange={(e) => setSelectedSpeciality(e.target.value)}
                >
                  <option value="">Выберите специальность...</option>
                  {specialities.map((speciality) => (
                    <option key={speciality.id} value={speciality.id}>
                      {speciality.name}
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
