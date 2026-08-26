import { test, expect, type Page } from "@playwright/test";
import { loginAs } from "./utils/auth";
import { mockForms } from "./utils/mock-api";

const MOBILE_BREAKPOINT = 768;

async function expectNoHorizontalOverflow(page: Page) {
  const overflow = await page.evaluate(() => {
    return (
      document.documentElement.scrollWidth - document.documentElement.clientWidth
    );
  });
  // A 1px tolerance absorbs scrollbar/subpixel rounding differences across engines.
  expect(overflow).toBeLessThanOrEqual(1);
}

test.describe("Mobile compatibility", () => {
  test("login page has no horizontal overflow", async ({ page }) => {
    await page.goto("/login");
    await expectNoHorizontalOverflow(page);
  });

  test("student dashboard has no horizontal overflow", async ({ page }) => {
    await loginAs(page, "Student");
    await mockForms(page, [
      {
        id: "11111111-1111-1111-1111-111111111111",
        title: "Опрос удовлетворенности качеством обучения",
        isActive: true,
        requiredFilters: ["Кафедра", "Специальность"],
      },
    ]);

    await page.goto("/dashboard");
    await expectNoHorizontalOverflow(page);
  });

  test("admin layout has no horizontal overflow", async ({ page }) => {
    await loginAs(page, "Admin");
    await mockForms(page, []);
    await page.route("**/api/dictionaries/**", (route) =>
      route.fulfill({ json: [] }),
    );

    await page.goto("/admin/teachers");
    await expectNoHorizontalOverflow(page);
  });

  test("admin sidebar opens as an overlay sheet on narrow viewports, inline otherwise", async ({
    page,
  }) => {
    await loginAs(page, "Admin");
    await mockForms(page, []);
    await page.route("**/api/dictionaries/**", (route) =>
      route.fulfill({ json: [] }),
    );

    await page.goto("/admin/teachers");
    await page
      .locator("header")
      .getByRole("button", { name: "Toggle Sidebar" })
      .click();

    const viewport = page.viewportSize();
    const isNarrow = (viewport?.width ?? 1280) < MOBILE_BREAKPOINT;

    if (isNarrow) {
      await expect(page.locator('[data-mobile="true"]')).toBeVisible();
    } else {
      await expect(page.locator('[data-mobile="true"]')).toHaveCount(0);
    }
  });

  test("interactive controls meet a minimum touch target size", async ({
    page,
  }) => {
    const viewport = page.viewportSize();
    test.skip(
      (viewport?.width ?? 1280) >= MOBILE_BREAKPOINT,
      "Touch target sizing only matters on narrow/touch viewports",
    );

    await page.goto("/login");
    const submit = page.getByRole("button", { name: "Войти" });
    const box = await submit.boundingBox();

    expect(box).not.toBeNull();
    expect(box!.height).toBeGreaterThanOrEqual(32);
  });
});
