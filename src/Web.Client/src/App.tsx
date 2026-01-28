import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { LoginPage } from "./pages/LoginPage";
import { DashboardPage } from "./pages/DashboardPage";
import { SurveyPage } from "./pages/SurveyPage";
import { AdminStatsPage } from "./pages/AdminStatsPage";
import { CreateFormPage } from "./pages/CreateFormPage";
import { AdminTeachersPage } from "./pages/AdminTeachersPage";
import { AdminDisciplinesPage } from "./pages/AdminDisciplinesPage";
import { AdminDepartmentsPage } from "./pages/AdminDepartmentsPage";
import { AdminGroupsPage } from "./pages/AdminGroupsPage";
import { MySubmissionsPage } from "./pages/MySubmissionsPage"; 
import { Toaster } from "react-hot-toast";

function App() {
  return (
    <BrowserRouter>
      <Toaster position="top-right" toastOptions={{ duration: 4000 }} />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/form/:id" element={<SurveyPage />} />
        <Route path="/admin/stats/:id" element={<AdminStatsPage />} />
        <Route path="/admin/create-form" element={<CreateFormPage />} />
        <Route path="/admin/teachers" element={<AdminTeachersPage />} />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/admin/disciplines" element={<AdminDisciplinesPage />} />
        <Route path="/admin/departments" element={<AdminDepartmentsPage />} />
        <Route path="/admin/groups" element={<AdminGroupsPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
