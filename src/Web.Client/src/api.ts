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
  specialityId?: string;
  isDeleted?: boolean;
}

export interface TeacherItem {
  id: string;
  fullName: string;
  departmentId?: string;
  isDeleted?: boolean;
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
    departmentId?: string;
    specialityId?: string;
    specializationId?: string;
    organizationName?: string;
    educationForm?: string;
    employeeCategory?: string;
    position?: string;
  };
}

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

export interface StatisticsFilters {
  disciplineId?: string;
  teacherId?: string;
  departmentId?: string;
  specialityId?: string;
  specializationId?: string;
  organizationName?: string;
  educationForm?: string;
  employeeCategory?: string;
  position?: string;
}

// New API types
export interface QuestionStatistics {
  questionId: string;
  questionText: string;
  median: number;
  mean: number;
  mode: number;
  standardDeviation: number;
  responseCount: number;
}

export interface AnalyticsByPeriodRequest {
  formId: string;
  fromDate: string;
  toDate: string;
  filterSet: StatisticsFilters;
}

export interface AnalyticsByPeriodResponse {
  questionId: string;
  questionText: string;
  median: number;
  mean: number;
  mode: number;
  standardDeviation: number;
  responseCount: number;
}

export interface PeriodRequest {
  label: string;
  dateFrom: string;
  dateTo: string;
  filterSet: StatisticsFilters;
}

export interface GetAnalyticsByPeriodsRequest {
  formId: string;
  periods: PeriodRequest[];
}

export interface PeriodAnalyticsResponse {
  label: string;
  periodStart: string;
  periodEnd: string;
  questionStatistics: QuestionStatistics[];
  totalSubmissions?: number;
  overallAverage?: number;
  overallStandardDeviation?: number;
}

export interface GetAnalyticsByGroupsRequest {
  formId: string;
  fromDate: string;
  toDate: string;
  groupBy: 'Department' | 'Discipline' | 'Speciality' | 'Specialization' | 'EducationForm' | 'EmployeeCategory' | 'Teacher';
  filterSet: StatisticsFilters;
}

export interface GroupAnalyticsResponse {
  groupKey: string;
  groupName: string;
  questionStatistics: QuestionStatistics[];
}

// Old API types (deprecated)
export interface AnalyticsSliceRequest extends StatisticsFilters {
  label: string;
  dateFrom: string;
  dateTo: string;
}

export interface AnalyticsReportRequest {
  formId: string;
  slices: AnalyticsSliceRequest[];
}

export interface AnalyticsSlice {
  label: string;
  dateFrom: string;
  dateTo: string;
  totalSubmissions: number;
  overallAverage: number;
  overallStandardDeviation: number;
  filters: StatisticsFilters;
}

export interface AnalyticsQuestionSliceMetric {
  sliceLabel: string;
  averageScore: number;
  resultScore: number;
  standardDeviation: number;
  submissionCount: number;
}

export interface AnalyticsQuestion {
  questionId: string;
  questionText: string;
  questionType: string;
  order: number;
  sliceMetrics: AnalyticsQuestionSliceMetric[];
}

export interface AnalyticsReport {
  formId: string;
  formTitle: string;
  slices: AnalyticsSlice[];
  questions: AnalyticsQuestion[];
}

export interface AdviceItem {
  text: string;
  teacherId?: string;
  departmentId?: string;
}

export const usersApi = {
  createGroup: (groupName: string, password: string) =>
    api.post<string>("/users/groups", { groupName, password }),
  createStaff: (login: string, displayName: string, password: string) =>
    api.post<string>("/users/staff", { login, displayName, password }),

  getStaff: () => api.get<GroupUser[]>("/users/staff"),
  getGroups: () => api.get<GroupUser[]>("/users/groups"),

  deleteUser: (id: string) => api.delete(`/users/${id}`),

  updateUser: (id: string, login: string, displayName: string) =>
    api.put(`/users/${id}`, { login, displayName }),

  setPassword: (id: string, newPassword: string) =>
    api.post(`/users/${id}/set-password`, { userId: id, newPassword }),
};

export const settingsApi = {
  closeSemester: () => api.post("/settings/close-semester"),
  openSemester: () => api.post("/settings/open-semester"),
};

export const dictionariesApi = {
  getDepartments: () => api.get<DictionaryItem[]>("/dictionaries/departments"),
  getTeachers: () => api.get<TeacherItem[]>("/dictionaries/teachers"),
  getDisciplines: () => api.get<DictionaryItem[]>("/dictionaries/disciplines"),
  getSpecialities: () =>
    api.get<DictionaryItem[]>("/dictionaries/specialities"),
  getSpecializations: () =>
    api.get<DictionaryItem[]>("/dictionaries/specializations"),

  createTeacher: (fullName: string, departmentId?: string) =>
    api.post<string>("/teachers", { fullName, departmentId }),
  createDiscipline: (name: string, departmentId: string) =>
    api.post<string>("/disciplines", { name, departmentId }),
  createDepartment: (name: string) =>
    api.post<string>("/departments", { name }),
  createSpeciality: (name: string) =>
    api.post<string>("/specialities", { name }),
  createSpecialization: (name: string, specialityId: string) =>
    api.post<string>("/specializations", { name, specialityId }),

  deleteDepartment: (id: string) => api.delete(`/departments/${id}`),
  deleteTeacher: (id: string) => api.delete(`/teachers/${id}`),
  deleteDiscipline: (id: string) => api.delete(`/disciplines/${id}`),
  deleteSpeciality: (id: string) => api.delete(`/specialities/${id}`),
  deleteSpecialization: (id: string) => api.delete(`/specializations/${id}`),

  restoreDepartment: (id: string) => api.post(`/departments/${id}/restore`),
  restoreTeacher: (id: string) => api.post(`/teachers/${id}/restore`),
  restoreDiscipline: (id: string) => api.post(`/disciplines/${id}/restore`),
  restoreSpeciality: (id: string) => api.post(`/specialities/${id}/restore`),
  restoreSpecialization: (id: string) =>
    api.post(`/specializations/${id}/restore`),

  updateDepartment: (id: string, name: string) =>
    api.put(`/departments/${id}`, { departmentId: id, name }),
  updateTeacher: (id: string, fullName: string, departmentId?: string) =>
    api.put(`/teachers/${id}`, { fullName, departmentId }),
  updateDiscipline: (id: string, name: string, departmentId: string) =>
    api.put(`/disciplines/${id}`, { name, departmentId }),
  updateSpeciality: (id: string, name: string) =>
    api.put(`/specialities/${id}`, { name }),
  updateSpecialization: (id: string, name: string, specialityId: string) =>
    api.put(`/specializations/${id}`, { name, specialityId }),
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

export const reportsApi = {
  // New analytics endpoints
  getAnalyticsByPeriod: (payload: AnalyticsByPeriodRequest) =>
    api.post<AnalyticsByPeriodResponse[]>("/reports/analytics/period", payload),

  getAnalyticsByPeriods: (payload: GetAnalyticsByPeriodsRequest) =>
    api.post<PeriodAnalyticsResponse[]>("/reports/analytics/periods", payload),

  getAnalyticsByGroups: (payload: GetAnalyticsByGroupsRequest) =>
    api.post<GroupAnalyticsResponse[]>("/reports/analytics/groups", payload),

  // Export endpoints
  exportAnalyticsByPeriod: (payload: AnalyticsByPeriodRequest) =>
    api.post("/reports/analytics/period/export", payload, {
      responseType: "blob",
    }),

  exportAnalyticsByPeriods: (payload: GetAnalyticsByPeriodsRequest) =>
    api.post("/reports/analytics/periods/export", payload, {
      responseType: "blob",
    }),

  exportAnalyticsByGroups: (payload: GetAnalyticsByGroupsRequest) =>
    api.post("/reports/analytics/groups/export", payload, {
      responseType: "blob",
    }),

  // Old endpoint (deprecated)
  getAnalytics: (payload: AnalyticsReportRequest) =>
    api.post<AnalyticsReport>("/reports/analytics", payload),

  getAdvices: (formId: string, teacherId?: string) =>
    api.get<AdviceItem[]>(`/reports/forms/${formId}/advices`, {
      params: { teacherId },
    }),
};

export const getApiErrorMessage = (
  error: unknown,
  fallback: string = "Произошла ошибка",
) => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | {
          detail?: string;
          title?: string;
          errors?: Record<string, string[]>;
        }
      | undefined;

    if (data?.detail) {
      return data.detail;
    }

    if (data?.errors) {
      const firstErrorGroup = Object.values(data.errors)[0];
      if (firstErrorGroup?.[0]) {
        return firstErrorGroup[0];
      }
    }

    if (data?.title) {
      return data.title;
    }
  }

  return fallback;
};

export default api;
