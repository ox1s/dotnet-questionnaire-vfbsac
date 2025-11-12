import React from 'react';
import { useAuthStore } from '../store/authStore';
import { Navigate, Outlet } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';

const AdminRoute: React.FC = () => {
    const isLoading = useAuthStore((state) => state.isLoading);
    const roles = useAuthStore((state) => state.roles);

    if (isLoading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                <CircularProgress />
            </Box>
        );
    }

    const isAdmin = roles.includes('admin');
    
    console.log('AdminRoute check: roles =', roles, 'isAdmin =', isAdmin); 

    return isAdmin ? <Outlet /> : <Navigate to="/" />;
};

export default AdminRoute;