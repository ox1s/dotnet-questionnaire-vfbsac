import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AdminAccountPage } from "./admin-account-page";

vi.mock("@/api.ts", () => ({
  usersApi: {
    getMe: vi.fn(),
    updateUser: vi.fn(),
    setPassword: vi.fn(),
  },
  getApiErrorMessage: (_: unknown, fallback: string) => fallback,
}));

vi.mock("@/hooks/use-admin-page-config", () => ({
  useAdminPageConfig: vi.fn(),
}));

import { usersApi } from "@/api.ts";

const renderPage = async () => {
  render(<AdminAccountPage />);
  await screen.findByDisplayValue("ADMIN");
};

describe("AdminAccountPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(usersApi.getMe).mockResolvedValue({
      data: { id: "admin-me", login: "ADMIN", displayName: "ADMIN" },
    } as never);
    vi.mocked(usersApi.updateUser).mockResolvedValue({} as never);
    vi.mocked(usersApi.setPassword).mockResolvedValue({} as never);
  });

  it("changes the signed-in admin's own login", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.clear(screen.getByLabelText("Логин"));
    await user.type(screen.getByLabelText("Логин"), "HEAD_ADMIN");
    await user.click(screen.getByRole("button", { name: "Сохранить логин" }));

    expect(usersApi.updateUser).toHaveBeenCalledWith(
      "admin-me",
      "HEAD_ADMIN",
      "HEAD_ADMIN",
    );
  });

  it("changes the signed-in admin's own password", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.type(screen.getByLabelText("Новый пароль"), "my-new-password-1");
    await user.type(
      screen.getByLabelText("Повторите пароль"),
      "my-new-password-1",
    );
    await user.click(screen.getByRole("button", { name: "Сменить пароль" }));

    expect(usersApi.setPassword).toHaveBeenCalledWith(
      "admin-me",
      "my-new-password-1",
    );
  });

  it("refuses a password that does not match its confirmation", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.type(screen.getByLabelText("Новый пароль"), "my-new-password-1");
    await user.type(
      screen.getByLabelText("Повторите пароль"),
      "my-new-password-2",
    );
    await user.click(screen.getByRole("button", { name: "Сменить пароль" }));

    expect(usersApi.setPassword).not.toHaveBeenCalled();
  });
});
