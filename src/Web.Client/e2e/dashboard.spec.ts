import { test, expect } from "@playwright/test";
import { loginAs } from "./utils/auth";
import { mockForms } from "./utils/mock-api";
import type { Form } from "../src/api";

const sampleForms: Form[] = [
  {
    id: "11111111-1111-1111-1111-111111111111",
    title: "Опрос удовлетворенности качеством обучения",
    isActive: true,
    requiredFilters: [],
  },
];

test.describe("Student dashboard", () => {
  test("lists available forms and links to the survey", async ({ page }) => {
    await loginAs(page, "Student");
    await mockForms(page, sampleForms);

    await page.goto("/dashboard");

    await expect(
      page.getByText("Опрос удовлетворенности качеством обучения"),
    ).toBeVisible();
    await expect(
      page.getByRole("link", { name: /Пройти опрос/ }),
    ).toHaveAttribute("href", `/form/${sampleForms[0].id}`);
  });

  test("shows an empty state when there are no forms", async ({ page }) => {
    await loginAs(page, "Student");
    await mockForms(page, []);

    await page.goto("/dashboard");

    await expect(page.getByText("Нет доступных анкет")).toBeVisible();
  });

  test("logout clears the session and returns to login", async ({ page }) => {
    await loginAs(page, "Student");
    await mockForms(page, []);

    await page.goto("/dashboard");
    await page.getByRole("button", { name: /Выйти/ }).click();

    await expect(page).toHaveURL(/\/login$/);
    const token = await page.evaluate(() => window.localStorage.getItem("token"));
    expect(token).toBeNull();
  });
});

test.describe("Admin dashboard", () => {
  // Regression test for the reported bug: logging out from an admin page used to
  // hard-navigate to /login (window.location.href), which returned a real server
  // "Not Found" on hosts with no SPA fallback. Logout must stay client-side, land
  // on /login, and render the actual login form rather than a blank page.
  test("sidebar logout stays client-side and lands on a fully-rendered login page", async ({
    page,
  }) => {
    await loginAs(page, "Admin");
    await mockForms(page, []);
    await page.route("**/api/dictionaries/**", (route) =>
      route.fulfill({ json: [] }),
    );

    await page.goto("/admin/teachers");
    await expect(page).toHaveURL(/\/admin\/teachers$/);

    const responses: number[] = [];
    page.on("response", (response) => {
      if (response.url().endsWith("/login")) {
        responses.push(response.status());
      }
    });

    await page.getByRole("button", { name: /Выйти/ }).click();

    await expect(page).toHaveURL(/\/login$/);
    const token = await page.evaluate(() => window.localStorage.getItem("token"));
    expect(token).toBeNull();

    // No document navigation to /login was ever issued by the browser.
    expect(responses).toHaveLength(0);

    await expect(page.getByText("Войдите в систему")).toBeVisible();
    await expect(
      page.getByRole("button", { name: "Войти" }),
    ).toBeVisible();
  });
});
