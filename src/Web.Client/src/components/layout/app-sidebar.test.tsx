import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { AppSidebar } from "./app-sidebar";
import { SidebarProvider } from "@/components/ui/sidebar";
import { ThemeProvider } from "@/components/layout/theme-provider";
import { TooltipProvider } from "@/components/ui/tooltip";

vi.mock("@/utils/auth", () => ({
  logout: vi.fn(),
}));

import { logout } from "@/utils/auth";

const renderSidebar = () =>
  render(
    <MemoryRouter initialEntries={["/admin/teachers"]}>
      <ThemeProvider>
        <TooltipProvider>
          <SidebarProvider>
            <AppSidebar />
          </SidebarProvider>
        </TooltipProvider>
      </ThemeProvider>
    </MemoryRouter>,
  );

// Collapsed groups don't render their links, so expand one by its trigger.
const expandGroup = async (name: string) => {
  await userEvent.setup().click(screen.getByRole("button", { name }));
};

describe("AppSidebar logout button", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("calls the shared logout() helper instead of a hard window navigation", async () => {
    renderSidebar();
    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /выйти|logout/i }));

    expect(logout).toHaveBeenCalledTimes(1);
  });
});

describe("AppSidebar sections", () => {
  // NavMain remembers expanded groups in session storage; start each test
  // from the defaults so expanding a group in one doesn't toggle it in the next.
  beforeEach(() => {
    window.sessionStorage.clear();
  });

  it("links to the staff accounts page", () => {
    renderSidebar();

    expect(screen.getByRole("link", { name: "Сотрудники" })).toHaveAttribute(
      "href",
      "/admin/staff",
    );
  });

  it("follows the Admin IA group and page order", async () => {
    renderSidebar();
    await expandGroup("Настройки");

    const titles = screen
      .getAllByRole("link")
      .map((link) => link.textContent)
      .filter((title) => title !== "ВФБГАС");

    expect(titles).toEqual([
      "Все анкеты",
      "Конструктор",
      "Преподаватели",
      "Сотрудники",
      "Группы",
      "Наниматели",
      "Кафедры",
      "Специальности",
      "Дисциплины",
      "Семестр",
      "Аккаунт",
      "Администраторы",
    ]);
  });

  it("has one teacher page covering both the directory and logins", () => {
    renderSidebar();

    const hrefs = screen
      .getAllByRole("link", { name: "Преподаватели" })
      .map((link) => link.getAttribute("href"));

    expect(hrefs).toEqual(["/admin/teachers"]);
  });

  it("lists employers among the participants, not the settings", async () => {
    renderSidebar();
    await expandGroup("Настройки");

    expect(screen.getByRole("link", { name: "Наниматели" })).toHaveAttribute(
      "href",
      "/admin/employers",
    );
    expect(screen.getByRole("link", { name: "Семестр" })).toHaveAttribute(
      "href",
      "/admin/settings",
    );
  });

  it("moves the admin's own login and password to the Аккаунт page", async () => {
    renderSidebar();
    await expandGroup("Настройки");

    expect(screen.getByRole("link", { name: "Аккаунт" })).toHaveAttribute(
      "href",
      "/admin/account",
    );
    expect(
      screen.queryByRole("button", { name: "Сменить пароль" }),
    ).not.toBeInTheDocument();
  });

  it("manages other admins from the settings", async () => {
    renderSidebar();
    await expandGroup("Настройки");

    expect(
      screen.getByRole("link", { name: "Администраторы" }),
    ).toHaveAttribute("href", "/admin/admins");
  });
});
