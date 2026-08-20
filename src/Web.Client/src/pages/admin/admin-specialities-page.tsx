import { dictionariesApi, type DictionaryItem } from "../../api";
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
import { useDictionaryCrud } from "@/hooks/use-dictionary-crud";
import { Label } from "@/components/ui/label";

export const AdminSpecialitiesPage = () => {
  const {
    filteredItems: filteredSpecialities,
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
  } = useDictionaryCrud<DictionaryItem>({
    fetch: async () => ({
      items: (await dictionariesApi.getSpecialities()).data,
    }),
    searchText: (speciality) => speciality.name,
    formName: (speciality) => speciality.name,
    submit: async (id) => {
      if (id) {
        await dictionariesApi.updateSpeciality(id, name);
      } else {
        await dictionariesApi.createSpeciality(name);
      }
    },
    remove: dictionariesApi.deleteSpeciality,
    restore: dictionariesApi.restoreSpeciality,
    confirmDelete: () => "Удалить специальность?",
    restoreSuccessMessage: "Специальность успешно восстановлена.",
  });

  useAdminPageConfig({
    title: "Справочники",
    subtitle: "Специальности",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} /> Добавить
      </Button>
    ),
  });

  return (
    <>
      <AdminTable
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Поиск специальности..."
        data={filteredSpecialities}
        columns={[
          { header: "Название" },
          { header: "Действия", className: "text-right w-32" },
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
                onDelete={() => handleDelete(speciality)}
                onRestore={() => handleRestore(speciality)}
                deleteDescription={`Вы уверены, что хотите удалить дисциплину "${speciality.name}"?`}
              />
            </TableCell>
          </AdminTableRow>
        )}
      />
      <AdminModal
        isOpen={isFormOpen}
        onClose={closeModal}
        title={editingId ? "Редактирование" : "Новая специальность"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>Название</Label>
          <Input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Введите название специальности..."
          />
        </div>
      </AdminModal>
    </>
  );
};
