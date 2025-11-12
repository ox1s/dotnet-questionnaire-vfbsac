import { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import LoginPage from './pages/LoginPage';
import DashboardPage  from './pages/DashboardPage';
import ProtectedRoute from './components/ProtectedRoute';
import SurveyPage from './pages/SurveyPage'
import { Box, CircularProgress, CssBaseline } from '@mui/material';
import AdminQuestionsPage from './pages/AdminQuestionsPage';
import AdminRoute from './components/AdminRoute';

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
                    
                    {/* Все защищенные роуты находятся внутри ОДНОГО ProtectedRoute */}
                    <Route element={<ProtectedRoute />}>
                        {/* Роуты для всех аутентифицированных пользователей */}
                        <Route path="/" element={<DashboardPage />} />
                        <Route path="/surveys/:id" element={<SurveyPage />} />

                        {/* Роуты ТОЛЬКО для админов, вложенные в AdminRoute */}
                        <Route element={<AdminRoute />}>
                            <Route path="/admin/questions" element={<AdminQuestionsPage />} />
                            {/* Здесь будут другие админские роуты, например /admin/forms */}
                        </Route>
                    </Route>

                    {/* Если пользователь залогинен, но ввел несуществующий URL, перенаправляем на главную */}
                    <Route path="*" element={<Navigate to="/" />} />
                </Routes>
            </Router>
        </>
    );
}

export default App;