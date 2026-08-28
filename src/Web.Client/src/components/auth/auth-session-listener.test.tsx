import { describe, it, expect, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Routes, Route } from "react-router-dom";
import { AuthSessionListener } from "./auth-session-listener";
import { LoginPage } from "@/pages/auth/login-page";
import { logout } from "@/utils/auth";

const AdminPageStub = () => <button onClick={logout}>Выйти</button>;

const renderApp = () =>
  render(
    <MemoryRouter initialEntries={["/admin/teachers"]}>
      <AuthSessionListener />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/admin/teachers" element={<AdminPageStub />} />
      </Routes>
    </MemoryRouter>,
  );

describe("logout regression: admin logout reaches a fully-rendered login page", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("clears the token and client-side-navigates from an admin route to the real login form (not a blank page)", async () => {
    localStorage.setItem("token", "fake-admin-token");
    renderApp();
    const user = userEvent.setup();

    await user.click(screen.getByText("Выйти"));

    expect(localStorage.getItem("token")).toBeNull();
    expect(screen.getByText(/Войдите в систему/i)).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /Войти/i }),
    ).toBeInTheDocument();
  });
});
