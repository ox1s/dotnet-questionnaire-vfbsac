import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Button, Box, TextField } from '@mui/material';
import type { Survey } from '../types/survey';
import { getAllForms, createForm } from '../api/adminService';

const AdminFormsPage: React.FC = () => {
    const [forms, setForms] = useState<Survey[]>([]);
    const [newFormName, setNewFormName] = useState('');
    const navigate = useNavigate();

    const fetchForms = async () => {
        const data = await getAllForms();
        setForms(data);
    };

    useEffect(() => {
        fetchForms();
    }, []);

    const handleCreate = async () => {
        if (!newFormName.trim()) return;
        await createForm(newFormName);
        setNewFormName('');
        await fetchForms();
    };

    return (
        <>
            <Box component="form" sx={{ display: 'flex', gap: 2, mb: 4 }}>
                <TextField
                    label="Название новой анкеты"
                    value={newFormName}
                    onChange={(e) => setNewFormName(e.target.value)}
                    fullWidth
                />
                <Button variant="contained" onClick={handleCreate}>Создать</Button>
            </Box>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>ID</TableCell>
                            <TableCell>Название</TableCell>
                            <TableCell>Активна</TableCell>
                            <TableCell>Действия</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {forms.map((form) => (
                            <TableRow key={form.id}>
                                <TableCell>{form.id}</TableCell>
                                <TableCell>{form.name}</TableCell>
                                <TableCell>{form.isActive ? 'Да' : 'Нет'}</TableCell>
                                <TableCell>
                                    <Button onClick={() => navigate(`/admin/forms/${form.id}`)}>
                                        Редактировать
                                    </Button>
                                </TableCell>
                                <Button
                                    variant="outlined"
                                    onClick={() => navigate(`/admin/reports/${form.id}`)}
                                >
                                    Отчет
                                </Button>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </>
    );
};

export default AdminFormsPage;