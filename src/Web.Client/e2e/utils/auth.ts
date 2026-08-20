import type { Page } from "@playwright/test";

export type Role = "Admin" | "DeputyHead" | "Student" | "Employer";

/**
 * The client only decodes the JWT payload client-side (see src/utils/auth.ts) and
 * never verifies the signature, so an unsigned token with a well-formed payload is
 * enough to drive the app's role-based routing without a live backend.
 */
function base64UrlEncode(value: string): string {
  return Buffer.from(value)
    .toString("base64")
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");
}

export function buildFakeToken(role: Role, sub = "e2e-user-id"): string {
  const header = base64UrlEncode(JSON.stringify({ alg: "none", typ: "JWT" }));
  const payload = base64UrlEncode(
    JSON.stringify({
      sub,
      role,
      exp: Math.floor(Date.now() / 1000) + 60 * 60,
    }),
  );
  return `${header}.${payload}.`;
}

/** Seeds localStorage with a role token before the app boots, simulating a logged-in user. */
export async function loginAs(page: Page, role: Role) {
  const token = buildFakeToken(role);
  await page.addInitScript((t) => {
    window.localStorage.setItem("token", t);
  }, token);
}
