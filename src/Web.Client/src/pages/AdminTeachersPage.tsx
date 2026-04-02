import React, { useEffect, useState } from "react";
import { dictionariesApi, getApiErrorMessage, type TeacherItem } from "../api";

import { Plus, Search, Edit2, Trash2, RotateCcw, Book } from "lucide-react";
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
    <AdminLayout
      title="Преподаватели"
      subtitle="Управление списком преподавателей."
      actions={
        <Button onClick={() => openModal()}>
          <Plus size={18} /> Добавить
        </Button>
      }
    >
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

      {/* TODO: shadcn Dialog) */}
      {isFormOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div
            className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm"
            onClick={() => setIsFormOpen(false)}
          ></div>
          <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6">
            <h3 className="text-lg font-bold text-slate-900 mb-4">
              {editingId ? "Редактирование" : "Новый преподаватель"}
            </h3>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-500 uppercase mb-1">
                  ФИО
                </label>
                <Input
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  className="bg-slate-50"
                />
              </div>

              <div className="flex gap-3 pt-4">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setIsFormOpen(false)}
                  className="flex-1"
                >
                  Отмена
                </Button>
                <Button
                  type="submit"
                  className="flex-1 bg-slate-800 hover:bg-slate-900"
                >
                  Сохранить
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </AdminLayout>
  );
};
