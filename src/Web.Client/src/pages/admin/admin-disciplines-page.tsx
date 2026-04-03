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
  AdminTableTextBadge,
} from "@/components/admin/admin-shared";
import { Button } from "@/components/ui/button";
import { TableCell } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { toast } from "sonner";
import { Label } from "@/components/ui/label";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

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

  useAdminPageConfig({
    title: "Справочники",
    subtitle: "Дисциплины",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

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
    try {
      await dictionariesApi.deleteDiscipline(id);
      loadData();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка удаления"));
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await dictionariesApi.restoreDiscipline(id);
      loadData();
      toast.success("Дисциплина успешно восстановлена.", {
        style: { color: "green" },
      });
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка восстановления"));
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
      toast.error("Ошибка");
    }
  };

  return (
    <>
      <AdminTable
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Поиск предмета..."
        data={filteredDisciplines}
        columns={[
          { header: "Название" },
          { header: "Кафедра" },
          { header: "", className: "text-right w-24" },
        ]}
        renderRow={(d) => (
          <AdminTableRow key={d.id} isDeleted={d.isDeleted}>
            <TableCell className="align-top">
              <AdminTableIconCell
                icon={<Book size={14} />}
                iconColorClass="bg-chart-1/15 text-chart-1"
                title={d.name}
                isDeleted={d.isDeleted}
              />
            </TableCell>

            <TableCell className="align-top">
              <AdminTableTextBadge text={getDeptName(d.departmentId)} />
            </TableCell>

            <TableCell className="align-top text-right">
              <AdminTableActions
                isDeleted={d.isDeleted}
                onEdit={() => openModal(d)}
                onDelete={() => handleDelete(d.id)}
                onRestore={() => handleRestore(d.id)}
                deleteDescription={`Вы уверены, что хотите удалить дисциплину "${d.name}"?`}
              />
            </TableCell>
          </AdminTableRow>
        )}
      />

      <AdminModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingId ? "Редактирование" : "Новая дисциплина"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>Название</Label>
          <Input value={newName} onChange={(e) => setNewName(e.target.value)} />
        </div>
        <div className="space-y-2">
          <Label>Кафедра</Label>
          <Select
            value={selectedDept}
            onValueChange={(value) => setSelectedDept(value)}
          >
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Выберите..." />
            </SelectTrigger>
            <SelectContent position="popper">
              <SelectGroup>
                {departments
                  .filter((d) => !d.isDeleted)
                  .map((d) => (
                    <SelectItem key={d.id} value={d.id}>
                      {d.name}
                    </SelectItem>
                  ))}
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
      </AdminModal>
    </>
  );
};
