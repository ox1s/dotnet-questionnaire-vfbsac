import { test, expect } from "@playwright/test";
import { loginAs } from "./utils/auth";
import { mockForms } from "./utils/mock-api";

test.describe("Route protection", () => {
  test("unauthenticated users are redirected to login from the dashboard", async ({
    page,
  }) => {
    await page.goto("/dashboard");
    await expect(page).toHaveURL(/\/login$/);
  });

  test("unauthenticated users are redirected to login from an admin route", async ({
    page,
  }) => {
    await page.goto("/admin/teachers");
    await expect(page).toHaveURL(/\/login$/);
  });

  test("non-admin users are bounced away from admin-only routes", async ({
    page,
  }) => {
    await loginAs(page, "Student");
    await mockForms(page, []);

    await page.goto("/admin/teachers");

    await expect(page).toHaveURL(/\/dashboard$/);
  });

  test("admins can reach admin-only routes", async ({ page }) => {
    await loginAs(page, "Admin");
    await mockForms(page, []);
    await page.route("**/api/dictionaries/**", (route) =>
      route.fulfill({ json: [] }),
    );

    await page.goto("/admin/teachers");

    await expect(page).toHaveURL(/\/admin\/teachers$/);
  });

  test("root path redirects authenticated users to the dashboard", async ({
    page,
  }) => {
    await loginAs(page, "Student");
    await mockForms(page, []);

    await page.goto("/");

    await expect(page).toHaveURL(/\/dashboard$/);
  });
});
