import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { DashboardContent } from "./dashboard-content";
import type { Form } from "@/api.ts";

const formWithFilters: Form = {
  id: "11111111-1111-1111-1111-111111111111",
  title: "Оценка удовлетворённости персонала работой в колледже",
  isActive: true,
  requiredFilters: ["EmployeeCategory", "Department"],
};

const renderDashboard = (form: Form) =>
  render(
    <MemoryRouter>
      <DashboardContent forms={[form]} isAdmin={false} />
    </MemoryRouter>,
  );

describe("DashboardContent required-filter badges", () => {
  it("renders Russian labels for required filters instead of raw English keys", () => {
    renderDashboard(formWithFilters);

    expect(screen.getByText("Категория персонала")).toBeInTheDocument();
    expect(screen.getByText("Филиал кафедры")).toBeInTheDocument();
    expect(screen.queryByText("EmployeeCategory")).not.toBeInTheDocument();
    expect(screen.queryByText("Department")).not.toBeInTheDocument();
  });
});
