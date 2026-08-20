import { useState } from "react";
import { dictionariesApi, type DictionaryItem } from "../../api";
import { Plus, Layers3 } from "lucide-react";
import {
  AdminModal,
  AdminTable,
  AdminTableActions,
  AdminTableIconCell,
  AdminTableRow,
  AdminTableTextBadge,
} from "@/components/admin/admin-shared";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { TableCell } from "@/components/ui/table";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { useDictionaryCrud } from "@/hooks/use-dictionary-crud";

type SpecializationItem = DictionaryItem & {
  specialityId?: string;
};

export const AdminSpecializationsPage = () => {
  const [selectedSpeciality, setSelectedSpeciality] = useState("");

  const {
    related: specialities,
    filteredItems: filteredSpecializations,
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
  } = useDictionaryCrud<SpecializationItem, DictionaryItem>({
    fetch: async () => {
      const [specializationsRes, specialitiesRes] = await Promise.all([
        dictionariesApi.getSpecializations(),
        dictionariesApi.getSpecialities(),
      ]);

      return {
        items: specializationsRes.data as SpecializationItem[],
        related: specialitiesRes.data,
      };
    },
    searchText: (specialization) => specialization.name,
    formName: (specialization) => specialization.name,
    fillForm: (specialization) =>
      setSelectedSpeciality(
        specialization?.specialityId || specialization?.departmentId || "",
      ),
    submit: async (id) => {
      if (id) {
        await dictionariesApi.updateSpecialization(
          id,
          name,
          selectedSpeciality,
        );
      } else {
        await dictionariesApi.createSpecialization(name, selectedSpeciality);
      }
    },
    remove: dictionariesApi.deleteSpecialization,
    restore: dictionariesApi.restoreSpecialization,
    restoreSuccessMessage: "Специализация успешно восстановлена.",
  });

  useAdminPageConfig({
    title: "Справочники",
    subtitle: "Специализации",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} /> Добавить
      </Button>
    ),
  });

  const getSpecialityName = (specialityId?: string) =>
    specialities.find((speciality) => speciality.id === specialityId)?.name ||
    "-";

  return (
    <>
      <AdminTable
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder="Поиск специализации..."
        data={filteredSpecializations}
        columns={[
          { header: "Название" },
          { header: "Специальность" },
          { header: "Действия", className: "text-right w-32" },
        ]}
        renderRow={(specialization) => (
          <AdminTableRow
            key={specialization.id}
            isDeleted={specialization.isDeleted}
          >
            <TableCell className="align-top">
              <AdminTableIconCell
                icon={<Layers3 size={14} />}
                iconColorClass="bg-chart-3/15 text-chart-3"
                title={specialization.name}
                isDeleted={specialization.isDeleted}
              />
            </TableCell>

            <TableCell className="align-top">
              <AdminTableTextBadge
                text={getSpecialityName(
                  specialization.specialityId || specialization.departmentId,
                )}
              />
            </TableCell>

            <TableCell className="align-top text-right">
              <AdminTableActions
                isDeleted={specialization.isDeleted}
                onEdit={() => openModal(specialization)}
                onDelete={() => handleDelete(specialization)}
                onRestore={() => handleRestore(specialization)}
                deleteDescription={`Вы уверены, что хотите удалить специализацию "${specialization.name}"?`}
              />
            </TableCell>
          </AdminTableRow>
        )}
      />

      <AdminModal
        isOpen={isFormOpen}
        onClose={closeModal}
        title={editingId ? "Редактирование" : "Новая специализация"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>Название</Label>
          <Input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Введите название специализации..."
          />
        </div>
        <div className="space-y-2">
          <Label>Специальность</Label>
          <Select
            value={selectedSpeciality}
            onValueChange={(value) => setSelectedSpeciality(value)}
          >
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Выберите..." />
            </SelectTrigger>
            <SelectContent position="popper">
              <SelectGroup>
                {specialities
                  .filter((speciality) => !speciality.isDeleted)
                  .map((speciality) => (
                    <SelectItem key={speciality.id} value={speciality.id}>
                      {speciality.name}
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
