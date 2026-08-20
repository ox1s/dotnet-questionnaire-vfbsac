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
