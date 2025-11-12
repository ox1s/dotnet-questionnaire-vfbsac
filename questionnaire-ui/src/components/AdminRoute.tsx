import React from 'react';
import { useAuthStore } from '../store/authStore';
import { Navigate, Outlet } from 'react-router-dom';

const AdminRoute: React.FC = () => {
    const roles = useAuthStore((state) => state.roles);
    const isAdmin = roles.includes('admin');

    return isAdmin ? <Outlet /> : <Navigate to="/" />;

    console.log('AdminRoute check: roles =', roles, 'isAdmin =', isAdmin);
};

export default AdminRoute;