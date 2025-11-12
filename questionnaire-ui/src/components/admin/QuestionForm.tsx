import React, { useState } from 'react';
import { Box, Button, TextField, Select, MenuItem, FormControl, InputLabel, Typography } from '@mui/material';
import { QuestionType } from '../../types/survey';
import type { CreateQuestionPayload } from '../../api/adminService';

interface QuestionFormProps {
    onSubmit: (payload: CreateQuestionPayload) => void;
    onCancel: () => void;
}

const QuestionForm: React.FC<QuestionFormProps> = ({ onSubmit, onCancel }) => {
    const [text, setText] = useState('');
    const [type, setType] = useState<QuestionType>(QuestionType.Text);
    const [options, setOptions] = useState<string>(''); // Храним как строку, разделенную переносами

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const payload: CreateQuestionPayload = {
            text,
            type,
            options: type === QuestionType.Choice ? options.split('\n').filter(opt => opt.trim() !== '') : undefined,
        };
        onSubmit(payload);
    };

    return (
        <Box component="form" onSubmit={handleSubmit} sx={{ mb: 4, p: 2, border: '1px solid #ccc', borderRadius: 1 }}>
            <Typography variant="h6" gutterBottom>Новый вопрос</Typography>
            <TextField
                label="Текст вопроса"
                fullWidth
                required
                value={text}
                onChange={(e) => setText(e.target.value)}
                sx={{ mb: 2 }}
            />
            <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Тип вопроса</InputLabel>
                <Select
                    value={type}
                    label="Тип вопроса"
                    onChange={(e) => setType(e.target.value as QuestionType)}
                >
                    <MenuItem value={QuestionType.Rating}>Оценка с весом</MenuItem>
                    <MenuItem value={QuestionType.Text}>Текстовый ответ</MenuItem>
                    <MenuItem value={QuestionType.Choice}>Выбор вариантов</MenuItem>
                </Select>
            </FormControl>

            {type === QuestionType.Choice && (
                <TextField
                    label="Варианты ответа (каждый с новой строки)"
                    fullWidth
                    multiline
                    rows={4}
                    value={options}
                    onChange={(e) => setOptions(e.target.value)}
                    sx={{ mb: 2 }}
                />
            )}

            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                <Button onClick={onCancel}>Отмена</Button>
                <Button type="submit" variant="contained">Сохранить</Button>
            </Box>
        </Box>
    );
};

export default QuestionForm;