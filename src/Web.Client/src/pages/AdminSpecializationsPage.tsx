import React, { useEffect, useState } from "react";
import {
  dictionariesApi,
  getApiErrorMessage,
  type DictionaryItem,
} from "../api";
import { Plus, Layers3 } from "lucide-react";
import {
  AdminModal,
  AdminTable,
  AdminTableActions,
  AdminTableIconCell,
  AdminTableRow,
  AdminTableTextBadge,
} from "@/components/AdminShared";
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
import { toast } from "sonner";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";

type SpecializationItem = DictionaryItem & {
  specialityId?: string;
};

export const AdminSpecializationsPage = () => {
  const [specializations, setSpecializations] = useState<SpecializationItem[]>(
    [],
  );
  const [specialities, setSpecialities] = useState<DictionaryItem[]>([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [newName, setNewName] = useState("");
  const [selectedSpeciality, setSelectedSpeciality] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);

  const loadData = async () => {
    try {
      const [specializationsRes, specialitiesRes] = await Promise.all([
        dictionariesApi.getSpecializations(),
        dictionariesApi.getSpecialities(),
      ]);

      setSpecializations(specializationsRes.data as SpecializationItem[]);
      setSpecialities(specialitiesRes.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  useAdminPageConfig({
    title: "Специализации",
    subtitle: "Управление списком специализаций учебного заведения.",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  const filteredSpecializations = specializations.filter((specialization) =>
    specialization.name.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  const getSpecialityName = (specialityId?: string) =>
    specialities.find((speciality) => speciality.id === specialityId)?.name ||
    "-";

  const openModal = (specialization?: SpecializationItem) => {
    if (specialization) {
      setEditingId(specialization.id);
      setNewName(specialization.name);
      setSelectedSpeciality(
        specialization.specialityId || specialization.departmentId || "",
      );
    } else {
      setEditingId(null);
      setNewName("");
      setSelectedSpeciality("");
    }

    setIsFormOpen(true);
  };

  const handleDelete = async (id: string) => {
    try {
      await dictionariesApi.deleteSpecialization(id);
      loadData();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка удаления"));
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await dictionariesApi.restoreSpecialization(id);
      loadData();
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Ошибка восстановления"));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingId) {
        await dictionariesApi.updateSpecialization(
          editingId,
          newName,
          selectedSpeciality,
        );
      } else {
        await dictionariesApi.createSpecialization(newName, selectedSpeciality);
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
        searchPlaceholder="Поиск специализации..."
        data={filteredSpecializations}
        columns={[
          { header: "Название" },
          { header: "Специальность" },
          { header: "", className: "text-right w-24" },
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
                onDelete={() => handleDelete(specialization.id)}
                onRestore={() => handleRestore(specialization.id)}
                deleteDescription={`Вы уверены, что хотите удалить специализацию "${specialization.name}"?`}
              />
            </TableCell>
          </AdminTableRow>
        )}
      />

      <AdminModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        title={editingId ? "Редактирование" : "Новая специализация"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>Название</Label>
          <Input
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
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
