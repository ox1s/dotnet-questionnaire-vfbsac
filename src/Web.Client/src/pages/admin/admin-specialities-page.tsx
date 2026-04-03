import React, { useEffect, useState } from "react";
import {
  dictionariesApi,
  getApiErrorMessage,
  type DictionaryItem,
} from "../../api";
import { Plus, Book } from "lucide-react";
import {
  AdminModal,
  AdminTable,
  AdminTableActions,
  AdminTableIconCell,
  AdminTableRow,
} from "@/components/admin/admin-shared";
import { Button } from "@/components/ui/button";
import { TableCell } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";

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

  useAdminPageConfig({
    title: "Справочники",
    subtitle: "Специальности",
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
      toast.error(getApiErrorMessage(e, "Ошибка удаления"));
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await dictionariesApi.restoreSpeciality(id);
      loadData();
      toast.success("Специальность успешно восстановлена.", {
        style: { color: "green" },
      });
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка восстановления"));
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
      toast.error(getApiErrorMessage(e, "Ошибка сохранения"));
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
          { header: "Действия", className: "text-right w-24" },
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
