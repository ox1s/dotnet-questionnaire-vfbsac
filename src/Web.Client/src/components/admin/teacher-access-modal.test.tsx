import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { TeacherAccessModal } from "./teacher-access-modal";

vi.mock("@/api.ts", () => ({
  usersApi: {
    createTeacherUser: vi.fn().mockResolvedValue({ data: "new-user-id" }),
    setPassword: vi.fn().mockResolvedValue({}),
  },
  getApiErrorMessage: (_: unknown, fallback: string) => fallback,
}));

import { usersApi } from "@/api.ts";

const teacher = { id: "teacher-1", fullName: "Иванова Анна Петровна" };

describe("TeacherAccessModal", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("creates a login for a teacher who has none and reports the credentials", async () => {
    const onSaved = vi.fn();
    render(
      <TeacherAccessModal
        teacher={teacher}
        account={null}
        onClose={vi.fn()}
        onSaved={onSaved}
      />,
    );
    const user = userEvent.setup();

    await user.type(screen.getByLabelText("Логин"), "IVANOVA");
    const password = (screen.getByLabelText("Пароль") as HTMLInputElement).value;
    expect(password).toHaveLength(12);

    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(usersApi.createTeacherUser).toHaveBeenCalledWith(
      "teacher-1",
      "IVANOVA",
      password,
    );
    expect(onSaved).toHaveBeenCalledWith(
      expect.objectContaining({ login: "IVANOVA", pass: password }),
    );
  });

  it("resets the password of a teacher who already has a login", async () => {
    const onSaved = vi.fn();
    render(
      <TeacherAccessModal
        teacher={teacher}
        account={{
          id: "user-1",
          login: "IVANOVA",
          displayName: teacher.fullName,
          teacherId: teacher.id,
        }}
        onClose={vi.fn()}
        onSaved={onSaved}
      />,
    );
    const user = userEvent.setup();

    expect(screen.getByText("IVANOVA")).toBeInTheDocument();
    const password = (screen.getByLabelText("Новый пароль") as HTMLInputElement)
      .value;

    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(usersApi.createTeacherUser).not.toHaveBeenCalled();
    expect(usersApi.setPassword).toHaveBeenCalledWith("user-1", password);
    expect(onSaved).toHaveBeenCalledWith(
      expect.objectContaining({ login: "IVANOVA", pass: password }),
    );
  });
});
