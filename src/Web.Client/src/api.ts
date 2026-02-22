import axios from "axios";
import { getDeviceId } from "./utils/device";

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
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  },
);
export interface DictionaryItem {
  id: string;
  name: string;
  departmentId?: string;
}

export interface TeacherItem {
  id: string;
  fullName: string;
  departmentId: string;
}

export interface GroupUser {
  id: string;
  login: string;
  displayName: string;
}
export interface SubmissionListItem {
  id: string;
  formId: string;
  submittedAt: string;
  context: {
    disciplineId?: string;
    teacherId?: string;
    departmenId?: string;
    specialityId?: string;
    specializationId?: string;
    organizationName?: string;
    educationForm?: string;
    employeeCategory?: string;
    position?: string;
  };
}

export const usersApi = {
  createGroup: (groupName: string, password: string) =>
    api.post<string>("/users/groups", { groupName, password }),

  getGroups: () => api.get<GroupUser[]>("/users/groups"),

  deleteUser: (id: string) => api.delete(`/users/${id}`),

  updateUser: (id: string, login: string, displayName: string) =>
    api.put(`/users/${id}`, { login, displayName }),

  setPassword: (id: string, newPassword: string) =>
    api.post(`/users/${id}/set-password`, { userId: id, newPassword }),
};

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
  deleteDepartment: (id: string) => api.delete(`/departments/${id}`),
  deleteTeacher: (id: string) => api.delete(`/teachers/${id}`),
  deleteDiscipline: (id: string) => api.delete(`/disciplines/${id}`),

  updateDepartment: (id: string, name: string) =>
    api.put(`/departments/${id}`, { departmentId: id, name }),

  updateTeacher: (id: string, fullName: string, departmentId: string) =>
    api.put(`/teachers/${id}`, { fullName, departmentId }),

  updateDiscipline: (id: string, name: string, departmentId: string) =>
    api.put(`/disciplines/${id}`, { name, departmentId }),
};

export const submissionsApi = {
  getMyList: () => {
    const user = JSON.parse(atob(localStorage.getItem("token")!.split(".")[1]));
    const userId = user.sub;
    const deviceId = getDeviceId();
    return api.get<SubmissionListItem[]>(
      `/submissions?userId=${userId}&deviceId=${deviceId}`,
    );
  },
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
