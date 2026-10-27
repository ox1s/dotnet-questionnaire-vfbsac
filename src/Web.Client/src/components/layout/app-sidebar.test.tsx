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
    <MemoryRouter initialEntries={["/admin/teacher-accounts"]}>
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
  it("links to the staff accounts page", () => {
    renderSidebar();

    expect(screen.getByRole("link", { name: "Сотрудники" })).toHaveAttribute(
      "href",
      "/admin/staff",
    );
  });

  it("keeps the teacher directory and teacher logins on separate pages", () => {
    renderSidebar();

    const hrefs = screen
      .getAllByRole("link", { name: "Преподаватели" })
      .map((link) => link.getAttribute("href"));

    expect(hrefs).toEqual(["/admin/teachers", "/admin/teacher-accounts"]);
  });

  it("lists employers under the dictionaries, not the settings", async () => {
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

  it("offers the admin's own password change from the footer", () => {
    renderSidebar();

    expect(
      screen.getByRole("button", { name: "Сменить пароль" }),
    ).toBeInTheDocument();
  });
});
