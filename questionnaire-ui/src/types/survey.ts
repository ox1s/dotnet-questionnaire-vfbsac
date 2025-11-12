export interface Question {
    id: number;
    text: string;
    type: number; // 0 = Rating, 1 = Text, 2 = Choice
    options: QuestionOption[];
}

export interface QuestionOption {
    id: number;
    text: string;
}

export type QuestionType = 'Rating' | 'Text' | 'Choice';

// Тип для анкеты в списке
export interface Survey {
    id: number;
    name: string;
    isActive: boolean;
}

// Тип для полной анкеты с вопросами
export interface SurveyDetail extends Survey {
    questions: Question[];
}

// Типы для отправки ответа
export interface AnswerDetail {
    questionId: number;
    weight?: number;
    mark?: number;
    textResponse?: string;
}

export interface SubmitSurveyPayload {
    formId: number;
    details: AnswerDetail[];
}