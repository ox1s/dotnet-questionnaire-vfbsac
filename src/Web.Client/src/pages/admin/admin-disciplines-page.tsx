import { useState } from "react";
import { dictionariesApi, type DictionaryItem } from "../../api";
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
import { Label } from "@/components/ui/label";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { useDictionaryCrud } from "@/hooks/use-dictionary-crud";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

export const AdminDisciplinesPage = () => {
  const [selectedDept, setSelectedDept] = useState("");

  const {
    related: departments,
    filteredItems: filteredDisciplines,
    searchQuery,
    setSearchQuery,
    isFormOpen,
    closeModal,
    openModal,
    editingId,
    name,
    setName,
    handleDelete,
    handleRestore,
    handleSubmit,
  } = useDictionaryCrud<DictionaryItem, DictionaryItem>({
    fetch: async () => {
      const [discRes, deptsRes] = await Promise.all([
        dictionariesApi.getDisciplines(),
        dictionariesApi.getDepartments(),
      ]);
      return { items: discRes.data, related: deptsRes.data };
    },
    searchText: (d) => d.name,
    formName: (d) => d.name,
    fillForm: (d) => setSelectedDept(d?.departmentId || ""),
    submit: async (id) => {
      if (id) await dictionariesApi.updateDiscipline(id, name, selectedDept);
      else await dictionariesApi.createDiscipline(name, selectedDept);
    },
    remove: dictionariesApi.deleteDiscipline,
    restore: dictionariesApi.restoreDiscipline,
    restoreSuccessMessage: "Предмет успешно восстановлен.",
    saveErrorMessage: "Ошибка",
  });

  useAdminPageConfig({
    title: "Справочники",
    subtitle: "Дисциплины",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  const getDeptName = (deptId?: string) =>
    departments.find((d) => d.id === deptId)?.name || "-";

  return (
    <>
      <AdminTable
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Поиск предмета..."
        data={filteredDisciplines}
        columns={[
          { header: "Название" },
          { header: "Филиал кафедры" },
          { header: "Действия", className: "text-right w-32" },
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
                onDelete={() => handleDelete(d)}
                onRestore={() => handleRestore(d)}
                deleteDescription={`Вы уверены, что хотите удалить дисциплину "${d.name}"?`}
              />
            </TableCell>
          </AdminTableRow>
        )}
      />

      <AdminModal
        isOpen={isFormOpen}
        onClose={closeModal}
        title={editingId ? "Редактирование" : "Новый предмет"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>Название</Label>
          <Input value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="space-y-2">
          <Label>Филиал кафедры</Label>
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
