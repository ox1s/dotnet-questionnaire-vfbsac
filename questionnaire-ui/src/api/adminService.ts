import apiClient from './axios';
import { QuestionType } from '../types/survey';
import type { Question } from '../types/survey';

// Тип для создания вопроса
export interface CreateQuestionPayload {
    text: string;
    type: QuestionType;
    options?: string[];
}

// Получить все вопросы
export const getAllQuestions = async (): Promise<Question[]> => {
    const response = await apiClient.get<Question[]>('/admin/questions');
    return response.data;
};

// Создать новый вопрос
export const createQuestion = async (payload: CreateQuestionPayload): Promise<Question> => {
    const response = await apiClient.post<Question>('/admin/questions', payload);
    return response.data;
};

// TODO: Добавить updateQuestion и deleteQuestion в будущем