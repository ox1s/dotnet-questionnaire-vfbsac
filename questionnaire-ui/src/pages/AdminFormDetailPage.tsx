import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Box, Typography, Paper, List, ListItem, ListItemText, Button, Select, MenuItem, FormControl } from '@mui/material';
import type { SurveyDetail, Question } from '../types/survey';
import { getFormById, getAllQuestions, addQuestionToForm } from '../api/adminService';

const AdminFormDetailPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const [form, setForm] = useState<SurveyDetail | null>(null);
    const [allQuestions, setAllQuestions] = useState<Question[]>([]);
    const [selectedQuestionId, setSelectedQuestionId] = useState<number | ''>('');

    const fetchFormDetails = async () => {
        if (!id) return;
        const formData = await getFormById(Number(id));
        setForm(formData);
    };

    useEffect(() => {
        fetchFormDetails();
        // Загружаем все вопросы один раз для выпадающего списка
        getAllQuestions().then(setAllQuestions);
    }, [id]);

    const handleAddQuestion = async () => {
        if (!id || !selectedQuestionId) return;
        const nextOrder = (form?.questions.length || 0) + 1;
        await addQuestionToForm(Number(id), selectedQuestionId, nextOrder);
        setSelectedQuestionId('');
        await fetchFormDetails(); // Обновляем детали анкеты
    };

    // Фильтруем вопросы, чтобы в списке для добавления не было уже добавленных
    const availableQuestions = allQuestions.filter(
        q => !form?.questions.some(fq => fq.id === q.id)
    );

    if (!form) return <Typography>Загрузка...</Typography>;

    return (
        <Box>
            <Typography variant="h4" gutterBottom>Редактирование анкеты: {form.name}</Typography>
            
            <Paper sx={{ p: 2, mb: 4 }}>
                <Typography variant="h6">Добавить вопрос в анкету</Typography>
                <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
                    <FormControl fullWidth>
                        <Select
                            value={selectedQuestionId}
                            onChange={(e) => setSelectedQuestionId(e.target.value as number)}
                            displayEmpty
                        >
                            <MenuItem value="" disabled>Выберите вопрос</MenuItem>
                            {availableQuestions.map(q => (
                                <MenuItem key={q.id} value={q.id}>{q.text}</MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <Button variant="contained" onClick={handleAddQuestion} disabled={!selectedQuestionId}>
                        Добавить
                    </Button>
                </Box>
            </Paper>

            <Paper>
                <List>
                    <ListItem>
                        <ListItemText primary={<Typography fontWeight="bold">Вопросы в анкете</Typography>} />
                    </ListItem>
                    {form.questions.map((question, index) => (
                        <ListItem key={question.id} divider>
                            <ListItemText primary={`${index + 1}. ${question.text}`} />
                            {/* Здесь можно добавить кнопку "Удалить" */}
                        </ListItem>
                    ))}
                </List>
            </Paper>
        </Box>
    );
};

export default AdminFormDetailPage;