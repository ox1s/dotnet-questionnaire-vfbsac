import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "@/components/ui/sonner";
import { LoginPage } from "./pages/auth/login-page";
import { DashboardPage } from "./pages/dashboard/dashboard-page";
import { SurveyPage } from "./pages/forms/survey-page";
import { AdminStatsPage } from "./pages/admin/admin-stats-page";
import { AdminFormPreviewPage } from "./pages/admin/admin-form-preview-page";
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
import { AppShell } from "./components/layout/app-shell";
import { AuthSessionListener } from "./components/auth/auth-session-listener";

// Split out from App so the route tree reads on its own, without the provider
// stack around it. Behaviour is covered end-to-end by e2e/protected-routes.spec.ts
// and e2e/sidebar-navigation.spec.ts.
const AppRoutes = () => (
  <Routes>
    <Route path="/login" element={<LoginPage />} />
    <Route path="/" element={<Navigate to="/dashboard" replace />} />

    <Route element={<ProtectedRoute />}>
      {/* Standalone screen: no sidebar, so it stays outside the shell. */}
      <Route path="/form/:id" element={<SurveyPage />} />

      {/* Every sidebar-bearing screen shares this one layout mount. Adding a
          route here rather than wrapping the page in its own <AdminLayout/> is
          what keeps the sidebar alive across navigation. */}
      <Route element={<AppShell />}>
        <Route path="/dashboard" element={<DashboardPage />} />

        <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
          <Route path="/admin/stats/:id" element={<AdminStatsPage />} />
          <Route path="/admin/preview/:id" element={<AdminFormPreviewPage />} />
          <Route path="/admin/create-form" element={<CreateFormPage />} />
          <Route path="/admin/teachers" element={<AdminTeachersPage />} />
          <Route path="/admin/disciplines" element={<AdminDisciplinesPage />} />
          <Route path="/admin/departments" element={<AdminDepartmentsPage />} />
          <Route path="/admin/specialities" element={<AdminSpecialitiesPage />} />
          <Route
            path="/admin/specializations"
            element={<AdminSpecializationsPage />}
          />
          <Route path="/admin/groups" element={<AdminGroupsPage />} />
          <Route path="/admin/employers" element={<AdminEmployersPage />} />
          <Route path="/admin/settings" element={<AdminSettingsPage />} />
        </Route>
      </Route>
    </Route>

    <Route path="*" element={<Navigate to="/dashboard" replace />} />
  </Routes>
);

function App() {
  return (
    <BrowserRouter>
      <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
        <TooltipProvider delayDuration={0}>
          <AuthSessionListener />
          <Toaster position="top-center" />
          <AppRoutes />
        </TooltipProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
}

export default App;
