import { test, expect } from "@playwright/test";
import { loginAs } from "./utils/auth";
import { mockForms } from "./utils/mock-api";
import type { Form, FormDetail } from "../src/api";

const staffForm: Form = {
  id: "22222222-2222-2222-2222-222222222222",
  title: "Оценка удовлетворённости персонала работой в колледже",
  isActive: true,
  requiredFilters: ["EmployeeCategory"],
  targetRole: "Staff",
};

const staffFormDetail: FormDetail = {
  ...staffForm,
  questions: [
    {
      id: "33333333-3333-3333-3333-333333333333",
      text: "Удовлетворённость условиями труда",
      type: "Number",
      order: 1,
    },
  ],
};

test.describe("Personnel category (АУП/ППС/УВП/ПОП) context", () => {
  test("dashboard shows the Russian label for the EmployeeCategory filter, not the raw key", async ({
    page,
  }) => {
    await loginAs(page, "Staff");
    await mockForms(page, [staffForm]);

    await page.goto("/dashboard");

    await expect(page.getByText("Категория персонала")).toBeVisible();
    await expect(page.getByText("EmployeeCategory", { exact: true })).toHaveCount(0);
  });

  test("survey page lets a staff member pick a personnel category and submits it", async ({
    page,
  }) => {
    await loginAs(page, "Staff");
    await mockForms(page, [staffForm]);
    await page.route(`**/api/forms/${staffForm.id}`, (route) =>
      route.fulfill({ json: staffFormDetail }),
    );

    let submittedBody: Record<string, unknown> | null = null;
    await page.route("**/api/submissions", async (route) => {
      submittedBody = route.request().postDataJSON();
      await route.fulfill({ json: "44444444-4444-4444-4444-444444444444" });
    });

    await page.goto(`/form/${staffForm.id}`);

    await expect(page.getByText("Категория персонала")).toBeVisible();

    const categoryTrigger = page.locator('[data-slot="select-trigger"]').last();
    await categoryTrigger.click();
    await page.getByRole("option", { name: "ППС" }).click();

    await page.getByPlaceholder("1–10").fill("8");
    await page.getByRole("button", { name: "Отправить анкету" }).click();

    await expect(page.getByText("Анкета успешно отправлена!")).toBeVisible();
    expect(submittedBody).not.toBeNull();
    expect(submittedBody?.employeeCategory).toBe("ППС");
  });
});
