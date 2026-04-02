import React, { useEffect, useState } from "react";
import {
  dictionariesApi,
  getApiErrorMessage,
  type DictionaryItem,
} from "../api";
import {
  Plus,
  Search,
  Edit2,
  Trash2,
  GraduationCap,
  RotateCcw,
  Book,
} from "lucide-react";
import {
  AdminLayout,
  AdminTable,
  AdminTableActions,
  AdminTableIconCell,
  AdminTableRow,
  AdminTableTextBadge,
} from "@/components/AdminShared";
import { Button } from "@/components/ui/button";
import { TableCell, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";

export const AdminSpecialitiesPage = () => {
  const [specialities, setSpecialities] = useState<DictionaryItem[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [newName, setNewName] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);

  const loadData = async () => {
    try {
      const res = await dictionariesApi.getSpecialities();
      setSpecialities(res.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const filteredSpecialities = specialities.filter((speciality) =>
    speciality.name.toLowerCase().includes(searchQuery.toLowerCase()),
  );
  const getSpecialityName = (id?: string) =>
    specialities.find((s) => s.id === id)?.name || "-";

  const openModal = (speciality?: DictionaryItem) => {
    if (speciality) {
      setEditingId(speciality.id);
      setNewName(speciality.name);
    } else {
      setEditingId(null);
      setNewName("");
    }

    setIsFormOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Удалить специальность?")) return;

    try {
      await dictionariesApi.deleteSpeciality(id);
      loadData();
    } catch (e) {
      alert(getApiErrorMessage(e, "Ошибка удаления"));
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await dictionariesApi.restoreSpeciality(id);
      loadData();
    } catch (e) {
      alert(getApiErrorMessage(e, "Ошибка восстановления"));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingId) {
        await dictionariesApi.updateSpeciality(editingId, newName);
      } else {
        await dictionariesApi.createSpeciality(newName);
      }

      setIsFormOpen(false);
      loadData();
    } catch (e) {
      alert("Ошибка");
    }
  };

  return (
    <AdminLayout
      title="Специальности"
      subtitle="Управление перечнем образовательных специальностей."
      actions={
        <Button onClick={() => openModal()}>
          <Plus size={18} /> Добавить
        </Button>
      }
    >
      <AdminTable
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Поиск специальности..."
        data={filteredSpecialities}
        columns={[
          { header: "Название" },
          { header: "", className: "text-right w-24" },
        ]}
        renderRow={(speciality) => (
          <AdminTableRow key={speciality.id} isDeleted={speciality.isDeleted}>
            <TableCell className="align-top">
              <AdminTableIconCell
                icon={<Book size={14} />}
                iconColorClass="bg-chart-1/15 text-chart-1"
                title={speciality.name}
                isDeleted={speciality.isDeleted}
              />
            </TableCell>

            <TableCell className="align-top text-right">
              <AdminTableActions
                isDeleted={speciality.isDeleted}
                onEdit={() => openModal(speciality)}
                onDelete={() => handleDelete(speciality.id)}
                onRestore={() => handleRestore(speciality.id)}
                deleteDescription={`Вы уверены, что хотите удалить дисциплину "${speciality.name}"?`}
              />
            </TableCell>
          </AdminTableRow>
        )}
      />

      {isFormOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div
            className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm"
            onClick={() => setIsFormOpen(false)}
          ></div>
          <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6">
            <h3 className="text-lg font-bold text-slate-900 mb-4">
              {editingId ? "Редактирование" : "Новая специальность"}
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
                  placeholder="Например: Программное обеспечение информационных технологий"
                />
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
