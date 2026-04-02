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
  AdminModal,
  AdminTable,
  AdminTableActions,
  AdminTableIconCell,
  AdminTableRow,
  AdminTableTextBadge,
} from "@/components/AdminShared";
import { Button } from "@/components/ui/button";
import { TableCell, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { Label } from "@/components/ui/label";

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

  useAdminPageConfig({
    title: "Специальности",
    subtitle: "Управление перечнем образовательных специальностей.",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} /> Добавить
      </Button>
    ),
  });

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
    <>
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
      <AdminModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingId ? "Редактирование" : "Новая специальность"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>Название</Label>
          <Input
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            placeholder="Введите название специальности..."
          />
        </div>
      </AdminModal>
    </>
  );
};
