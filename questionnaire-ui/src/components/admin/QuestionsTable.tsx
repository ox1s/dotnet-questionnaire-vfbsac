import React from 'react';
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Typography } from '@mui/material';
import type { Question } from '../../types/survey';
import { QuestionType } from '../../types/survey';

interface QuestionsTableProps {
    questions: Question[];
}

const QuestionTypeMap: { [key in QuestionType]: string } = {
    [QuestionType.Rating]: 'Оценка',
    [QuestionType.Text]: 'Текст',
    [QuestionType.Choice]: 'Выбор',
};

const QuestionsTable: React.FC<QuestionsTableProps> = ({ questions }) => {
    return (
        <TableContainer component={Paper}>
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell>ID</TableCell>
                        <TableCell>Текст вопроса</TableCell>
                        <TableCell>Тип</TableCell>
                        <TableCell>Действия</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {questions.length === 0 ? (
                        <TableRow>
                            <TableCell colSpan={4} align="center">
                                <Typography>Вопросы не найдены.</Typography>
                            </TableCell>
                        </TableRow>
                    ) : (
                        questions.map((q) => (
                            <TableRow key={q.id}>
                                <TableCell>{q.id}</TableCell>
                                <TableCell>{q.text}</TableCell>
                                <TableCell>{QuestionTypeMap[q.type]}</TableCell>
                                <TableCell>{/* Кнопки Edit/Delete будут здесь */}</TableCell>
                            </TableRow>
                        ))
                    )}
                </TableBody>
            </Table>
        </TableContainer>
    );
};

export default QuestionsTable;