import React, { useEffect, useState } from 'react';
import { useAuthStore } from '../store/authStore';
import { Box, Button, Container, Typography, CircularProgress, Alert } from '@mui/material';
import { getAvailableSurveys } from '../api/surveyService';
import type { Survey } from '../types/survey';
import { Link as RouterLink } from 'react-router-dom';

const DashboardPage: React.FC = () => {
    const logout = useAuthStore((state) => state.logout);
    const [surveys, setSurveys] = useState<Survey[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchSurveys = async () => {
            try {
                setIsLoading(true);
                setError(null);
                const data = await getAvailableSurveys();
                setSurveys(data);
            } catch (err) {
                setError('Не удалось загрузить список анкет.');
            } finally {
                setIsLoading(false);
            }
        };

        fetchSurveys();
    }, []);

    const renderContent = () => {
        if (isLoading) {
            return <CircularProgress />;
        }

        if (error) {
            return <Alert severity="error">{error}</Alert>;
        }

        if (surveys.length === 0) {
            return <Typography>Для вас нет доступных анкет.</Typography>;
        }

        return (
            <Box>
                {surveys.map((survey) => (
                    <Box
                        key={survey.id}
                        sx={{
                            p: 2,
                            border: '1px solid #ccc',
                            borderRadius: '4px',
                            mb: 2,
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                        }}
                    >
                        <Typography variant="h6">{survey.name}</Typography>
                        <Button
                            component={RouterLink}
                            to={`/surveys/${survey.id}`}
                            variant="contained"
                        >
                            Пройти
                        </Button>
                    </Box>
                ))}
            </Box>
        );
    };

    return (
        <Container maxWidth="md">
            <Box sx={{ my: 4 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                    <Typography variant="h4" component="h1">
                        Доступные анкеты
                    </Typography>
                    <Button variant="outlined" onClick={logout}>
                        Выйти
                    </Button>
                </Box>
                {renderContent()}
            </Box>
        </Container>
    );
};

export default DashboardPage;