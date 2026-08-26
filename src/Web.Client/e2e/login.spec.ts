import { test, expect } from "@playwright/test";
import { buildFakeToken } from "./utils/auth";
import { mockLogin, mockForms } from "./utils/mock-api";

test.describe("Login", () => {
  test("renders the login form", async ({ page }) => {
    await page.goto("/login");

    await expect(page.getByText("Войдите в систему")).toBeVisible();
    await expect(page.getByPlaceholder("Например: ПО111")).toBeVisible();
    await expect(page.getByRole("button", { name: "Войти" })).toBeVisible();
  });

  test("shows an error message on invalid credentials", async ({ page }) => {
    await mockLogin(page, { status: 401, detail: "Неверный логин или пароль" });
    await page.goto("/login");

    await page.getByPlaceholder("Например: ПО111").fill("ПО111");
    await page.locator('input[type="password"]').fill("wrong-password");
    await page.getByRole("button", { name: "Войти" }).click();

    await expect(page.getByText("Неверный логин или пароль")).toBeVisible();
    await expect(page).toHaveURL(/\/login$/);
  });

  test("redirects to the dashboard on successful login", async ({ page }) => {
    await mockLogin(page, { token: buildFakeToken("Student") });
    await mockForms(page, []);
    await page.goto("/login");

    await page.getByPlaceholder("Например: ПО111").fill("ПО111");
    await page.locator('input[type="password"]').fill("correct-password");
    await page.getByRole("button", { name: "Войти" }).click();

    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(
      page.getByRole("heading", { name: "Доступные анкеты" }),
    ).toBeVisible();
  });
});
