import React from 'react';
import { Box, FormControl, FormLabel, TextField, Typography, Select, MenuItem, InputLabel } from '@mui/material';
import type { Question, AnswerDetail } from '../types/survey';
import {QuestionType} from '../types/survey';

interface QuestionRendererProps {
    question: Question;
    answer: AnswerDetail;
    onAnswerChange: (questionId: number, newAnswer: Partial<AnswerDetail>) => void;
}

const QuestionRenderer: React.FC<QuestionRendererProps> = ({ question, answer, onAnswerChange }) => {
    
    const handleRatingChange = (field: 'weight' | 'mark', value: number) => {
        const newAnswer: Partial<AnswerDetail> = { [field]: value };

        // Если изменили вес, и текущая оценка стала больше нового веса, сбрасываем оценку
        if (field === 'weight' && answer.mark && answer.mark > value) {
            newAnswer.mark = value;
        }
        onAnswerChange(question.id, newAnswer);
    };

    const renderQuestionType = () => {
        switch (question.type) {
            case QuestionType.Rating:
                // Генерируем опции для выпадающих списков
                const weightOptions = Array.from({ length: 10 }, (_, i) => i + 1); // [1, 2, ..., 10]
                const markOptions = Array.from({ length: (answer.weight || 0) + 1 }, (_, i) => i); // [0, 1, ..., weight]

                return (
                    <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                        {/* Select для "Важности" */}
                        <FormControl sx={{ minWidth: 200 }}>
                            <InputLabel id={`weight-label-${question.id}`}>Важность (макс. оценка)</InputLabel>
                            <Select
                                labelId={`weight-label-${question.id}`}
                                value={answer.weight || ''}
                                label="Важность (макс. оценка)"
                                onChange={(e) => handleRatingChange('weight', Number(e.target.value))}
                            >
                                {weightOptions.map(val => (
                                    <MenuItem key={val} value={val}>{val}</MenuItem>
                                ))}
                            </Select>
                        </FormControl>

                        {/* Select для "Оценки" */}
                        <FormControl sx={{ minWidth: 200 }} disabled={!answer.weight}>
                            <InputLabel id={`mark-label-${question.id}`}>Ваша оценка</InputLabel>
                            <Select
                                labelId={`mark-label-${question.id}`}
                                value={answer.mark ?? ''} // Используем ?? для корректной обработки 0
                                label="Ваша оценка"
                                onChange={(e) => handleRatingChange('mark', Number(e.target.value))}
                            >
                                {markOptions.map(val => (
                                    <MenuItem key={val} value={val}>{val}</MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </Box>
                );
            case QuestionType.Text:
                return (
                    <TextField
                        label="Ваш ответ"
                        fullWidth
                        multiline
                        rows={3}
                        value={answer.textResponse || ''}
                        onChange={(e) => onAnswerChange(question.id, { textResponse: e.target.value })}
                    />
                );
            case QuestionType.Choice:
                return <Typography color="text.secondary">Вопросы с выбором вариантов будут доступны позже.</Typography>;
            default:
                return null;
        }
    };

    return (
        <Box sx={{ mb: 4, p: 2, border: '1px solid #e0e0e0', borderRadius: 1 }}>
            <FormControl component="fieldset" fullWidth>
                <FormLabel component="legend" sx={{ mb: 2, fontSize: '1.1rem' }}>
                    {question.text}
                </FormLabel>
                {renderQuestionType()}
            </FormControl>
        </Box>
    );
};

export default QuestionRenderer;