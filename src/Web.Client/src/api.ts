import axios from "axios";

const api = axios.create({
  baseURL: "/api",
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export interface DictionaryItem {
  id: string;
  name: string; // Для Department, Discipline, Speciality
}

export interface TeacherItem {
  id: string;
  fullName: string;
  departmentId: string;
}

export const dictionariesApi = {
  getDepartments: () => api.get<DictionaryItem[]>("/dictionaries/departments"),
  getTeachers: () => api.get<TeacherItem[]>("/dictionaries/teachers"),
  getDisciplines: () => api.get<DictionaryItem[]>("/dictionaries/disciplines"),
  createTeacher: (fullName: string, departmentId: string) =>
    api.post<string>("/teachers", { fullName, departmentId }),
  createDiscipline: (name: string, departmentId: string) =>
    api.post<string>("/disciplines", { name, departmentId }),
  createDepartment: (name: string) =>
    api.post<string>("/departments", { name }),
};

export default api;

export interface Form {
  id: string;
  title: string;
  requiredFilters: string[];
}

export interface Question {
  id: string;
  text: string;
  type: string;
  order: number;
}

export interface FormDetail extends Form {
  questions: Question[];
}
export interface Statistics {
  formId: string;
  totalSubmissions: number;
  averageScores: number[]; // Среднее арифметическое
  resultScores: number[]; // Взвешенная оценка (Итоговая)
  standardDeviations: number[]; // Отклонение
  overallAverage: number;
  overallStandardDeviation: number;
}

export const reportsApi = {
  getStatistics: (formId: string) =>
    api.get<Statistics>(`/submissions/statistics?formId=${formId}`),

  downloadWordReport: (formId: string) =>
    api.get(`/reports/word/${formId}`, { responseType: "blob" }),
};
