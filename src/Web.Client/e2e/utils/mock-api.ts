import type { Page } from "@playwright/test";
import type { Form } from "../../src/api";

/** Backend is a full docker stack (Postgres/Seq/etc.) that isn't available in CI,
 * so e2e tests stub the `/api` surface at the network layer instead. */
export async function mockLogin(
  page: Page,
  options: { token: string } | { status: number; detail: string },
) {
  await page.route("**/api/users/login", async (route) => {
    if ("token" in options) {
      await route.fulfill({ status: 200, json: options.token });
    } else {
      await route.fulfill({
        status: options.status,
        json: { detail: options.detail },
      });
    }
  });
}

export async function mockForms(page: Page, forms: Form[]) {
  await page.route("**/api/forms", async (route) => {
    if (route.request().method() === "GET") {
      await route.fulfill({ json: forms });
    } else {
      await route.continue();
    }
  });
  await page.route("**/api/forms/admin", async (route) => {
    await route.fulfill({ json: forms });
  });
}
