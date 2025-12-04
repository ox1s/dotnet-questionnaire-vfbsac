import React from 'react';
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Typography, IconButton } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import type { Question } from '../../types/survey';

interface QuestionsTableProps {
    questions: Question[];
    onDelete: (id: number) => void; // Callback для удаления
}

const numberToQuestionTypeMap: { [key: number]: string } = {
    0: 'Оценка',
    1: 'Текст',
    2: 'Выбор',
};

const QuestionsTable: React.FC<QuestionsTableProps> = ({ questions, onDelete }) => {
    return (
        <TableContainer component={Paper}>
            <Table>
                <TableHead>
                    <TableRow>
                        <TableCell>ID</TableCell>
                        <TableCell>Текст вопроса</TableCell>
                        <TableCell>Тип</TableCell>
                        <TableCell align="right">Действия</TableCell>
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
                                <TableCell>{numberToQuestionTypeMap[q.type]}</TableCell>
                                <TableCell align="right">
                                    <IconButton onClick={() => onDelete(q.id)} color="error">
                                        <DeleteIcon />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))
                    )}
                </TableBody>
            </Table>
        </TableContainer>
    );
};

export default QuestionsTable;