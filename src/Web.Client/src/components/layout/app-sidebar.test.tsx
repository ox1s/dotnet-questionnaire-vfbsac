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
