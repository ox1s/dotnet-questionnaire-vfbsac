import {dictionariesApi, type DictionaryItem} from "../../api";
import {Plus, Building2} from "lucide-react";
import {Button} from "@/components/ui/button";
import {Input} from "@/components/ui/input";
import {TableCell} from "@/components/ui/table";
import {
    AdminModal,
    AdminTable,
    AdminTableActions,
    AdminTableIconCell,
    AdminTableRow,
} from "@/components/admin/admin-shared";
import {Label} from "@/components/ui/label";
import {useAdminPageConfig} from "@/hooks/use-admin-page-config";
import {useDictionaryCrud} from "@/hooks/use-dictionary-crud";

export const AdminDepartmentsPage = () => {
    const {
        filteredItems: filteredDepartments,
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
            items: (await dictionariesApi.getDepartments()).data,
        }),
        searchText: (d) => d.name,
        formName: (d) => d.name,
        submit: async (id) => {
            if (id) await dictionariesApi.updateDepartment(id, name);
            else await dictionariesApi.createDepartment(name);
        },
        remove: dictionariesApi.deleteDepartment,
        restore: dictionariesApi.restoreDepartment,
        restoreSuccessMessage: "Филиал кафедры успешно восстановлен.",
    });

    useAdminPageConfig({
        title: "Справочники",
        subtitle: "Кафедры",
        actions: (
            <Button onClick={() => openModal()}>
                <Plus size={18} className="mr-2"/> Добавить
            </Button>
        ),
    });

    return (
        <>
            <AdminTable
                data={filteredDepartments}
                searchQuery={searchQuery}
                onSearchChange={setSearchQuery}
                searchPlaceholder="Поиск кафедры..."
                columns={[
                    {header: "Название / Аббревиатура"},
                    {header: "Действия", className: "text-right w-24"},
                ]}
                renderRow={(d) => (
                    <AdminTableRow key={d.id} isDeleted={d.isDeleted}>
                        <TableCell className="align-top">
                            <AdminTableIconCell
                                icon={<Building2 size={14}/>}
                                iconColorClass="bg-chart-4/15 text-chart-4"
                                title={d.name}
                                isDeleted={d.isDeleted}
                            />
                        </TableCell>
                        <TableCell className="align-top text-right">
                            <AdminTableActions
                                isDeleted={d.isDeleted}
                                onEdit={() => openModal(d)}
                                onDelete={() => handleDelete(d)}
                                onRestore={() => handleRestore(d)}
                                deleteDescription={`Вы уверены, что хотите удалить кафедру "${d.name}"?`}
                            />
                        </TableCell>
                    </AdminTableRow>
                )}
            />

            <AdminModal
                isOpen={isFormOpen}
                onClose={closeModal}
                title={editingId ? "Редактирование" : "Новый филиал кафедры"}
                onSubmit={handleSubmit}
            >
                <div className="space-y-2">
                    <Label>Название</Label>
                    <Input
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="Введите название кафедры..."
                    />
                </div>
            </AdminModal>
        </>
    );
};
