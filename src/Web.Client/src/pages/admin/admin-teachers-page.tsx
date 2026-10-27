import { useState } from "react";
import { toast } from "sonner";
import {
  dictionariesApi,
  getApiErrorMessage,
  usersApi,
  type DictionaryItem,
  type TeacherItem,
  type TeacherUser,
} from "@/api.ts";
import { ChevronDownIcon, Plus, RefreshCw } from "lucide-react";
import {
  AdminModal,
  AdminTable,
  AdminTableActions,
  AdminTableIconCell,
  AdminTableRow,
  AdminTableTextBadge,
} from "@/components/admin/admin-shared";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { TableCell } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { useDictionaryCrud } from "@/hooks/use-dictionary-crud";
import { Label } from "@/components/ui/label";
import {
  CredentialsBanner,
  type IssuedCredentials,
} from "@/components/admin/credentials-banner";
import { generateSecurePassword, MIN_PASSWORD_LENGTH } from "@/utils/password";

// A teacher and their sign-in access are edited in one dialog, like every
// other participant: the login is optional, so a teacher can be listed before
// they are given access.
export const AdminTeachersPage = () => {
  const [selectedDepartmentIds, setSelectedDepartmentIds] = useState<string[]>(
    [],
  );
  const [teacherUsers, setTeacherUsers] = useState<TeacherUser[]>([]);
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const [issuedCredentials, setIssuedCredentials] =
    useState<IssuedCredentials | null>(null);

  const findTeacherUser = (teacherId: string) =>
    teacherUsers.find((account) => account.teacherId === teacherId) ?? null;

  const issueCredentials = (issuedLogin: string, issuedPassword: string) =>
    setIssuedCredentials({
      login: issuedLogin,
      pass: issuedPassword,
      link: `${window.location.origin}/login`,
    });

  const {
    related: departments,
    filteredItems: filteredTeachers,
    searchQuery,
    setSearchQuery,
    isFormOpen,
    isSubmitting,
    closeModal,
    openModal,
    editingId,
    name,
    setName,
    handleDelete,
    handleRestore,
    handleSubmit,
  } = useDictionaryCrud<TeacherItem, DictionaryItem>({
    fetch: async () => {
      const [teachersRes, departmentsRes, teacherUsersRes] = await Promise.all([
        dictionariesApi.getTeachers(),
        dictionariesApi.getDepartments(),
        usersApi.getTeacherUsers(),
      ]);
      setTeacherUsers(teacherUsersRes.data);
      return { items: teachersRes.data, related: departmentsRes.data };
    },
    searchText: (t) => [t.fullName, findTeacherUser(t.id)?.login ?? ""],
    formName: (t) => t.fullName,
    fillForm: (t) => {
      setSelectedDepartmentIds(t?.departmentIds ?? []);
      setLogin(t ? (findTeacherUser(t.id)?.login ?? "") : "");
      // A new teacher gets a ready password in case a login is entered; an
      // existing one keeps theirs unless a new one is typed in.
      setPassword(t ? "" : generateSecurePassword());
    },
    validate: () => {
      const account = editingId ? findTeacherUser(editingId) : null;
      if (!name.trim()) {
        toast.error("Введите ФИО");
        return false;
      }
      if (account && !login.trim()) {
        toast.error("Логин нельзя удалить");
        return false;
      }
      if (!account && login.trim() && !password.trim()) {
        toast.error("Введите пароль для нового логина");
        return false;
      }
      if (password.trim() && password.trim().length < MIN_PASSWORD_LENGTH) {
        toast.error(
          `Пароль должен содержать не менее ${MIN_PASSWORD_LENGTH} символов`,
        );
        return false;
      }
      return true;
    },
    submit: async (id) => {
      const fullName = name.trim();
      const trimmedLogin = login.trim();
      const trimmedPassword = password.trim();

      if (id) {
        await dictionariesApi.updateTeacher(
          id,
          fullName,
          selectedDepartmentIds,
        );

        const account = findTeacherUser(id);
        if (account) {
          if (
            trimmedLogin !== account.login ||
            fullName !== account.displayName
          ) {
            await usersApi.updateUser(account.id, trimmedLogin, fullName);
          }
          if (trimmedPassword) {
            await usersApi.setPassword(account.id, trimmedPassword);
            issueCredentials(trimmedLogin, trimmedPassword);
          }
        } else if (trimmedLogin) {
          await usersApi.createTeacherUser(id, trimmedLogin, trimmedPassword);
          issueCredentials(trimmedLogin, trimmedPassword);
        }
        return;
      }

      const { data: teacherId } = await dictionariesApi.createTeacher(
        fullName,
        selectedDepartmentIds,
      );
      if (!trimmedLogin) return;

      // The teacher already exists at this point: failing the whole save would
      // keep the dialog open and a retry would create them twice. Report it
      // and let the admin issue access by editing the teacher.
      try {
        await usersApi.createTeacherUser(
          teacherId,
          trimmedLogin,
          trimmedPassword,
        );
        issueCredentials(trimmedLogin, trimmedPassword);
      } catch (e) {
        toast.error(
          getApiErrorMessage(
            e,
            "Преподаватель добавлен, но доступ не выдан: возможно, логин занят.",
          ),
        );
      }
    },
    remove: dictionariesApi.deleteTeacher,
    restore: dictionariesApi.restoreTeacher,
    confirmDelete: () => "Вы уверены, что хотите удалить этого преподавателя?",
    restoreSuccessMessage: "Преподаватель успешно восстановлен.",
    saveErrorMessage:
      "Ошибка. Возможно, логин занят или содержит недопустимые символы.",
  });

  useAdminPageConfig({
    title: "Участники",
    subtitle: "Преподаватели",
    actions: (
      <Button onClick={() => openModal()}>
        <Plus size={18} className="mr-2" /> Добавить
      </Button>
    ),
  });

  const editingAccount = editingId ? findTeacherUser(editingId) : null;

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
        searchPlaceholder="Поиск по ФИО или логину..."
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
          { header: "Действия", className: "text-right w-32" },
        ]}
        renderRow={(teacher) => {
          const account = findTeacherUser(teacher.id);
          return (
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
                  subtitle={
                    account ? (
                      <span className="font-mono">{account.login}</span>
                    ) : (
                      "Нет доступа"
                    )
                  }
                  isDeleted={teacher.isDeleted}
                />
              </TableCell>

              <TableCell className="align-top">
                <AdminTableTextBadge
                  text={getDepartmentNames(teacher.departmentIds)}
                />
              </TableCell>

              <TableCell className="align-top text-right">
                <AdminTableActions
                  isDeleted={teacher.isDeleted}
                  onEdit={() => openModal(teacher)}
                  onDelete={() => handleDelete(teacher)}
                  onRestore={() => handleRestore(teacher)}
                  deleteDescription={`Вы уверены, что хотите удалить преподавателя "${formatTeacherLabel(teacher)}"? Его логин для входа тоже будет удален.`}
                />
              </TableCell>
            </AdminTableRow>
          );
        }}
      />

      <AdminModal
        isOpen={isFormOpen}
        onClose={closeModal}
        title={editingId ? "Редактирование" : "Новый преподаватель"}
        onSubmit={handleSubmit}
        submitText={isSubmitting ? "Сохранение..." : "Сохранить"}
      >
        <div className="space-y-2">
          <Label htmlFor="teacher-full-name">ФИО</Label>
          <Input
            id="teacher-full-name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Введите фио..."
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="teacher-departments">Филиалы кафедры</Label>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button
                id="teacher-departments"
                type="button"
                className="flex h-8 w-full items-center justify-between gap-1.5 border border-input bg-transparent pr-2 pl-2.5 text-xs transition-colors outline-none focus-visible:border-ring focus-visible:ring-1 focus-visible:ring-ring/50 dark:bg-input/30 dark:hover:bg-input/50"
              >
                <span
                  className={`truncate ${selectedDepartmentIds.length > 0 ? "" : "text-muted-foreground"}`}
                >
                  {selectedDepartmentIds.length > 0
                    ? getDepartmentNames(selectedDepartmentIds)
                    : "Выберите филиалы"}
                </span>
                <ChevronDownIcon className="size-4 shrink-0 text-muted-foreground" />
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="max-h-60">
              {departments
                .filter((department) => !department.isDeleted)
                .map((department) => (
                  <DropdownMenuCheckboxItem
                    key={department.id}
                    checked={selectedDepartmentIds.includes(department.id)}
                    onCheckedChange={() => toggleDepartment(department.id)}
                    // Keep the list open so several departments can be picked.
                    onSelect={(e) => e.preventDefault()}
                  >
                    {department.name}
                  </DropdownMenuCheckboxItem>
                ))}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        <div className="space-y-2">
          <Label htmlFor="teacher-login">Логин</Label>
          <Input
            id="teacher-login"
            value={login}
            onChange={(e) => setLogin(e.target.value)}
            placeholder={
              editingAccount
                ? "Например: IVANOVA"
                : "Оставьте пустым, если доступ пока не нужен"
            }
          />
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between gap-3">
            <Label htmlFor="teacher-password">
              {editingAccount ? "Новый пароль" : "Пароль"}
            </Label>
            <Button
              type="button"
              variant="outline"
              onClick={() => setPassword(generateSecurePassword())}
              className="h-8 px-3"
            >
              <RefreshCw size={14} className="mr-2" />
              Сгенерировать
            </Button>
          </div>
          <Input
            id="teacher-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder={
              editingAccount
                ? "Оставьте пустым, чтобы не менять пароль"
                : "Введите или сгенерируйте пароль"
            }
          />
          <p className="text-xs text-muted-foreground">
            {editingAccount
              ? "Если поле пустое, пароль преподавателя останется прежним."
              : `Нужен только вместе с логином. Не менее ${MIN_PASSWORD_LENGTH} символов; после сохранения появятся данные для входа.`}
          </p>
        </div>
      </AdminModal>
    </>
  );
};
