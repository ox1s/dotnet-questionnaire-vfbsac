import { describe, it, expect, vi, beforeEach } from "vitest";
import axios from "axios";

vi.mock("./utils/auth", () => ({
  logout: vi.fn(),
}));

import { logout } from "./utils/auth";
import { handleUnauthorizedResponse } from "./api";

describe("handleUnauthorizedResponse (api.ts 401 interceptor)", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(axios, "isAxiosError").mockReturnValue(true);
  });

  it("calls the shared logout() helper on a 401 response", async () => {
    const error = {
      isAxiosError: true,
      response: { status: 401 },
    };

    await expect(handleUnauthorizedResponse(error)).rejects.toBe(error);

    expect(logout).toHaveBeenCalledTimes(1);
  });

  it("does not call logout() on a non-401 error", async () => {
    const error = {
      isAxiosError: true,
      response: { status: 500 },
    };

    await expect(handleUnauthorizedResponse(error)).rejects.toBe(error);

    expect(logout).not.toHaveBeenCalled();
  });

  it("does not call logout() on a 401 from the login request itself (wrong password)", async () => {
    const error = {
      isAxiosError: true,
      config: { url: "/users/login" },
      response: { status: 401 },
    };

    await expect(handleUnauthorizedResponse(error)).rejects.toBe(error);

    expect(logout).not.toHaveBeenCalled();
  });
});
