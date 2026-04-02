import React, { useEffect, useState } from "react";
import {
  dictionariesApi,
  getApiErrorMessage,
  type DictionaryItem,
} from "../../api";
import { Plus, Building2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { TableCell } from "@/components/ui/table";
import { toast } from "sonner";
import {
  AdminModal,
  AdminTable,
  AdminTableActions,
  AdminTableIconCell,
  AdminTableRow,
} from "@/components/admin/admin-shared";
import { Label } from "@/components/ui/label";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";

export const AdminDepartmentsPage = () => {
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [newName, setNewName] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);

  const loadData = async () => {
    try {
      const res = await dictionariesApi.getDepartments();
      setDepartments(res.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  useAdminPageConfig({
    title: "Кафедры",
    subtitle: "Управление списком кафедр учебного заведения.",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  const openModal = (d?: DictionaryItem) => {
    if (d) {
      setEditingId(d.id);
      setNewName(d.name);
    } else {
      setEditingId(null);
      setNewName("");
    }
    setIsFormOpen(true);
  };

  const handleDelete = async (id: string) => {
    try {
      await dictionariesApi.deleteDepartment(id);
      loadData();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка удаления"));
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await dictionariesApi.restoreDepartment(id);
      loadData();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка восстановления"));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) await dictionariesApi.updateDepartment(editingId, newName);
      else await dictionariesApi.createDepartment(newName);
      setIsFormOpen(false);
      loadData();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка сохранения"));
    }
  };

  return (
    <>
      <AdminTable
        data={departments}
        columns={[
          { header: "Название / Аббревиатура" },
          { header: "", className: "text-right w-24" },
        ]}
        renderRow={(d) => (
          <AdminTableRow key={d.id} isDeleted={d.isDeleted}>
            <TableCell className="align-top">
              <AdminTableIconCell
                icon={<Building2 size={14} />}
                iconColorClass="bg-chart-4/15 text-chart-4"
                title={d.name}
                isDeleted={d.isDeleted}
              />
            </TableCell>
            <TableCell className="align-top text-right">
              <AdminTableActions
                isDeleted={d.isDeleted}
                onEdit={() => openModal(d)}
                onDelete={() => handleDelete(d.id)}
                onRestore={() => handleRestore(d.id)}
                deleteDescription={`Вы уверены, что хотите удалить кафедру "${d.name}"?`}
              />
            </TableCell>
          </AdminTableRow>
        )}
      />

      <AdminModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingId ? "Редактирование" : "Новая кафедра"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>Название</Label>
          <Input
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            placeholder="Введите название кафедры..."
          />
        </div>
      </AdminModal>
    </>
  );
};
