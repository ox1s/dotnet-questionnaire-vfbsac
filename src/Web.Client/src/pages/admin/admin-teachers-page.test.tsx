import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { AdminPageContext, type AdminPageConfig } from "@/hooks/use-admin-page";
import { AdminTeachersPage } from "./admin-teachers-page";

vi.mock("@/api.ts", () => ({
  dictionariesApi: {
    getTeachers: vi.fn(),
    getDepartments: vi.fn(),
    createTeacher: vi.fn(),
    updateTeacher: vi.fn(),
    deleteTeacher: vi.fn(),
    restoreTeacher: vi.fn(),
  },
  usersApi: {
    getTeacherUsers: vi.fn(),
    createTeacherUser: vi.fn(),
    updateUser: vi.fn(),
    setPassword: vi.fn(),
  },
  getApiErrorMessage: (_: unknown, fallback: string) => fallback,
}));

import { dictionariesApi, usersApi } from "@/api.ts";

const withAccount = {
  id: "teacher-1",
  fullName: "Иванова Анна Петровна",
  departmentIds: [],
  isDeleted: false,
};
const withoutAccount = {
  id: "teacher-2",
  fullName: "Петров Пётр Петрович",
  departmentIds: [],
  isDeleted: false,
};

// Stands in for AppShell's header: the page's "Добавить" button is rendered
// from the config it pushes up, not by the page itself.
const Shell = () => {
  const [config, setConfig] = useState<AdminPageConfig>({});
  return (
    <AdminPageContext.Provider value={{ config, setConfig }}>
      <header>{config.actions}</header>
      <AdminTeachersPage />
    </AdminPageContext.Provider>
  );
};

const renderPage = async () => {
  render(<Shell />);
  await screen.findByText("Иванова Анна Петровна");
};

const editButtonFor = (name: string) =>
  within(screen.getByText(name).closest("tr")!).getByTitle("Редактировать");

describe("AdminTeachersPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(dictionariesApi.getTeachers).mockResolvedValue({
      data: [withAccount, withoutAccount],
    } as never);
    vi.mocked(dictionariesApi.getDepartments).mockResolvedValue({
      data: [],
    } as never);
    vi.mocked(usersApi.getTeacherUsers).mockResolvedValue({
      data: [
        {
          id: "user-1",
          login: "IVANOVA",
          displayName: "Иванова Анна Петровна",
          teacherId: "teacher-1",
        },
      ],
    } as never);
    vi.mocked(dictionariesApi.createTeacher).mockResolvedValue({
      data: "teacher-new",
    } as never);
  });

  it("has no separate access button: access is edited with the teacher", async () => {
    await renderPage();

    expect(
      screen.queryByRole("button", { name: /Выдать доступ|Сменить пароль/ }),
    ).not.toBeInTheDocument();
  });

  it("sets a new password from the edit dialog", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.click(editButtonFor("Иванова Анна Петровна"));
    expect(screen.getByLabelText("Логин")).toHaveValue("IVANOVA");
    await user.type(screen.getByLabelText("Новый пароль"), "new-password-123");
    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(usersApi.setPassword).toHaveBeenCalledWith(
      "user-1",
      "new-password-123",
    );
    expect(await screen.findByText("Доступ преподавателя")).toBeInTheDocument();
  });

  it("renames the login from the edit dialog", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.click(editButtonFor("Иванова Анна Петровна"));
    await user.clear(screen.getByLabelText("Логин"));
    await user.type(screen.getByLabelText("Логин"), "IVANOVA_A");
    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(usersApi.updateUser).toHaveBeenCalledWith(
      "user-1",
      "IVANOVA_A",
      "Иванова Анна Петровна",
    );
    expect(usersApi.setPassword).not.toHaveBeenCalled();
  });

  it("issues access to a teacher without a login from the edit dialog", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.click(editButtonFor("Петров Пётр Петрович"));
    await user.type(screen.getByLabelText("Логин"), "PETROV");
    await user.type(screen.getByLabelText("Пароль"), "petrov-password-1");
    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(usersApi.createTeacherUser).toHaveBeenCalledWith(
      "teacher-2",
      "PETROV",
      "petrov-password-1",
    );
  });

  it("creates a teacher together with their login", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /Добавить/ }));
    await user.type(
      screen.getByPlaceholderText("Введите фио..."),
      "Сидорова Мария Ивановна",
    );
    await user.type(screen.getByLabelText("Логин"), "SIDOROVA");
    const password = (screen.getByLabelText("Пароль") as HTMLInputElement)
      .value;
    expect(password.length).toBeGreaterThanOrEqual(12);
    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(dictionariesApi.createTeacher).toHaveBeenCalledWith(
      "Сидорова Мария Ивановна",
      [],
    );
    expect(usersApi.createTeacherUser).toHaveBeenCalledWith(
      "teacher-new",
      "SIDOROVA",
      password,
    );
  });

  it("creates a teacher without access when the login is left empty", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /Добавить/ }));
    await user.type(
      screen.getByPlaceholderText("Введите фио..."),
      "Сидорова Мария Ивановна",
    );
    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(dictionariesApi.createTeacher).toHaveBeenCalled();
    expect(usersApi.createTeacherUser).not.toHaveBeenCalled();
  });
});
