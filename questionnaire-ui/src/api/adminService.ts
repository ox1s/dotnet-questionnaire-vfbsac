import apiClient from './axios';
import type { Question, Survey, SurveyDetail } from '../types/survey';
import type { SummaryReport } from '../types/report';

// Тип для создания вопроса
export interface CreateQuestionPayload {
    text: string;
    type: number;
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

// Получить все анкеты (краткая инфо)
export const getAllForms = async (): Promise<Survey[]> => {
    const response = await apiClient.get<Survey[]>('/forms');
    return response.data;
};

// Создать новую анкету
export const createForm = async (name: string): Promise<Survey> => {
    const response = await apiClient.post<Survey>('/forms', { name });
    return response.data;
};

// Получить анкету по ID (детальная инфо)
export const getFormById = async (id: number): Promise<SurveyDetail> => {
    const response = await apiClient.get<SurveyDetail>(`/forms/${id}`);
    return response.data;
};

// Добавить вопрос в анкету
export const addQuestionToForm = async (formId: number, questionId: number, order: number): Promise<void> => {
    await apiClient.post(`/forms/${formId}/questions/${questionId}`, { order });
};

// --- Функции для отчетов ---
export const getSummaryReport = async (formId: number): Promise<SummaryReport> => {
    const response = await apiClient.get<SummaryReport>(`/reports/summary/${formId}`);
    return response.data;
};

// --- Функции удаления ---

// Удалить вопрос по ID
export const deleteQuestion = async (id: number): Promise<void> => {
    await apiClient.delete(`/admin/questions/${id}`);
};

// Удалить анкету по ID
export const deleteForm = async (id: number): Promise<void> => {
    await apiClient.delete(`/forms/${id}`);
};

// Удалить вопрос из анкеты
export const removeQuestionFromForm = async (formId: number, questionId: number): Promise<void> => {
    await apiClient.delete(`/forms/${formId}/questions/${questionId}`);
};