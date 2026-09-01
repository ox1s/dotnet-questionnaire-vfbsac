import { describe, it, expect, beforeEach, vi } from "vitest";
import { AUTH_LOGOUT_EVENT, logout } from "./auth";

describe("logout", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("removes the auth token from localStorage", () => {
    localStorage.setItem("token", "fake-token");

    logout();

    expect(localStorage.getItem("token")).toBeNull();
  });

  it("dispatches the auth:logout event so listeners can navigate client-side", () => {
    const handler = vi.fn();
    window.addEventListener(AUTH_LOGOUT_EVENT, handler);

    logout();

    expect(handler).toHaveBeenCalledTimes(1);
    window.removeEventListener(AUTH_LOGOUT_EVENT, handler);
  });
});
