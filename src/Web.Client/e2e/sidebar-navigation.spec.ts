import { test, expect, type Page } from "@playwright/test";
import { loginAs } from "./utils/auth";
import { mockForms } from "./utils/mock-api";

// Regression test for the reported bug: expanding "Настройки" and then clicking
// "Анкеты" or "Конструктор анкет" (now "Все анкеты" / "Конструктор") collapsed the group again. /dashboard used to
// render its own <AdminLayout>, so moving between it and the /admin/* pages
// unmounted and rebuilt the whole sidebar. Everything now shares one <AppShell>
// layout route, so the sidebar simply stays mounted.
//
// "Семестр" only exists under "Настройки", which makes it a reliable marker
// for whether that group is expanded.

// Locators are scoped to the sidebar because the breadcrumb in the header also
// exposes "Все анкеты" with a link role, which would otherwise be ambiguous.
const sidebarOf = (page: Page) => page.locator('[data-sidebar="sidebar"]');

test.describe("Sidebar navigation", () => {
  // Below the 768px breakpoint (use-mobile.ts) the sidebar is an off-canvas
  // Sheet that is dismissed on navigation, so "stays expanded" isn't a
  // meaningful assertion there; mobile.spec.ts covers that viewport.
  test.skip(
    ({ viewport }) => (viewport?.width ?? 1280) < 768,
    "sidebar is an off-canvas sheet on narrow viewports",
  );

  test.beforeEach(async ({ page }) => {
    await loginAs(page, "Admin");
    await mockForms(page, []);
    await page.route("**/api/dictionaries/**", (route) =>
      route.fulfill({ json: [] }),
    );
  });

  test("an expanded nav group survives navigating between the dashboard and an admin page", async ({
    page,
  }) => {
    const sidebar = sidebarOf(page);
    const semesterLink = sidebar.getByRole("link", { name: "Семестр" });

    await page.goto("/dashboard");
    await expect(semesterLink).toBeHidden();

    await sidebar.getByRole("button", { name: /Настройки/ }).click();
    await expect(semesterLink).toBeVisible();

    await sidebar.getByRole("link", { name: "Конструктор" }).click();
    await expect(page).toHaveURL(/\/admin\/create-form$/);
    await expect(semesterLink).toBeVisible();

    await sidebar.getByRole("link", { name: "Все анкеты" }).click();
    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(semesterLink).toBeVisible();
  });

  test("a group the user collapsed by hand stays collapsed across navigation", async ({
    page,
  }) => {
    const sidebar = sidebarOf(page);
    const semesterLink = sidebar.getByRole("link", { name: "Семестр" });

    // /admin/settings lives under "Настройки", so the group auto-expands.
    await page.goto("/admin/settings");
    await expect(semesterLink).toBeVisible();

    await sidebar.getByRole("button", { name: /Настройки/ }).click();
    await expect(semesterLink).toBeHidden();

    await sidebar.getByRole("link", { name: "Все анкеты" }).click();
    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(semesterLink).toBeHidden();
  });
});
