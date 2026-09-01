import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Outlet, Route, Routes } from "react-router-dom";
import { AppSidebar } from "./app-sidebar";
import { SidebarProvider } from "@/components/ui/sidebar";
import { ThemeProvider } from "@/components/layout/theme-provider";
import { TooltipProvider } from "@/components/ui/tooltip";

vi.mock("@/utils/auth", () => ({
  logout: vi.fn(),
}));

// Mirrors App.tsx: every sidebar-bearing screen sits under one <AppShell/>
// layout route, so navigating between them re-renders only the <Outlet/>.
const Shell = () => (
  <SidebarProvider>
    <AppSidebar />
    <Outlet />
  </SidebarProvider>
);

const renderApp = (initialPath: string) =>
  render(
    <MemoryRouter initialEntries={[initialPath]}>
      <ThemeProvider>
        <TooltipProvider>
          <Routes>
            <Route element={<Shell />}>
              <Route path="/dashboard" element={<div>dashboard page</div>} />
              <Route
                path="/admin/create-form"
                element={<div>create form page</div>}
              />
              <Route
                path="/admin/settings"
                element={<div>settings page</div>}
              />
            </Route>
          </Routes>
        </TooltipProvider>
      </ThemeProvider>
    </MemoryRouter>,
  );

// "Наниматели" only exists under the "Настройки" group, so it is a reliable
// marker for whether that group is expanded.
const settingsGroupIsOpen = () =>
  screen.queryByRole("link", { name: "Наниматели" }) !== null;

describe("NavMain group expansion", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    window.sessionStorage.clear();
  });

  it("keeps the Настройки group expanded after navigating to Конструктор анкет", async () => {
    renderApp("/dashboard");
    const user = userEvent.setup();

    expect(settingsGroupIsOpen()).toBe(false);
    await user.click(screen.getByRole("button", { name: /Настройки/ }));
    expect(settingsGroupIsOpen()).toBe(true);

    await user.click(screen.getByRole("link", { name: "Конструктор анкет" }));
    expect(screen.getByText("create form page")).toBeInTheDocument();

    expect(settingsGroupIsOpen()).toBe(true);
  });

  it("keeps the Настройки group expanded after navigating to Анкеты", async () => {
    renderApp("/admin/create-form");
    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /Настройки/ }));
    expect(settingsGroupIsOpen()).toBe(true);

    await user.click(screen.getByRole("link", { name: "Анкеты" }));
    expect(screen.getByText("dashboard page")).toBeInTheDocument();

    expect(settingsGroupIsOpen()).toBe(true);
  });

  it("still auto-expands the group that owns the current route", () => {
    renderApp("/admin/settings");

    expect(settingsGroupIsOpen()).toBe(true);
  });

  it("respects a group the user collapsed, even when it owns the current route", async () => {
    renderApp("/admin/settings");
    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /Настройки/ }));
    expect(settingsGroupIsOpen()).toBe(false);

    await user.click(screen.getByRole("link", { name: "Анкеты" }));
    expect(screen.getByText("dashboard page")).toBeInTheDocument();

    expect(settingsGroupIsOpen()).toBe(false);
  });

  // The layout survives navigation but not a reload or a re-login, which is
  // what the session-storage persistence in nav-main.tsx is there for.
  it("restores the group state after a remount", async () => {
    const first = renderApp("/dashboard");
    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /Настройки/ }));
    expect(settingsGroupIsOpen()).toBe(true);
    first.unmount();

    renderApp("/dashboard");

    expect(settingsGroupIsOpen()).toBe(true);
  });
});
