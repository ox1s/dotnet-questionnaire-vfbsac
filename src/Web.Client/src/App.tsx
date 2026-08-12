import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "@/components/ui/sonner";
import { LoginPage } from "./pages/auth/login-page";
import { DashboardPage } from "./pages/dashboard/dashboard-page";
import { SurveyPage } from "./pages/forms/survey-page";
import { AdminStatsPage } from "./pages/admin/admin-stats-page";
import { CreateFormPage } from "./pages/forms/create-form-page";
import { AdminTeachersPage } from "./pages/admin/admin-teachers-page";
import { AdminDisciplinesPage } from "./pages/admin/admin-disciplines-page";
import { AdminDepartmentsPage } from "./pages/admin/admin-departments-page";
import { AdminGroupsPage } from "./pages/admin/admin-groups-page";
import { AdminEmployersPage } from "./pages/admin/admin-employers-page";
import { ProtectedRoute } from "./components/auth/protected-route";
import { AdminSettingsPage } from "./pages/admin/admin-settings-page";
import { AdminSpecialitiesPage } from "./pages/admin/admin-specialities-page";
import { AdminSpecializationsPage } from "./pages/admin/admin-specializations-page";
import { TooltipProvider } from "@/components/ui/tooltip";
import { ThemeProvider } from "@/components/layout/theme-provider";
import { AdminLayout } from "./components/admin/admin-shared";

function App() {
  return (
    <BrowserRouter>
      <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
        <TooltipProvider delayDuration={0}>
          <Toaster position="top-center" />
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/" element={<Navigate to="/dashboard" replace />} />

            <Route element={<ProtectedRoute />}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/form/:id" element={<SurveyPage />} />
            </Route>

            <Route
              element={
                <ProtectedRoute allowedRoles={["Admin", "DeputyHead"]} />
              }
            >
              <Route path="/admin/stats/:id" element={<AdminStatsPage />} />
            </Route>

            <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
              <Route element={<AdminLayout />}>
                <Route path="/admin/create-form" element={<CreateFormPage />} />
                <Route path="/admin/teachers" element={<AdminTeachersPage />} />
                <Route
                  path="/admin/disciplines"
                  element={<AdminDisciplinesPage />}
                />
                <Route
                  path="/admin/departments"
                  element={<AdminDepartmentsPage />}
                />
                <Route
                  path="/admin/specialities"
                  element={<AdminSpecialitiesPage />}
                />
                <Route
                  path="/admin/specializations"
                  element={<AdminSpecializationsPage />}
                />
                <Route path="/admin/groups" element={<AdminGroupsPage />} />
                <Route
                  path="/admin/employers"
                  element={<AdminEmployersPage />}
                />
                <Route path="/admin/settings" element={<AdminSettingsPage />} />
              </Route>
            </Route>
          </Routes>
        </TooltipProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
}

export default App;

