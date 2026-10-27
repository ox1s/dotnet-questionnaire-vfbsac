import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { AdminPageContext, type AdminPageConfig } from "@/hooks/use-admin-page";
import { AdminAdminsPage } from "./admin-admins-page";

vi.mock("@/api.ts", () => ({
  usersApi: {
    getAdmins: vi.fn(),
    createAdmin: vi.fn(),
    updateUser: vi.fn(),
    setPassword: vi.fn(),
  },
  getApiErrorMessage: (_: unknown, fallback: string) => fallback,
}));

vi.mock("@/utils/auth.ts", () => ({
  getUserInfo: () => ({ sub: "admin-me", role: "Admin", exp: 0 }),
}));

import { usersApi } from "@/api.ts";

const me = { id: "admin-me", login: "ADMIN", displayName: "ADMIN" };
const deputy = { id: "admin-2", login: "DEPUTY", displayName: "DEPUTY" };

// Stands in for AppShell's header: the page's "Добавить" button is rendered
// from the config it pushes up, not by the page itself.
const Shell = () => {
  const [config, setConfig] = useState<AdminPageConfig>({});
  return (
    <AdminPageContext.Provider value={{ config, setConfig }}>
      <header>{config.actions}</header>
      <AdminAdminsPage />
    </AdminPageContext.Provider>
  );
};

const renderPage = async () => {
  render(<Shell />);
  await screen.findByText("DEPUTY");
};

const rowOf = (login: string) => screen.getByText(login).closest("tr")!;

describe("AdminAdminsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(usersApi.getAdmins).mockResolvedValue({
      data: [me, deputy],
    } as never);
    vi.mocked(usersApi.createAdmin).mockResolvedValue({ data: "new" } as never);
    vi.mocked(usersApi.updateUser).mockResolvedValue({} as never);
    vi.mocked(usersApi.setPassword).mockResolvedValue({} as never);
  });

  it("lists admins, marks the signed-in one and offers no deletion", async () => {
    await renderPage();

    expect(within(rowOf("ADMIN")).getByText("Ваш аккаунт")).toBeInTheDocument();
    expect(
      within(rowOf("DEPUTY")).queryByText("Ваш аккаунт"),
    ).not.toBeInTheDocument();
    expect(screen.queryByTitle("Удалить")).not.toBeInTheDocument();
  });

  it("adds an admin with a login and password", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /Добавить/ }));
    await user.type(screen.getByLabelText("Логин"), "NEW_ADMIN");
    const password = (screen.getByLabelText("Пароль") as HTMLInputElement)
      .value;
    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(password.length).toBeGreaterThanOrEqual(12);
    expect(usersApi.createAdmin).toHaveBeenCalledWith("NEW_ADMIN", password);
  });

  it("renames another admin's login without touching the password", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.click(within(rowOf("DEPUTY")).getByTitle("Редактировать"));
    await user.clear(screen.getByLabelText("Логин"));
    await user.type(screen.getByLabelText("Логин"), "DEPUTY_2");
    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(usersApi.updateUser).toHaveBeenCalledWith(
      "admin-2",
      "DEPUTY_2",
      "DEPUTY_2",
    );
    expect(usersApi.setPassword).not.toHaveBeenCalled();
  });

  it("sets a new password for another admin when one is entered", async () => {
    await renderPage();
    const user = userEvent.setup();

    await user.click(within(rowOf("DEPUTY")).getByTitle("Редактировать"));
    await user.type(screen.getByLabelText("Новый пароль"), "deputy-password-1");
    await user.click(screen.getByRole("button", { name: "Сохранить" }));

    expect(usersApi.setPassword).toHaveBeenCalledWith(
      "admin-2",
      "deputy-password-1",
    );
  });
});
