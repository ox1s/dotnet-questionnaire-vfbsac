import { type ReactNode, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import toast from "react-hot-toast";
import {
  CalendarRange,
  Columns3,
  Download,
  Filter,
  GitCompareArrows,
  RefreshCw,
} from "lucide-react";
import api, {
  dictionariesApi,
  getApiErrorMessage,
  reportsApi,
  type AnalyticsQuestion,
  type AnalyticsReport,
  type AnalyticsReportRequest,
  type DictionaryItem,
  type FormDetail,
  type StatisticsFilters,
  type TeacherItem,
} from "../api";
import { AdminLayout } from "../layouts/AdminLayout";

type Mode = "single" | "periods" | "groups";
type CompareField =
  | "departmentId"
  | "specialityId"
  | "specializationId"
  | "disciplineId"
  | "teacherId";
type RangeState = { label: string; dateFrom: string; dateTo: string };

const colors = [
  "#0f766e",
  "#2563eb",
  "#f59e0b",
  "#dc2626",
  "#7c3aed",
  "#0891b2",
  "#65a30d",
];

function asDateInput(date: Date) {
  return date.toISOString().slice(0, 10);
}

function getSemesterRange(): RangeState {
  const now = new Date();
  const start =
    now.getMonth() < 6
      ? new Date(now.getFullYear(), 0, 1)
      : new Date(now.getFullYear(), 6, 1);
  return {
    label: "Текущий семестр",
    dateFrom: asDateInput(start),
    dateTo: asDateInput(now),
  };
}

export const AdminStatsPage = () => {
  const { id } = useParams();
  const [form, setForm] = useState<FormDetail | null>(null);
  const [report, setReport] = useState<AnalyticsReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);
  const [specialities, setSpecialities] = useState<DictionaryItem[]>([]);
  const [specializations, setSpecializations] = useState<DictionaryItem[]>([]);
  const [mode, setMode] = useState<Mode>("single");
  const [filters, setFilters] = useState<StatisticsFilters>({});
  const [singleRange, setSingleRange] =
    useState<RangeState>(getSemesterRange());
  const [periods, setPeriods] = useState<RangeState[]>([
    getSemesterRange(),
    {
      label: "Предыдущий период",
      dateFrom: asDateInput(
        new Date(
          new Date().getFullYear(),
          Math.max(new Date().getMonth() - 6, 0),
          1,
        ),
      ),
      dateTo: asDateInput(new Date()),
    },
  ]);
  const [compareField, setCompareField] =
    useState<CompareField>("departmentId");
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  useEffect(() => {
    if (!id) return;
    const run = async () => {
      try {
        const [formRes, tch, dep, disc, spec, specz] = await Promise.all([
          api.get<FormDetail>(`/forms/${id}`),
          dictionariesApi.getTeachers(),
          dictionariesApi.getDepartments(),
          dictionariesApi.getDisciplines(),
          dictionariesApi.getSpecialities(),
          dictionariesApi.getSpecializations(),
        ]);
        setForm(formRes.data);
        setTeachers(tch.data);
        setDepartments(dep.data);
        setDisciplines(disc.data);
        setSpecialities(spec.data);
        setSpecializations(specz.data);
      } catch (error) {
        toast.error(
          getApiErrorMessage(error, "Не удалось загрузить аналитику"),
        );
      } finally {
        setLoading(false);
      }
    };
    run();
  }, [id]);

  const labelFor = (field: CompareField, value: string) => {
    if (field === "teacherId")
      return teachers.find((item) => item.id === value)?.fullName ?? value;
    const sets: Record<Exclude<CompareField, "teacherId">, DictionaryItem[]> = {
      departmentId: departments,
      disciplineId: disciplines,
      specialityId: specialities,
      specializationId: specializations,
    };
    return sets[field].find((item) => item.id === value)?.name ?? value;
  };

  const optionsFor = () => {
    if (compareField === "teacherId")
      return teachers.map((item) => ({ value: item.id, label: item.fullName }));
    const sets: Record<Exclude<CompareField, "teacherId">, DictionaryItem[]> = {
      departmentId: departments,
      disciplineId: disciplines,
      specialityId: specialities,
      specializationId: specializations,
    };
    return sets[compareField].map((item) => ({
      value: item.id,
      label: item.name,
    }));
  };

  const baseFilters = (
    field?: CompareField,
    value?: string,
  ): StatisticsFilters => {
    const next = { ...filters };
    if (field) delete next[field];
    if (field && value) next[field] = value;
    return next;
  };

  const buildRequest = (): AnalyticsReportRequest | null => {
    if (!id) return null;
    if (mode === "single")
      return {
        formId: id,
        slices: [
          {
            label: singleRange.label,
            dateFrom: singleRange.dateFrom,
            dateTo: singleRange.dateTo,
            ...filters,
          },
        ],
      };
    if (mode === "periods")
      return {
        formId: id,
        slices: periods
          .filter((item) => item.dateFrom && item.dateTo)
          .map((item, index) => ({
            label: item.label || `Период ${index + 1}`,
            dateFrom: item.dateFrom,
            dateTo: item.dateTo,
            ...filters,
          })),
      };
    if (selectedIds.length === 0) return null;
    return {
      formId: id,
      slices: selectedIds.map((value) => ({
        label: labelFor(compareField, value),
        dateFrom: singleRange.dateFrom,
        dateTo: singleRange.dateTo,
        ...baseFilters(compareField, value),
      })),
    };
  };

  const loadReport = async () => {
    const request = buildRequest();
    if (!request || request.slices.length === 0) {
      setReport(null);
      return;
    }
    setRefreshing(true);
    try {
      const response = await reportsApi.getAnalytics(request);
      setReport(response.data);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Не удалось построить отчет"));
    } finally {
      setRefreshing(false);
    }
  };

  useEffect(() => {
    if (!loading && form) void loadReport();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, form, mode]);

  const exportReport = async () => {
    const request = buildRequest();
    if (!request) return;
    try {
      const response = await reportsApi.downloadAnalyticsWord(request);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `analytics_${request.formId}.docx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Не удалось экспортировать отчет"));
    }
  };

  const questions = report
    ? [...report.questions].sort((a, b) => a.order - b.order)
    : [];
  const chartData = questions.map((question, index) => {
    const row: Record<string, string | number> = {
      name: `В${index + 1}`,
      fullName: question.questionText,
    };
    question.sliceMetrics.forEach((metric, metricIndex) => {
      row[`slice_${metricIndex}`] = metric.resultScore;
    });
    return row;
  });

  const updateFilter = (field: keyof StatisticsFilters, value: string) =>
    setFilters((previous) => ({ ...previous, [field]: value || undefined }));
  const updatePeriod = (
    index: number,
    field: keyof RangeState,
    value: string,
  ) =>
    setPeriods((previous) =>
      previous.map((item, itemIndex) =>
        itemIndex === index ? { ...item, [field]: value } : item,
      ),
    );

  const renderCards = () =>
    report?.slices.length === 1 ? (
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <MetricCard
          title="Всего анкет"
          value={String(report.slices[0].totalSubmissions)}
        />
        <MetricCard
          title="Средний балл"
          value={report.slices[0].overallAverage.toFixed(2)}
        />
        <MetricCard
          title="Отклонение"
          value={report.slices[0].overallStandardDeviation.toFixed(2)}
        />
      </div>
    ) : (
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-8">
        {report?.slices.map((slice, index) => (
          <MetricCard
            key={`${slice.label}-${index}`}
            title={slice.label}
            value={`${slice.overallAverage.toFixed(2)} / ${slice.totalSubmissions}`}
            subtitle={`${slice.dateFrom.slice(0, 10)} - ${slice.dateTo.slice(0, 10)}`}
          />
        ))}
      </div>
    );

  if (loading)
    return (
      <AdminLayout title="Аналитика" subtitle="Загрузка...">
        <div className="p-10 text-center text-slate-400">Загрузка...</div>
      </AdminLayout>
    );

  return (
    <AdminLayout
      title="Аналитика"
      subtitle={form ? `Отчет по форме: ${form.title}` : "Загрузка..."}
      actions={
        <button
          onClick={exportReport}
          className="flex items-center gap-2 px-5 py-2.5 bg-white border border-slate-200 text-slate-700 rounded-xl hover:bg-slate-50 font-bold shadow-sm text-sm transition-all whitespace-nowrap w-full md:w-auto justify-center"
        >
          <Download size={18} /> Экспорт в Word
        </button>
      }
    >
      <div className="bg-white p-5 rounded-2xl shadow-sm border border-slate-200 mb-6">
        <div className="flex items-center gap-2 mb-4 text-slate-800 font-bold">
          <Filter size={18} />
          <h4>Режим аналитики</h4>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3 mb-6">
          <ModeButton
            active={mode === "single"}
            onClick={() => setMode("single")}
            icon={<CalendarRange size={16} />}
            label="Статистика за период"
          />
          <ModeButton
            active={mode === "periods"}
            onClick={() => setMode("periods")}
            icon={<GitCompareArrows size={16} />}
            label="Сравнение периодов"
          />
          <ModeButton
            active={mode === "groups"}
            onClick={() => setMode("groups")}
            icon={<Columns3 size={16} />}
            label="Сравнение групп"
          />
        </div>

        {mode === "single" || mode === "groups" ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <input
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
              value={singleRange.label}
              onChange={(event) =>
                setSingleRange((previous) => ({
                  ...previous,
                  label: event.target.value,
                }))
              }
            />
            <input
              type="date"
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
              value={singleRange.dateFrom}
              onChange={(event) =>
                setSingleRange((previous) => ({
                  ...previous,
                  dateFrom: event.target.value,
                }))
              }
            />
            <input
              type="date"
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
              value={singleRange.dateTo}
              onChange={(event) =>
                setSingleRange((previous) => ({
                  ...previous,
                  dateTo: event.target.value,
                }))
              }
            />
          </div>
        ) : (
          <div className="space-y-4 mb-6">
            {periods.map((item, index) => (
              <div
                key={`${item.label}-${index}`}
                className="grid grid-cols-1 md:grid-cols-4 gap-4"
              >
                <input
                  className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
                  value={item.label}
                  onChange={(event) =>
                    updatePeriod(index, "label", event.target.value)
                  }
                />
                <input
                  type="date"
                  className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
                  value={item.dateFrom}
                  onChange={(event) =>
                    updatePeriod(index, "dateFrom", event.target.value)
                  }
                />
                <input
                  type="date"
                  className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
                  value={item.dateTo}
                  onChange={(event) =>
                    updatePeriod(index, "dateTo", event.target.value)
                  }
                />
                <button
                  onClick={() =>
                    setPeriods((previous) =>
                      previous.length > 1
                        ? previous.filter((_, itemIndex) => itemIndex !== index)
                        : previous,
                    )
                  }
                  className="px-4 py-2 text-sm font-medium text-slate-600"
                >
                  Удалить
                </button>
              </div>
            ))}
            <button
              onClick={() =>
                setPeriods((previous) => [
                  ...previous,
                  {
                    label: `Период ${previous.length + 1}`,
                    dateFrom: singleRange.dateFrom,
                    dateTo: singleRange.dateTo,
                  },
                ])
              }
              className="px-4 py-2 rounded-lg bg-slate-100 text-slate-700 text-sm font-medium"
            >
              Добавить период
            </button>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
          <select
            className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
            value={filters.teacherId || ""}
            disabled={mode === "groups" && compareField === "teacherId"}
            onChange={(event) => updateFilter("teacherId", event.target.value)}
          >
            <option value="">Все преподаватели</option>
            {teachers.map((item) => (
              <option key={item.id} value={item.id}>
                {item.fullName}
              </option>
            ))}
          </select>
          <select
            className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
            value={filters.disciplineId || ""}
            disabled={mode === "groups" && compareField === "disciplineId"}
            onChange={(event) =>
              updateFilter("disciplineId", event.target.value)
            }
          >
            <option value="">Все дисциплины</option>
            {disciplines.map((item) => (
              <option key={item.id} value={item.id}>
                {item.name}
              </option>
            ))}
          </select>
          <select
            className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
            value={filters.departmentId || ""}
            disabled={mode === "groups" && compareField === "departmentId"}
            onChange={(event) =>
              updateFilter("departmentId", event.target.value)
            }
          >
            <option value="">Все кафедры</option>
            {departments.map((item) => (
              <option key={item.id} value={item.id}>
                {item.name}
              </option>
            ))}
          </select>
          <select
            className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
            value={filters.specialityId || ""}
            disabled={mode === "groups" && compareField === "specialityId"}
            onChange={(event) =>
              updateFilter("specialityId", event.target.value)
            }
          >
            <option value="">Все специальности</option>
            {specialities.map((item) => (
              <option key={item.id} value={item.id}>
                {item.name}
              </option>
            ))}
          </select>
          <select
            className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
            value={filters.specializationId || ""}
            disabled={mode === "groups" && compareField === "specializationId"}
            onChange={(event) =>
              updateFilter("specializationId", event.target.value)
            }
          >
            <option value="">Все специализации</option>
            {specializations.map((item) => (
              <option key={item.id} value={item.id}>
                {item.name}
              </option>
            ))}
          </select>
          <input
            type="text"
            placeholder="Название организации..."
            className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
            value={filters.organizationName || ""}
            onChange={(event) =>
              updateFilter("organizationName", event.target.value)
            }
          />
        </div>

        {mode === "groups" ? (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
            <select
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50"
              value={compareField}
              onChange={(event) => {
                setCompareField(event.target.value as CompareField);
                setSelectedIds([]);
              }}
            >
              {[
                { value: "departmentId", label: "Кафедры" },
                { value: "specialityId", label: "Специальности" },
                { value: "specializationId", label: "Специализации" },
                { value: "disciplineId", label: "Дисциплины" },
                { value: "teacherId", label: "Преподаватели" },
              ].map((item) => (
                <option key={item.value} value={item.value}>
                  {item.label}
                </option>
              ))}
            </select>
            <select
              multiple
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 min-h-36"
              value={selectedIds}
              onChange={(event) =>
                setSelectedIds(
                  Array.from(event.target.selectedOptions).map(
                    (item) => item.value,
                  ),
                )
              }
            >
              {optionsFor().map((item) => (
                <option key={item.value} value={item.value}>
                  {item.label}
                </option>
              ))}
            </select>
          </div>
        ) : null}

        <div className="flex justify-end">
          <button
            onClick={() => void loadReport()}
            disabled={refreshing}
            className="flex items-center gap-2 px-5 py-2 bg-primary text-white rounded-lg text-sm font-bold shadow-sm disabled:opacity-70"
          >
            {refreshing ? (
              <RefreshCw className="animate-spin" size={16} />
            ) : null}{" "}
            Обновить аналитику
          </button>
        </div>
      </div>

      {refreshing ? (
        <div className="flex justify-center p-10 text-slate-400">
          <RefreshCw className="animate-spin" size={24} />
        </div>
      ) : null}
      {report ? (
        renderCards()
      ) : (
        <div className="p-10 text-center text-slate-400 bg-white rounded-2xl border border-slate-200">
          Настройте период и срезы для аналитики.
        </div>
      )}

      {report && chartData.length > 0 ? (
        <div className="bg-white p-4 md:p-6 rounded-2xl shadow-sm border border-slate-200 mb-8">
          <h3 className="text-base md:text-lg font-bold text-slate-900 mb-6">
            Сравнение итоговых баллов по вопросам
          </h3>
          <div className="h-80 w-full min-w-70">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={chartData}
                margin={{ top: 10, right: 10, left: -25, bottom: 0 }}
              >
                <CartesianGrid
                  strokeDasharray="3 3"
                  vertical={false}
                  stroke="#e2e8f0"
                />
                <XAxis
                  dataKey="name"
                  axisLine={false}
                  tickLine={false}
                  tick={{ fill: "#64748b", fontSize: 10 }}
                  dy={10}
                />
                <YAxis
                  axisLine={false}
                  tickLine={false}
                  tick={{ fill: "#64748b", fontSize: 10 }}
                  domain={[0, 10]}
                  ticks={[0, 2, 4, 6, 8, 10]}
                />
                <Tooltip
                  formatter={(value, name) => [
                    Number(value ?? 0).toFixed(2),
                    name,
                  ]}
                  labelFormatter={(_label, payload) =>
                    payload?.[0]?.payload?.fullName ?? "Вопрос"
                  }
                />
                {report.slices.map((slice, index) => (
                  <Bar
                    key={`${slice.label}-${index}`}
                    dataKey={`slice_${index}`}
                    name={slice.label}
                    fill={colors[index % colors.length]}
                    radius={[4, 4, 0, 0]}
                    maxBarSize={40}
                  />
                ))}
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      ) : null}

      {report ? (
        <QuestionsTable questions={questions} slices={report.slices} />
      ) : null}
    </AdminLayout>
  );
};

function ModeButton({
  active,
  onClick,
  icon,
  label,
}: {
  active: boolean;
  onClick: () => void;
  icon: ReactNode;
  label: string;
}) {
  return (
    <button
      onClick={onClick}
      className={`px-4 py-3 rounded-xl text-sm font-bold border transition-colors flex items-center justify-center gap-2 ${active ? "bg-primary text-white border-primary" : "bg-slate-50 text-slate-700 border-slate-200"}`}
    >
      {icon}
      {label}
    </button>
  );
}

function MetricCard({
  title,
  value,
  subtitle,
}: {
  title: string;
  value: string;
  subtitle?: string;
}) {
  return (
    <div className="bg-white p-5 rounded-2xl shadow-sm border border-slate-200">
      <p className="text-xs font-bold uppercase tracking-wider text-slate-500 mb-2">
        {title}
      </p>
      <p className="text-3xl font-bold text-slate-900">{value}</p>
      {subtitle ? (
        <p className="text-xs text-slate-400 mt-2">{subtitle}</p>
      ) : null}
    </div>
  );
}

function QuestionsTable({
  questions,
  slices,
}: {
  questions: AnalyticsQuestion[];
  slices: AnalyticsReport["slices"];
}) {
  return (
    <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden mb-8">
      <div className="px-4 py-4 border-b border-slate-100">
        <h3 className="text-base md:text-lg font-bold text-slate-900">
          Детализация
        </h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse min-w-150">
          <thead>
            <tr className="bg-slate-50/50">
              <th className="py-3 px-4 text-xs font-bold text-slate-500 uppercase w-12 text-center">
                №
              </th>
              <th className="py-3 px-4 text-xs font-bold text-slate-500 uppercase">
                Вопрос
              </th>
              {slices.length === 1 ? (
                <>
                  <th className="py-3 px-4 text-xs font-bold text-slate-500 uppercase text-right">
                    Среднее
                  </th>
                  <th className="py-3 px-4 text-xs font-bold text-slate-500 uppercase text-right">
                    Итог
                  </th>
                  <th className="py-3 px-4 text-xs font-bold text-slate-500 uppercase text-right">
                    Sigma
                  </th>
                </>
              ) : (
                slices.map((slice, index) => (
                  <th
                    key={`${slice.label}-${index}`}
                    className="py-3 px-4 text-xs font-bold text-slate-500 uppercase text-right"
                  >
                    {slice.label}
                  </th>
                ))
              )}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {questions.map((question, index) => (
              <tr key={question.questionId} className="hover:bg-slate-50">
                <td className="py-3 px-4 text-center text-sm">{index + 1}</td>
                <td className="py-3 px-4 text-sm font-medium text-slate-900">
                  {question.questionText}
                </td>
                {slices.length === 1 ? (
                  <>
                    <td className="py-3 px-4 text-right font-mono text-sm text-slate-500">
                      {question.sliceMetrics[0]?.averageScore.toFixed(2) ?? "-"}
                    </td>
                    <td className="py-3 px-4 text-right font-mono text-sm font-bold text-primary">
                      {question.sliceMetrics[0]?.resultScore.toFixed(2) ?? "-"}
                    </td>
                    <td className="py-3 px-4 text-right font-mono text-sm text-slate-400">
                      {question.sliceMetrics[0]?.standardDeviation.toFixed(2) ??
                        "-"}
                    </td>
                  </>
                ) : (
                  question.sliceMetrics.map((metric, metricIndex) => (
                    <td
                      key={`${question.questionId}-${metricIndex}`}
                      className="py-3 px-4 text-right font-mono text-sm"
                    >
                      {metric.resultScore.toFixed(2)}
                    </td>
                  ))
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
