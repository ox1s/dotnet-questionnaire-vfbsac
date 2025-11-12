import React, { useEffect, useState } from 'react';
import { Container, Typography, CircularProgress, Alert, Box, Button } from '@mui/material';
import type { Question } from '../types/survey';
import { getAllQuestions, createQuestion } from '../api/adminService';
import type { CreateQuestionPayload } from '../api/adminService';
import QuestionForm from '../components/admin/QuestionForm';
import QuestionsTable from '../components/admin/QuestionsTable';

const AdminQuestionsPage: React.FC = () => {
    const [questions, setQuestions] = useState<Question[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [isFormOpen, setIsFormOpen] = useState(false);

    const fetchQuestions = async () => {
        try {
            setIsLoading(true);
            const data = await getAllQuestions();
            setQuestions(data);
        } catch (err) {
            setError('Не удалось загрузить вопросы.');
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchQuestions();
    }, []);

    const handleCreateQuestion = async (payload: CreateQuestionPayload) => {
        try {
            await createQuestion(payload);
            setIsFormOpen(false); // Закрываем форму после успеха
            await fetchQuestions(); // Обновляем список
        } catch (err) {
            // Здесь можно добавить более детальную обработку ошибок формы
            alert('Ошибка при создании вопроса');
        }
    };

    if (isLoading) return <CircularProgress />;

    return (
        <Container maxWidth="lg" sx={{ mt: 4 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h4">Управление вопросами</Typography>
                <Button variant="contained" onClick={() => setIsFormOpen(true)}>
                    Создать вопрос
                </Button>
            </Box>

            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

            {isFormOpen && (
                <QuestionForm 
                    onSubmit={handleCreateQuestion}
                    onCancel={() => setIsFormOpen(false)}
                />
            )}

            <QuestionsTable questions={questions} />
        </Container>
    );
};

export default AdminQuestionsPage;