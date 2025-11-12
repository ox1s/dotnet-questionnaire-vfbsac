import { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import ProtectedRoute from './components/ProtectedRoute';
import SurveyPage from './pages/SurveyPage';
import { Box, CircularProgress, CssBaseline } from '@mui/material';
import AdminQuestionsPage from './pages/AdminQuestionsPage';
import AdminRoute from './components/AdminRoute';
import AdminLayout from './components/admin/AdminLayout';
import AdminFormsPage from './pages/AdminFormsPage';
import AdminFormDetailPage from './pages/AdminFormDetailPage';
import AdminReportPage from './pages/AdminReportPage';

function App() {
  const initialize = useAuthStore((state) => state.initialize);
  const isLoading = useAuthStore((state) => state.isLoading);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  useEffect(() => {
    initialize();
  }, []);

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <>
      <CssBaseline />
      <Router>
        <Routes>
          <Route path="/login" element={isAuthenticated ? <Navigate to="/" /> : <LoginPage />} />

          <Route element={<ProtectedRoute />}>
            {/* --- Пользовательские роуты --- */}
            <Route path="/" element={<DashboardPage />} />
            <Route path="/surveys/:id" element={<SurveyPage />} />

            {/* --- Админские роуты --- */}
            {/* Сначала проверяем, что пользователь админ */}
            <Route path="/admin" element={<AdminRoute />}>
              {/* Если проверка пройдена, рендерим AdminLayout, который содержит Outlet для дочерних роутов */}
              <Route element={<AdminLayout />}>
                {/* Redirect с /admin на /admin/questions для удобства */}
                <Route index element={<Navigate to="/admin/questions" replace />} />
                <Route path="questions" element={<AdminQuestionsPage />} />
                <Route path="forms" element={<AdminFormsPage />} />
                <Route path="forms/:id" element={<AdminFormDetailPage />} />
                <Route path="reports/:id" element={<AdminReportPage />} />
              </Route>
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </Router>
    </>
  );
}

export default App;