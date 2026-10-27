import { useState } from "react";
import {
  dictionariesApi,
  usersApi,
  type DictionaryItem,
  type TeacherItem,
  type TeacherUser,
} from "@/api.ts";
import { CheckCircle2, KeyRound, Plus } from "lucide-react";
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
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { useDictionaryCrud } from "@/hooks/use-dictionary-crud";
import { Label } from "@/components/ui/label";
import { TeacherAccessModal } from "@/components/admin/teacher-access-modal";
import {
  CredentialsBanner,
  type IssuedCredentials,
} from "@/components/admin/credentials-banner";

export const AdminTeachersPage = () => {
  const [selectedDepartmentIds, setSelectedDepartmentIds] = useState<string[]>(
    [],
  );
  const [teacherUsers, setTeacherUsers] = useState<TeacherUser[]>([]);
  const [accessTeacher, setAccessTeacher] = useState<TeacherItem | null>(null);
  const [issuedCredentials, setIssuedCredentials] =
    useState<IssuedCredentials | null>(null);

  const findTeacherUser = (teacherId: string) =>
    teacherUsers.find((account) => account.teacherId === teacherId) ?? null;

  const {
    related: departments,
    filteredItems: filteredTeachers,
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
    reload,
  } = useDictionaryCrud<TeacherItem, DictionaryItem>({
    fetch: async () => {
      const [teachersRes, departmentsRes, teacherUsersRes] = await Promise.all([
        dictionariesApi.getTeachers(),
        dictionariesApi.getDepartments(),
        usersApi.getTeacherUsers(),
      ]);
      // Logins are reloaded with the list, so deleting a teacher (which also
      // retires their login) or issuing access is reflected straight away.
      setTeacherUsers(teacherUsersRes.data);
      return { items: teachersRes.data, related: departmentsRes.data };
    },
    searchText: (t) => t.fullName,
    formName: (t) => t.fullName,
    fillForm: (t) => setSelectedDepartmentIds(t?.departmentIds ?? []),
    submit: async (id) => {
      if (id)
        await dictionariesApi.updateTeacher(id, name, selectedDepartmentIds);
      else await dictionariesApi.createTeacher(name, selectedDepartmentIds);
    },
    remove: dictionariesApi.deleteTeacher,
    restore: dictionariesApi.restoreTeacher,
    confirmDelete: () => "Вы уверены, что хотите удалить этого преподавателя?",
    restoreSuccessMessage: "Преподаватель успешно восстановлен.",
  });

  useAdminPageConfig({
    title: "Справочники",
    subtitle: "Преподаватели",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  const getDepartmentName = (departmentId?: string) =>
    departments.find((department) => department.id === departmentId)?.name ??
    "Не указана";

  const getDepartmentNames = (departmentIds: string[]) =>
    departmentIds.length > 0
      ? departmentIds.map((id) => getDepartmentName(id)).join(", ")
      : "Не указана";

  const formatTeacherLabel = (teacher: TeacherItem) =>
    teacher.departmentIds.length > 0
      ? `${teacher.fullName} (${getDepartmentNames(teacher.departmentIds)})`
      : teacher.fullName;

  const toggleDepartment = (departmentId: string) => {
    setSelectedDepartmentIds((prev) =>
      prev.includes(departmentId)
        ? prev.filter((id) => id !== departmentId)
        : [...prev, departmentId],
    );
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
        topContent={
          issuedCredentials && (
            <CredentialsBanner
              title="Доступ преподавателя"
              credentials={issuedCredentials}
              onClose={() => setIssuedCredentials(null)}
            />
          )
        }
        columns={[
          { header: "ФИО" },
          { header: "Филиал кафедры" },
          { header: "Логин" },
          { header: "Действия", className: "text-right w-32" },
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
                    : formatTeacherLabel(teacher)
                }
                isDeleted={teacher.isDeleted}
              />
            </TableCell>

            <TableCell className="align-top">
              <AdminTableTextBadge
                text={getDepartmentNames(teacher.departmentIds)}
              />
            </TableCell>

            <TableCell className="align-top">
              {findTeacherUser(teacher.id) ? (
                <span className="font-mono text-sm">
                  {findTeacherUser(teacher.id)?.login}
                </span>
              ) : (
                <span className="text-xs text-muted-foreground">
                  Нет доступа
                </span>
              )}
            </TableCell>

            <TableCell className="align-top text-right">
              <div className="flex items-center justify-end gap-1">
                {!teacher.isDeleted && (
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setAccessTeacher(teacher)}
                    title={
                      findTeacherUser(teacher.id)
                        ? "Сменить пароль"
                        : "Выдать доступ"
                    }
                  >
                    <KeyRound size={16} />
                  </Button>
                )}
                <AdminTableActions
                  isDeleted={teacher.isDeleted}
                  onEdit={() => openModal(teacher)}
                  onDelete={() => handleDelete(teacher)}
                  onRestore={() => handleRestore(teacher)}
                  deleteDescription={`Вы уверены, что хотите удалить преподавателя "${formatTeacherLabel(teacher)}"? Его логин для входа тоже будет удален.`}
                />
              </div>
            </TableCell>
          </AdminTableRow>
        )}
      />

      {accessTeacher && (
        <TeacherAccessModal
          teacher={accessTeacher}
          account={findTeacherUser(accessTeacher.id)}
          onClose={() => setAccessTeacher(null)}
          onSaved={async (credentials) => {
            setAccessTeacher(null);
            setIssuedCredentials(credentials);
            await reload();
          }}
        />
      )}

      <AdminModal
        isOpen={isFormOpen}
        onClose={closeModal}
        title={editingId ? "Редактирование" : "Новый преподаватель"}
        onSubmit={handleSubmit}
      >
        <div className="space-y-2">
          <Label>ФИО</Label>
          <Input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Введите фио..."
          />
        </div>
        <div className="space-y-2">
          <Label>Филиалы кафедры</Label>
          <div className="space-y-2 max-h-48 overflow-y-auto">
            {departments
              .filter((department) => !department.isDeleted)
              .map((department) => {
                const active = selectedDepartmentIds.includes(department.id);
                return (
                  <button
                    key={department.id}
                    type="button"
                    onClick={() => toggleDepartment(department.id)}
                    className={`flex w-full items-center justify-between border px-4 py-2 text-sm font-medium transition-all ${
                      active
                        ? "bg-primary border-primary text-primary-foreground shadow-sm"
                        : "bg-background border-border text-foreground hover:bg-muted"
                    }`}
                  >
                    {department.name}
                    {active && <CheckCircle2 size={16} />}
                  </button>
                );
              })}
          </div>
        </div>
      </AdminModal>
    </>
  );
};
