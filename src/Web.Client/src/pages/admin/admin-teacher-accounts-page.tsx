import { useCallback, useEffect, useMemo, useState } from "react";
import { KeyRound } from "lucide-react";
import { toast } from "sonner";
import {
  dictionariesApi,
  getApiErrorMessage,
  usersApi,
  type TeacherItem,
  type TeacherUser,
} from "@/api.ts";
import {
  AdminTable,
  AdminTableIconCell,
  AdminTableRow,
} from "@/components/admin/admin-shared";
import { Button } from "@/components/ui/button";
import { TableCell } from "@/components/ui/table";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { TeacherAccessModal } from "@/components/admin/teacher-access-modal";
import {
  CredentialsBanner,
  type IssuedCredentials,
} from "@/components/admin/credentials-banner";

// Logins for teachers. The teachers themselves are reference data and are
// added, renamed and deleted under "Справочники"; this page only issues access
// and resets passwords, so every account in the system is managed from the
// "Пользователи" section.
export const AdminTeacherAccountsPage = () => {
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [teacherUsers, setTeacherUsers] = useState<TeacherUser[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [accessTeacher, setAccessTeacher] = useState<TeacherItem | null>(null);
  const [issuedCredentials, setIssuedCredentials] =
    useState<IssuedCredentials | null>(null);

  useAdminPageConfig({
    title: "Пользователи",
    subtitle: "Преподаватели",
  });

  const reload = useCallback(async () => {
    try {
      const [teachersRes, teacherUsersRes] = await Promise.all([
        dictionariesApi.getTeachers(),
        usersApi.getTeacherUsers(),
      ]);
      // A deleted teacher's login is retired with them, so there is nothing
      // to manage for them here.
      setTeachers(teachersRes.data.filter((teacher) => !teacher.isDeleted));
      setTeacherUsers(teacherUsersRes.data);
    } catch (e) {
      toast.error(getApiErrorMessage(e, "Не удалось загрузить данные"));
    }
  }, []);

  // Same shape as useDictionaryCrud: awaiting inside an async wrapper keeps
  // the state updates out of the effect body.
  useEffect(() => {
    void (async () => {
      await reload();
    })();
  }, [reload]);

  const findTeacherUser = (teacherId: string) =>
    teacherUsers.find((account) => account.teacherId === teacherId) ?? null;

  const filteredTeachers = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    if (!query) return teachers;
    return teachers.filter((teacher) => {
      const login =
        teacherUsers.find((account) => account.teacherId === teacher.id)
          ?.login ?? "";
      return (
        teacher.fullName.toLowerCase().includes(query) ||
        login.toLowerCase().includes(query)
      );
    });
  }, [teachers, teacherUsers, searchQuery]);

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
          { header: "Логин" },
          { header: "Действия", className: "text-right w-40" },
        ]}
        renderRow={(teacher) => {
          const account = findTeacherUser(teacher.id);
          return (
            <AdminTableRow key={teacher.id} isDeleted={false}>
              <TableCell className="align-top">
                <AdminTableIconCell
                  textIcon={teacher.fullName.substring(0, 1)}
                  iconColorClass="bg-chart-2/15 text-chart-2"
                  title={teacher.fullName}
                  isDeleted={false}
                />
              </TableCell>

              <TableCell className="align-top">
                {account ? (
                  <span className="font-mono text-sm">{account.login}</span>
                ) : (
                  <span className="text-xs text-muted-foreground">
                    Нет доступа
                  </span>
                )}
              </TableCell>

              <TableCell className="align-top text-right">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setAccessTeacher(teacher)}
                >
                  <KeyRound size={14} className="mr-2" />
                  {account ? "Сменить пароль" : "Выдать доступ"}
                </Button>
              </TableCell>
            </AdminTableRow>
          );
        }}
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
    </>
  );
};
