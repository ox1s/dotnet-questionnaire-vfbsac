import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Box, Typography, Paper, CircularProgress, Alert } from '@mui/material';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { getSummaryReport } from '../api/adminService';
import type { SummaryReport, QuestionSummary } from '../types/report';

const AdminReportPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const [report, setReport] = useState<SummaryReport | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        if (!id) return;
        getSummaryReport(Number(id))
            .then(setReport)
            .finally(() => setIsLoading(false));
    }, [id]);

    if (isLoading) return <CircularProgress />;
    if (!report) return <Alert severity="error">Не удалось загрузить отчет.</Alert>;

    const renderQuestionReport = (q: QuestionSummary) => {
        switch (q.questionType) {
            case 0: // Rating
                return (
                    <Box>
                        <Typography>Средняя оценка: <strong>{q.ratingData?.averageMark.toFixed(2)}</strong></Typography>
                        <Typography>Средний вес: <strong>{q.ratingData?.averageWeight.toFixed(2)}</strong></Typography>
                        <Typography>Кол-во ответов: <strong>{q.ratingData?.responseCount}</strong></Typography>
                    </Box>
                );
            case 2: // Choice
                return (
                    <Box sx={{ height: 300, width: '100%' }}>
                        <ResponsiveContainer>
                            <BarChart data={q.choiceData || []}>
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis dataKey="optionText" />
                                <YAxis allowDecimals={false} />
                                <Tooltip />
                                <Legend />
                                <Bar dataKey="selectedCount" fill="#8884d8" name="Кол-во выборов" />
                            </BarChart>
                        </ResponsiveContainer>
                    </Box>
                );
            case 1: // Text
                return (
                    <Box>
                        {q.textData?.map((text, index) => (
                            <Typography key={index} sx={{ borderBottom: '1px solid #eee', py: 1 }}>- {text}</Typography>
                        ))}
                    </Box>
                );
            default:
                return null;
        }
    };

    return (
        <Box>
            <Typography variant="h4" gutterBottom>Отчет по анкете: {report.formName}</Typography>
            <Typography variant="h6" gutterBottom>Всего прохождений: {report.totalSubmissions}</Typography>

            {report.questions.map(q => (
                <Paper key={q.questionId} sx={{ p: 2, my: 2 }}>
                    <Typography variant="h6" gutterBottom>{q.questionText}</Typography>
                    {renderQuestionReport(q)}
                </Paper>
            ))}
        </Box>
    );
};

export default AdminReportPage;