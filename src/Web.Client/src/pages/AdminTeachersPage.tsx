import React, { useEffect, useState } from "react";
import { dictionariesApi, getApiErrorMessage, type TeacherItem } from "../api";

import { Plus, Search, Edit2, Trash2, RotateCcw, Book } from "lucide-react";
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

export const AdminTeachersPage = () => {
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const [newName, setNewName] = useState("");
  const [selectedTeacher, setSelectedTeacher] = useState("");
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
  useAdminPageConfig({
    title: "Преподаватели",
    subtitle: "Управление списком преподавателейц учебного заведения.",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  const filteredTeachers = teachers.filter((t) =>
    t.fullName.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  const getTeacherName = (teacherId?: string) =>
    teachers.find((t) => t.id === teacherId)?.fullName || "-";

  const openModal = (t?: TeacherItem) => {
    if (t) {
      setEditingId(t.id);
      setNewName(t.fullName);
      setSelectedTeacher(t.id || "");
    } else {
      setEditingId(null);
      setNewName("");
      setSelectedTeacher("");
    }
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
    <>
      <AdminTable
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Поиск преподавателя..."
        data={filteredTeachers}
        columns={[
          { header: "ФИО" },
          { header: "", className: "text-right w-24" },
        ]}
        renderRow={(teacher) => (
          <AdminTableRow key={teacher.id} isDeleted={teacher.isDeleted}>
            <TableCell className="align-top">
              <AdminTableIconCell
                textIcon={teacher.fullName.substring(0, 1)}
                iconColorClass="bg-chart-2/15 text-chart-2"
                title={
                  teacher.fullName.length > 20
                    ? truncateFirstWord(teacher.fullName)
                    : teacher.fullName
                }
                isDeleted={teacher.isDeleted}
              />
            </TableCell>

            <TableCell className="align-top text-right">
              <AdminTableActions
                isDeleted={teacher.isDeleted}
                onEdit={() => openModal(teacher)}
                onDelete={() => handleDelete(teacher.id)}
                onRestore={() => handleRestore(teacher.id)}
                deleteDescription={`Вы уверены, что хотите удалить преподавателя "${teacher.fullName}"?`}
              />
            </TableCell>
          </AdminTableRow>
        )}
      />

      <AdminModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingId ? "Редактирование" : "Новый преподаватель"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>ФИО</Label>
          <Input
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            placeholder="Введите фио..."
          />
        </div>
      </AdminModal>
    </>
  );
};
