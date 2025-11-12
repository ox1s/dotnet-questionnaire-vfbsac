import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getSurveyById, submitSurvey } from '../api/surveyService';
import type { SurveyDetail, AnswerDetail } from '../types/survey';
import { Box, Button, Container, Typography, CircularProgress, Alert } from '@mui/material';
import QuestionRenderer from '../components/QuestionRenderer';

const SurveyPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [survey, setSurvey] = useState<SurveyDetail | null>(null);
    const [answers, setAnswers] = useState<{ [key: number]: AnswerDetail }>({});
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!id) return;
        const surveyId = parseInt(id, 10);

        const fetchSurvey = async () => {
            try {
                setIsLoading(true);
                const data = await getSurveyById(surveyId);
                setSurvey(data);
                // Инициализируем состояние ответов
                const initialAnswers: { [key: number]: AnswerDetail } = {};
                data.questions.forEach(q => {
                    initialAnswers[q.id] = { questionId: q.id };
                });
                setAnswers(initialAnswers);
            } catch (err) {
                setError('Не удалось загрузить анкету.');
            } finally {
                setIsLoading(false);
            }
        };

        fetchSurvey();
    }, [id]);

    const handleAnswerChange = (questionId: number, newAnswer: Partial<AnswerDetail>) => {
        setAnswers(prev => ({
            ...prev,
            [questionId]: { ...prev[questionId], ...newAnswer },
        }));
    };

    const handleSubmit = async () => {
        if (!survey) return;
        try {
            await submitSurvey({
                formId: survey.id,
                details: Object.values(answers),
            });
            alert('Спасибо! Ваши ответы приняты.');
            navigate('/');
        } catch (err) {
            setError('Произошла ошибка при отправке ответов.');
        }
    };

    if (isLoading) return <CircularProgress />;
    if (error) return <Alert severity="error">{error}</Alert>;
    if (!survey) return <Typography>Анкета не найдена.</Typography>;

    return (
        <Container maxWidth="lg">
            <Box sx={{ my: 4 }}>
                <Typography variant="h4" component="h1" gutterBottom>
                    {survey.name}
                </Typography>
                <Box>
                    {survey.questions.map(question => (
                        <QuestionRenderer
                            key={question.id}
                            question={question}
                            answer={answers[question.id] || { questionId: question.id }}
                            onAnswerChange={handleAnswerChange}
                        />
                    ))}
                </Box>
                <Button variant="contained" size="large" onClick={handleSubmit} sx={{ mt: 4 }}>
                    Отправить ответы
                </Button>
            </Box>
        </Container>
    );
};

export default SurveyPage;