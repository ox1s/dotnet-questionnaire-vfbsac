import React from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import { Box, Container, Paper, Tabs, Tab } from '@mui/material';

const AdminLayout: React.FC = () => {
    return (
        <Container maxWidth="lg" sx={{ mt: 4 }}>
            <Paper>
                <Tabs value={location.pathname}>
                    <Tab 
                        label="Управление Вопросами" 
                        value="/admin/questions" 
                        component={NavLink} 
                        to="/admin/questions" 
                    />
                    <Tab 
                        label="Управление Анкетами" 
                        value="/admin/forms" 
                        component={NavLink} 
                        to="/admin/forms" 
                    />
                </Tabs>
            </Paper>
            <Box sx={{ mt: 4 }}>
                <Outlet /> {/* Здесь будут рендериться дочерние страницы */}
            </Box>
        </Container>
    );
};

export default AdminLayout;