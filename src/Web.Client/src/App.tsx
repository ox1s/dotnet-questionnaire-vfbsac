import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "@/components/ui/sonner";
import { LoginPage } from "./pages/LoginPage";
import { DashboardPage } from "./pages/DashboardPage";
import { SurveyPage } from "./pages/SurveyPage";
import { AdminStatsPage } from "./pages/AdminStatsPage";
import { CreateFormPage } from "./pages/CreateFormPage";
import { AdminTeachersPage } from "./pages/AdminTeachersPage";
import { AdminDisciplinesPage } from "./pages/AdminDisciplinesPage";
import { AdminDepartmentsPage } from "./pages/AdminDepartmentsPage";
import { AdminGroupsPage } from "./pages/AdminGroupsPage";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { AdminSettingsPage } from "./pages/AdminSettingsPage";
import { AdminSpecialitiesPage } from "./pages/AdminSpecialitiesPage";
import { AdminSpecializationsPage } from "./pages/AdminSpecializationsPage";
import { TooltipProvider } from "@/components/ui/tooltip";
import { ThemeProvider } from "@/components/theme-provider";

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
              <Route path="/admin/settings" element={<AdminSettingsPage />} />
            </Route>
          </Routes>
        </TooltipProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
}

export default App;
