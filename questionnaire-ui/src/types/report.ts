export interface SummaryReport {
formId: number;
formName: string;
totalSubmissions: number;
questions: QuestionSummary[];
}

export interface QuestionSummary {
questionId: number;
questionText: string;
questionType: number; // 0=Rating, 1=Text, 2=Choice
ratingData: RatingSummaryData | null;
textData: string[] | null;
choiceData: ChoiceSummaryData[] | null;
}

export interface RatingSummaryData {
averageMark: number;
averageWeight: number;
responseCount: number;
}

export interface ChoiceSummaryData {
optionId: number;
optionText: string;
selectedCount: number;
}