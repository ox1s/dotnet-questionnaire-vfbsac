import apiClient from './axios';
import type { Survey, SurveyDetail, SubmitSurveyPayload } from '../types/survey';

export const getAvailableSurveys = async (): Promise<Survey[]> => {
    try {
        const response = await apiClient.get<Survey[]>('/surveys');
        return response.data;
    } catch (error) {
        console.error('Failed to fetch available surveys:', error);
        throw error;
    }
};
export const getSurveyById = async (id: number): Promise<SurveyDetail> => {
    try {
        const response = await apiClient.get<SurveyDetail>(`/forms/${id}`);
        return response.data;
    } catch (error) {
        console.error(`Failed to fetch survey with id ${id}:`, error);
        throw error;
    }
};

export const submitSurvey = async (payload: SubmitSurveyPayload): Promise<void> => {
    try {
        await apiClient.post('/surveys/submit', payload);
    } catch (error) {
        console.error('Failed to submit survey:', error);
        throw error;
    }
};