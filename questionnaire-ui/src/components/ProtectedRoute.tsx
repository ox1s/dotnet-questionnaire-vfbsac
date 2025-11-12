import React from 'react';
import { useAuthStore } from '../store/authStore';
import { Navigate, Outlet } from 'react-router-dom';

const ProtectedRoute: React.FC = () => {
    const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

    // Если пользователь аутентифицирован, показываем дочерний компонент (страницу).
    // Иначе — перенаправляем на страницу входа.
    return isAuthenticated ? <Outlet /> : <Navigate to="/login" />;
};

export default ProtectedRoute;