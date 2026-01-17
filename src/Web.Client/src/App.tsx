import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { LoginPage } from "./pages/LoginPage";
import { DashboardPage } from "./pages/DashboardPage";
import { SurveyPage } from "./pages/SurveyPage";
import { AdminStatsPage } from "./pages/AdminStatsPage";
import { CreateFormPage } from "./pages/CreateFormPage";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/form/:id" element={<SurveyPage />} />
        <Route path="/admin/stats/:id" element={<AdminStatsPage />} />
        <Route path="/admin/create-form" element={<CreateFormPage />} />

        <Route path="/" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
