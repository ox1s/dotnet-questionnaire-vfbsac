import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts";
import {
  ChartTooltip,
  ChartTooltipContent,
  ChartContainer,
  type ChartConfig,
} from "@/components/ui/chart";
import { FilterSelect } from "@/components/shared/filter-select";
import { toast } from "sonner";
import {
  Download,
  Filter,
  RefreshCw,
  Calendar as CalendarIcon,
} from "lucide-react";
import { format, parseISO } from "date-fns";
import { ru } from "date-fns/locale";

import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import {
  getLinkedFilterOptions,
  sanitizeLinkedFilters,
} from "@/utils/linked-filters";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

// api
import api, {
  dictionariesApi,
  getApiErrorMessage,
  reportsApi,
  type AdviceItem,
  type DictionaryItem,
  type FormDetail,
  type StatisticsFilters,
  type TeacherItem,
  type AnalyticsByPeriodRequest,
  type GetAnalyticsByPeriodsRequest,
  type GetAnalyticsByGroupsRequest,
  type PeriodAnalyticsResponse,
  type SatisfactionRating,
} from "../../api";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { AdminLayout } from "@/components/admin/admin-shared";

type Mode = "single" | "periods" | "groups";
type CompareField =
  | "departmentId"
  | "specialityId"
  | "specializationId"
  | "disciplineId"
  | "teacherId";
type RangeState = {
  id: string;
  label: string;
  dateFrom: string;
  dateTo: string;
};

const colors = [
  "var(--chart-5)",
  "var(--chart-4)",
  "var(--chart-3)",
  "var(--chart-2)",
  "var(--chart-1)",
];

// Table 1 thresholds from "Методика оценки удовлетворенности потребителей" — mirrors
// StatisticsCalculator.ClassifySatisfaction on the backend, used only for the client-computed
// overall (across-questions) rating.
function classifyRating(satisfactionPercentage: number): SatisfactionRating {
  if (satisfactionPercentage < 40) return "Unsatisfactory";
  if (satisfactionPercentage < 60) return "Satisfactory";
  if (satisfactionPercentage < 80) return "Good";
  return "Excellent";
}

const ratingLabels: Record<SatisfactionRating, string> = {
  Excellent: "отлично",
  Good: "хорошо",
  Satisfactory: "удовлетворительно",
  Unsatisfactory: "неудовлетворительно",
};

function asDateInput(date: Date) {
  return format(date, "yyyy-MM-dd");
}

function createRangeState(
  label: string,
  dateFrom: string,
  dateTo: string,
): RangeState {
  return {
    id: crypto.randomUUID(),
    label,
    dateFrom,
    dateTo,
  };
}

function getSemesterRange(): RangeState {
  const now = new Date();
  const start =
    now.getMonth() < 6
      ? new Date(now.getFullYear(), 0, 1)
      : new Date(now.getFullYear(), 6, 1);

  return createRangeState(
    "Текущий семестр",
    asDateInput(start),
    asDateInput(now),
  );
}

function getPreviousPeriodRange(): RangeState {
  const now = new Date();

  return {
    id: crypto.randomUUID(),
    label: "Предыдущий период",
    dateFrom: asDateInput(
      new Date(now.getFullYear(), Math.max(now.getMonth() - 6, 0), 1),
    ),
    dateTo: asDateInput(now),
  };
}

export const AdminStatsPage = () => {
  const { id } = useParams();
  const [form, setForm] = useState<FormDetail | null>(null);
  const [report, setReport] = useState<PeriodAnalyticsResponse[] | null>(null);
  const [advices, setAdvices] = useState<AdviceItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);
  const [specialities, setSpecialities] = useState<DictionaryItem[]>([]);
  const [specializations, setSpecializations] = useState<DictionaryItem[]>([]);
  const [mode, setMode] = useState<Mode>("single");
  const [filters, setFilters] = useState<StatisticsFilters>({});
  const [singleRange, setSingleRange] = useState<RangeState>(getSemesterRange());
  const [periods, setPeriods] = useState<RangeState[]>([
    getSemesterRange(),
    getPreviousPeriodRange(),
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

  useEffect(() => {
    setFilters((previous) =>
      sanitizeLinkedFilters(previous, {
        departments,
        disciplines,
        specialities,
        specializations,
      }),
    );
  }, [departments, disciplines, specialities, specializations]);

  const teacherLabel = (teacher: TeacherItem) => {
    if (!teacher.departmentId) {
      return teacher.fullName;
    }

    const departmentName = departments.find(
      (department) => department.id === teacher.departmentId,
    )?.name;

    return departmentName
      ? `${teacher.fullName} (${departmentName})`
      : teacher.fullName;
  };

  const getTeacherName = (teacherId?: string) =>
    teachers.find((teacher) => teacher.id === teacherId)?.fullName;

  const getDepartmentName = (departmentId?: string) =>
    departments.find((department) => department.id === departmentId)?.name;

  const optionsFor = () => {
    const filtersWithoutCompareField = baseFilters(compareField);
    const compareOptions = getLinkedFilterOptions(filtersWithoutCompareField, {
      departments,
      disciplines,
      specialities,
      specializations,
    });

    if (compareField === "teacherId") {
      // Filter teachers by department if department filter is set
      const filteredTeachers = filtersWithoutCompareField.departmentId
        ? teachers.filter((t) => t.departmentId === filtersWithoutCompareField.departmentId)
        : teachers;
      
      return filteredTeachers.map((item) => ({
        value: item.id,
        label: teacherLabel(item),
      }));
    }
    
    const sets: Record<Exclude<CompareField, "teacherId">, DictionaryItem[]> = {
      departmentId: compareOptions.departments,
      disciplineId: compareOptions.disciplines,
      specialityId: compareOptions.specialities,
      specializationId: compareOptions.specializations,
    };
    return sets[compareField].map((item) => ({
      value: item.id,
      label: item.name,
    }));
  };

  useEffect(() => {
    // Don't clear selectedIds when filters change in groups mode
    // Users should be able to compare groups even if they're filtered out
    if (mode !== "groups") return;
    
    const allowedIds = new Set(optionsFor().map((item) => item.value));
    setSelectedIds((previous) =>
      previous.filter((value) => allowedIds.has(value)),
    );
  }, [compareField, departments, disciplines, specialities, specializations]);

  const baseFilters = (
    field?: CompareField,
    value?: string,
  ): StatisticsFilters => {
    const next = { ...filters };
    if (field) delete next[field];
    if (field && value) next[field] = value;
    return next;
  };

  const buildRequest = ():
    | AnalyticsByPeriodRequest
    | GetAnalyticsByPeriodsRequest
    | GetAnalyticsByGroupsRequest
    | null => {
    if (!id) return null;

    if (mode === "single") {
      return {
        formId: id,
        fromDate: new Date(singleRange.dateFrom).toISOString(),
        toDate: new Date(singleRange.dateTo).toISOString(),
        filterSet: filters,
      } satisfies AnalyticsByPeriodRequest;
    }

    if (mode === "periods") {
      const validPeriods = periods.filter(
        (item) => item.dateFrom && item.dateTo,
      );
      if (validPeriods.length === 0) return null;

      return {
        formId: id,
        periods: validPeriods.map((item) => ({
          label: item.label,
          dateFrom: new Date(item.dateFrom).toISOString(),
          dateTo: new Date(item.dateTo).toISOString(),
          filterSet: filters,
        })),
      } satisfies GetAnalyticsByPeriodsRequest;
    }

    if (selectedIds.length === 0) return null;

    const groupByMapping: Record<CompareField, GetAnalyticsByGroupsRequest["groupBy"]> = {
      departmentId: "Department",
      disciplineId: "Discipline",
      specialityId: "Speciality",
      specializationId: "Specialization",
      teacherId: "Teacher",
    };

    return {
      formId: id,
      fromDate: new Date(singleRange.dateFrom).toISOString(),
      toDate: new Date(singleRange.dateTo).toISOString(),
      groupBy: groupByMapping[compareField],
      filterSet: filters,
    } satisfies GetAnalyticsByGroupsRequest;
  };

  const loadReport = async () => {
    const request = buildRequest();
    if (!request || !id) {
      // In groups mode, if no groups selected, show empty report instead of null
      if (mode === "groups" && id) {
        setReport([]);
      } else {
        setReport(null);
      }
      setAdvices([]);
      return;
    }

    setRefreshing(true);
    try {
      let reportResponse: { data: PeriodAnalyticsResponse[] };

      if (mode === "single") {
        const singleResponse = await reportsApi.getAnalyticsByPeriod(
          request as AnalyticsByPeriodRequest,
        );
        const stats = singleResponse.data;
        const totalSubmissions = stats.length > 0 ? stats[0].responseCount : 0;
        // Formula (6)/(5): overall % and average deviation are the mean across questions
        const overallSatisfactionPercentage = stats.length > 0
          ? stats.reduce((sum, q) => sum + q.satisfactionPercentage, 0) / stats.length
          : 0;
        const overallStandardDeviation = stats.length > 0
          ? stats.reduce((sum, q) => sum + q.standardDeviation, 0) / stats.length
          : 0;

        reportResponse = {
          data: [
            {
              label: singleRange.label,
              periodStart: singleRange.dateFrom,
              periodEnd: singleRange.dateTo,
              questionStatistics: stats,
              totalSubmissions,
              overallSatisfactionPercentage,
              overallStandardDeviation,
              overallRating: classifyRating(overallSatisfactionPercentage),
            },
          ],
        };
      } else if (mode === "periods") {
        reportResponse = await reportsApi.getAnalyticsByPeriods(
          request as GetAnalyticsByPeriodsRequest,
        );
        // Add computed overall metrics
        reportResponse.data = reportResponse.data.map(period => {
          const stats = period.questionStatistics;
          const totalSubmissions = stats.length > 0 ? stats[0].responseCount : 0;
          const overallSatisfactionPercentage = stats.length > 0
            ? stats.reduce((sum, q) => sum + q.satisfactionPercentage, 0) / stats.length
            : 0;
          const overallStandardDeviation = stats.length > 0
            ? stats.reduce((sum, q) => sum + q.standardDeviation, 0) / stats.length
            : 0;
          return {
            ...period,
            totalSubmissions,
            overallSatisfactionPercentage,
            overallStandardDeviation,
            overallRating: classifyRating(overallSatisfactionPercentage),
          };
        });
      } else {
        const groupsResponse = await reportsApi.getAnalyticsByGroups(
          request as GetAnalyticsByGroupsRequest,
        );
        
        // Filter groups by selectedIds
        const filteredGroups = groupsResponse.data.filter((item) =>
          selectedIds.includes(item.groupKey)
        );
        
        reportResponse = {
          data: filteredGroups.map((item) => {
            const stats = item.questionStatistics;
            const totalSubmissions = stats.length > 0 ? stats[0].responseCount : 0;
            const overallSatisfactionPercentage = stats.length > 0
              ? stats.reduce((sum, q) => sum + q.satisfactionPercentage, 0) / stats.length
              : 0;
            const overallStandardDeviation = stats.length > 0
              ? stats.reduce((sum, q) => sum + q.standardDeviation, 0) / stats.length
              : 0;

            return {
              label: item.groupName,
              periodStart: singleRange.dateFrom,
              periodEnd: singleRange.dateTo,
              questionStatistics: stats,
              totalSubmissions,
              overallSatisfactionPercentage,
              overallStandardDeviation,
              overallRating: classifyRating(overallSatisfactionPercentage),
            };
          }),
        };
      }

      const advicesResponse = await reportsApi.getAdvices(
        id,
        filters.teacherId,
      );

      setReport(reportResponse.data);
      setAdvices(
        filters.departmentId
          ? advicesResponse.data.filter(
              (item) => item.departmentId === filters.departmentId,
            )
          : advicesResponse.data,
      );
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
    if (!request) {
      toast.error("Настройте параметры отчета перед экспортом");
      return;
    }

    try {
      let response;
      let filename = `analytics-${format(new Date(), "yyyy-MM-dd")}.docx`;

      if (mode === "single") {
        response = await reportsApi.exportAnalyticsByPeriod(
          request as AnalyticsByPeriodRequest,
        );
        filename = `analytics-period-${format(new Date(), "yyyy-MM-dd")}.docx`;
      } else if (mode === "periods") {
        response = await reportsApi.exportAnalyticsByPeriods(
          request as GetAnalyticsByPeriodsRequest,
        );
        filename = `analytics-periods-${format(new Date(), "yyyy-MM-dd")}.docx`;
      } else {
        response = await reportsApi.exportAnalyticsByGroups(
          request as GetAnalyticsByGroupsRequest,
        );
        filename = `analytics-groups-${format(new Date(), "yyyy-MM-dd")}.docx`;
      }

      // Create download link
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", filename);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);

      toast.success("Отчет успешно экспортирован");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Не удалось экспортировать отчет"));
    }
  };

  const questions = report && report.length > 0
    ? report[0].questionStatistics
    : [];
  const availableLinkedOptions = getLinkedFilterOptions(filters, {
    departments,
    disciplines,
    specialities,
    specializations,
  });
  const chartData = questions.map((question, index) => {
    const row: Record<string, string | number> = {
      name: `В${index + 1}`,
      fullName: question.questionText,
    };
    report?.forEach((period, periodIndex) => {
      const metric = period.questionStatistics.find((q) => q.questionId === question.questionId);
      row[`slice_${periodIndex}`] = metric?.satisfactionPercentage ?? 0;
    });
    return row;
  });

  const updateFilter = (field: keyof StatisticsFilters, value: string) =>
    setFilters((previous) =>
      sanitizeLinkedFilters(
        { ...previous, [field]: value || undefined },
        {
          departments,
          disciplines,
          specialities,
          specializations,
        },
      ),
    );
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
  const chartConfig = {
    desktop: {
      label: "Desktop",
      color: "#2563eb",
    },
    mobile: {
      label: "Mobile",
      color: "#60a5fa",
    },
  } satisfies ChartConfig;
  const compareFieldOptions: { value: CompareField; label: string }[] = [
    { value: "departmentId", label: "Кафедры" },
    { value: "specialityId", label: "Специальности" },
    { value: "specializationId", label: "Специализации" },
    { value: "disciplineId", label: "Дисциплины" },
    { value: "teacherId", label: "Преподаватели" },
  ];

  const renderCards = () =>
    report?.length === 1 ? (
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <Card>
          <CardHeader>
            <CardTitle>Всего анкет</CardTitle>
          </CardHeader>
          <CardContent className="text-2xl">
            {String(report[0].totalSubmissions ?? 0)}
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Удовлетворенность потребителей, %</CardTitle>
          </CardHeader>
          <CardContent className="text-2xl">
            {(report[0].overallSatisfactionPercentage ?? 0).toFixed(2)}
            <span className="ml-2 text-sm font-normal text-muted-foreground">
              {ratingLabels[report[0].overallRating ?? "Unsatisfactory"]}
            </span>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Отклонение</CardTitle>
          </CardHeader>
          <CardContent className="text-2xl">
            ± {(report[0].overallStandardDeviation ?? 0).toFixed(2)}
          </CardContent>
        </Card>
      </div>
    ) : (
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-8">
        {report?.map((period, index) => (
          <Card key={`${period.label}-${index}`}>
            <CardHeader>
              <CardTitle>{period.label}</CardTitle>
            </CardHeader>
            <CardContent className="text-2xl">
              {`${(period.overallSatisfactionPercentage ?? 0).toFixed(2)}% / ${period.totalSubmissions ?? 0}`}
            </CardContent>
            <CardFooter className="text-xs">
              {`${period.periodStart.slice(0, 10)} - ${period.periodEnd.slice(0, 10)}`}
            </CardFooter>
          </Card>
        ))}
      </div>
    );

  return (
    <AdminLayout
      title="Дашборд"
      subtitle={
        loading
          ? "Загрузка..."
          : form
            ? `Отчет по форме: ${form.title}`
            : "Форма не найдена"
      }
      actions={
        !loading && form ? (
          <Button onClick={exportReport}>
            <Download size={16} className="mr-2" />
            <span className="hidden md:inline">Экспорт в Word</span>
          </Button>
        ) : undefined
      }
    >
      <div className="flex flex-1 flex-col gap-4 bg-background">
        {loading ? (
          <div className="flex h-full min-h-[50vh] items-center justify-center text-muted-foreground">
            <RefreshCw className="animate-spin mr-2" size={24} /> Загрузка...
          </div>
        ) : (
          <>
            <div className="mb-6 border bg-card p-6 shadow-sm">
              <div className="mb-5 flex items-center gap-2 font-bold text-foreground">
                <Filter size={18} />
                <h4>Режим аналитики</h4>
              </div>
              <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-3">
                <Button
                  variant={mode === "single" ? "default" : "outline"}
                  onClick={() => setMode("single")}
                  size="lg"
                >
                  Статистика за период
                </Button>
                <Button
                  variant={mode === "periods" ? "default" : "outline"}
                  onClick={() => setMode("periods")}
                  size="lg"
                >
                  Сравнение периодов
                </Button>
                <Button
                  variant={mode === "groups" ? "default" : "outline"}
                  onClick={() => setMode("groups")}
                  size="lg"
                >
                  Сравнение групп
                </Button>
              </div>

              {mode === "single" || mode === "groups" ? (
                <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-3">
                  <AnalyticsField label="Название периода">
                    <Input
                      className="w-full text-sm"
                      value={singleRange.label}
                      placeholder="Название периода"
                      onChange={(event) =>
                        setSingleRange((previous) => ({
                          ...previous,
                          label: event.target.value,
                        }))
                      }
                    />
                  </AnalyticsField>
                  <AnalyticsField label="Дата начала">
                    <DatePickerInput
                      value={singleRange.dateFrom}
                      onChange={(val) =>
                        setSingleRange((previous) => ({
                          ...previous,
                          dateFrom: val,
                        }))
                      }
                    />
                  </AnalyticsField>
                  <AnalyticsField label="Дата окончания">
                    <DatePickerInput
                      value={singleRange.dateTo}
                      onChange={(val) =>
                        setSingleRange((previous) => ({
                          ...previous,
                          dateTo: val,
                        }))
                      }
                    />
                  </AnalyticsField>
                </div>
              ) : (
                <div className="mb-6 space-y-4">
                  {periods.map((item, index) => (
                    <div
                      key={item.id}
                      className="grid grid-cols-1 gap-4 md:grid-cols-4"
                    >
                      <AnalyticsField label="Название периода">
                        <Input
                          className="bg-background"
                          value={item.label}
                          placeholder={`Период ${index + 1}`}
                          onChange={(event) =>
                            updatePeriod(index, "label", event.target.value)
                          }
                        />
                      </AnalyticsField>
                      <AnalyticsField label="Дата начала">
                        <DatePickerInput
                          value={item.dateFrom}
                          onChange={(val) =>
                            updatePeriod(index, "dateFrom", val)
                          }
                        />
                      </AnalyticsField>
                      <AnalyticsField label="Дата окончания">
                        <DatePickerInput
                          value={item.dateTo}
                          onChange={(val) => updatePeriod(index, "dateTo", val)}
                        />
                      </AnalyticsField>
                      <AnalyticsField label="Действие">
                        <Button
                          variant="outline"
                          onClick={() =>
                            setPeriods((previous) =>
                              previous.length > 1
                                ? previous.filter(
                                    (_, itemIndex) => itemIndex !== index,
                                  )
                                : previous,
                            )
                          }
                          className="w-full justify-center"
                        >
                          Удалить
                        </Button>
                      </AnalyticsField>
                    </div>
                  ))}
                  <Button
                    variant="outline"
                    className="w-full sm:w-auto"
                    onClick={() =>
                      setPeriods((previous) => [
                        ...previous,
                        createRangeState(
                          `Период ${previous.length + 1}`,
                          singleRange.dateFrom,
                          singleRange.dateTo,
                        ),
                      ])
                    }
                  >
                    Добавить период
                  </Button>
                </div>
              )}

              <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
                <AnalyticsField label="Преподаватель">
                  <FilterSelect
                    value={filters.teacherId}
                    onChange={(val) => updateFilter("teacherId", val)}
                    disabled={mode === "groups" && compareField === "teacherId"}
                    placeholder="Все преподаватели"
                    options={teachers.map((t) => ({
                      id: t.id,
                      label: teacherLabel(t),
                    }))}
                  />
                </AnalyticsField>

                <AnalyticsField label="Предмет">
                  <FilterSelect
                    value={filters.disciplineId}
                    onChange={(val) => updateFilter("disciplineId", val)}
                    disabled={
                      mode === "groups" && compareField === "disciplineId"
                    }
                    placeholder="Все дисциплины"
                    options={availableLinkedOptions.disciplines.map((d) => ({
                      id: d.id,
                      label: d.name,
                    }))}
                  />
                </AnalyticsField>

                <AnalyticsField label="Филиал кафедры">
                  <FilterSelect
                    value={filters.departmentId}
                    onChange={(val) => updateFilter("departmentId", val)}
                    disabled={
                      mode === "groups" && compareField === "departmentId"
                    }
                    placeholder="Все кафедры"
                    options={availableLinkedOptions.departments.map((d) => ({
                      id: d.id,
                      label: d.name,
                    }))}
                  />
                </AnalyticsField>

                <AnalyticsField label="Специальность">
                  <FilterSelect
                    value={filters.specialityId}
                    onChange={(val) => updateFilter("specialityId", val)}
                    disabled={
                      mode === "groups" && compareField === "specialityId"
                    }
                    placeholder="Все специальности"
                    options={availableLinkedOptions.specialities.map((s) => ({
                      id: s.id,
                      label: s.name,
                    }))}
                  />
                </AnalyticsField>

                <AnalyticsField label="Специализация">
                  <FilterSelect
                    value={filters.specializationId}
                    onChange={(val) => updateFilter("specializationId", val)}
                    disabled={
                      mode === "groups" && compareField === "specializationId"
                    }
                    placeholder="Все специализации"
                    options={availableLinkedOptions.specializations.map((s) => ({
                      id: s.id,
                      label: s.name,
                    }))}
                  />
                </AnalyticsField>

                <AnalyticsField label="Организация">
                  <Input
                    type="text"
                    className="w-full text-sm"
                    placeholder="Название организации..."
                    value={filters.organizationName || ""}
                    onChange={(event) =>
                      updateFilter("organizationName", event.target.value)
                    }
                  />
                </AnalyticsField>
              </div>

              {mode === "groups" ? (
                <div className="mb-6 grid grid-cols-1 gap-4 xl:grid-cols-3">
                  <AnalyticsField
                    label="Поле сравнения"
                    className="xl:col-span-1"
                  >
                    <Select
                      value={compareField}
                      onValueChange={(value) => {
                        setCompareField(value as CompareField);
                        setSelectedIds([]);
                      }}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Выберите поле" />
                      </SelectTrigger>
                      <SelectContent position="popper">
                        {compareFieldOptions.map((item) => (
                          <SelectItem key={item.value} value={item.value}>
                            {item.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </AnalyticsField>

                  <AnalyticsField
                    label="Сравниваемые значения"
                    className="xl:col-span-2"
                  >
                    <Card className="gap-3 border bg-muted/40 py-3">
                      <CardContent className="space-y-2">
                        {optionsFor().length > 0 ? (
                          optionsFor().map((item) => {
                            const selected = selectedIds.includes(item.value);

                            return (
                              <Button
                                key={item.value}
                                type="button"
                                variant={selected ? "default" : "outline"}
                                className="w-full justify-start"
                                onClick={() =>
                                  setSelectedIds((previous) =>
                                    previous.includes(item.value)
                                      ? previous.filter(
                                          (current) => current !== item.value,
                                        )
                                      : [...previous, item.value],
                                  )
                                }
                              >
                                {item.label}
                              </Button>
                            );
                          })
                        ) : (
                          <p className="text-xs text-muted-foreground">
                            Нет доступных значений для выбранного поля.
                          </p>
                        )}
                      </CardContent>
                    </Card>
                  </AnalyticsField>
                </div>
              ) : null}

              <div className="flex justify-end pt-2">
                <Button
                  onClick={() => void loadReport()}
                  disabled={refreshing}
                  size="lg"
                  variant="default"
                >
                  {refreshing ? (
                    <RefreshCw className="animate-spin mr-2" size={16} />
                  ) : null}{" "}
                  Обновить аналитику
                </Button>
              </div>
            </div>

            {refreshing ? (
              <div className="flex justify-center p-10 text-muted-foreground">
                <RefreshCw className="animate-spin" size={24} />
              </div>
            ) : null}
            {report !== null && report.length > 0 ? (
              renderCards()
            ) : report !== null && report.length === 0 ? (
              <div className="border bg-card p-10 text-center text-muted-foreground">
                {mode === "groups" 
                  ? "Выберите группы для сравнения из списка выше."
                  : "Нет данных для отображения."}
              </div>
            ) : (
              <div className="border bg-card p-10 text-center text-muted-foreground">
                Настройте период и срезы для аналитики.
              </div>
            )}

            {report && report.length > 0 && chartData.length > 0 ? (
              <div>
                <ChartContainer config={chartConfig} className="h-50 w-full">
                  <BarChart
                    data={chartData}
                    margin={{ top: 10, right: 10, left: -25, bottom: 0 }}
                  >
                    <CartesianGrid
                      strokeDasharray="3 3"
                      vertical={false}
                      stroke="var(--border)"
                    />
                    <XAxis
                      dataKey="name"
                      axisLine={false}
                      tickLine={false}
                      tick={{ fill: "var(--muted-foreground)", fontSize: 10 }}
                      dy={10}
                    />
                    <YAxis
                      axisLine={false}
                      tickLine={false}
                      tick={{ fill: "var(--muted-foreground)", fontSize: 10 }}
                      domain={[0, 100]}
                      ticks={[0, 20, 40, 60, 80, 100]}
                    />
                    <ChartTooltip
                      cursor={true}
                      content={
                        <ChartTooltipContent
                          labelKey="fullName"
                          formatter={(value, name) => [
                            <b>{name}</b>,
                            " ",
                            Number(value ?? 0).toFixed(2),
                          ]}
                        />
                      }
                    />
                    {report.map((period, index) => (
                      <Bar
                        key={`${period.label}-${index}`}
                        dataKey={`slice_${index}`}
                        name={period.label}
                        fill={colors[index % colors.length]}
                        radius={[4, 4, 0, 0]}
                        maxBarSize={40}
                      />
                    ))}
                  </BarChart>
                </ChartContainer>
              </div>
            ) : null}

            {report && report.length > 0 ? (
              <QuestionsTable questions={questions} periods={report} />
            ) : null}

            {report && report.length > 0 ? (
              <AdvicesSection
                advices={advices}
                teacherFilterId={filters.teacherId}
                departmentFilterId={filters.departmentId}
                getTeacherName={getTeacherName}
                getDepartmentName={getDepartmentName}
              />
            ) : null}
          </>
        )}
      </div>
    </AdminLayout>
  );
};

function DatePickerInput({
  value,
  onChange,
}: {
  value: string;
  onChange: (val: string) => void;
}) {
  const date = value ? parseISO(value) : undefined;

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          className={cn(
            "w-full justify-start bg-background text-left font-normal",
            !date && "text-muted-foreground",
          )}
        >
          <CalendarIcon className="mr-2 h-4 w-4" />
          {date ? (
            format(date, "PPP", { locale: ru })
          ) : (
            <span>Выберите дату</span>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0">
        <Calendar
          mode="single"
          selected={date}
          onSelect={(d) => onChange(d ? format(d, "yyyy-MM-dd") : "")}
          initialFocus
        />
      </PopoverContent>
    </Popover>
  );
}

function AdvicesSection({
  advices,
  teacherFilterId,
  departmentFilterId,
  getTeacherName,
  getDepartmentName,
}: {
  advices: AdviceItem[];
  teacherFilterId?: string;
  departmentFilterId?: string;
  getTeacherName: (teacherId?: string) => string | undefined;
  getDepartmentName: (departmentId?: string) => string | undefined;
}) {
  const showTeacher = !departmentFilterId || Boolean(teacherFilterId);
  const showDepartment = !teacherFilterId || Boolean(departmentFilterId);

  return (
    <div className="mb-8 overflow-hidden border bg-card shadow-sm">
      <div className="border-b px-4 py-4">
        <h3 className="text-base font-bold text-foreground md:text-lg">
          Текстовые ответы
        </h3>
      </div>

      {advices.length > 0 ? (
        <div className="space-y-4 p-4">
          {advices.map((advice, index) => {
            const teacherName = getTeacherName(advice.teacherId);
            const departmentName = getDepartmentName(advice.departmentId);

            return (
              <Card key={`${advice.text}-${index}`} className="gap-3 py-5">
                <CardContent className="space-y-3">
                  <div className="flex flex-wrap gap-2">
                    {showTeacher && teacherName ? (
                      <Badge variant="secondary">{teacherName}</Badge>
                    ) : null}
                    {showDepartment && departmentName ? (
                      <Badge variant="outline">{departmentName}</Badge>
                    ) : null}
                  </div>
                  <p className="text-sm leading-6 text-foreground whitespace-pre-wrap">
                    {advice.text}
                  </p>
                </CardContent>
              </Card>
            );
          })}
        </div>
      ) : (
        <div className="p-10 text-center text-muted-foreground">
          Текстовые ответы не найдены.
        </div>
      )}
    </div>
  );
}

function AnalyticsField({
  label,
  className,
  children,
}: {
  label: string;
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <div className={cn("space-y-2", className)}>
      <Label className="text-xs text-muted-foreground">{label}</Label>
      {children}
    </div>
  );
}

function QuestionsTable({
  questions,
  periods,
}: {
  questions: PeriodAnalyticsResponse["questionStatistics"];
  periods: PeriodAnalyticsResponse[];
}) {
  return (
    <div className="mb-8 overflow-hidden border bg-card shadow-sm">
      <div className="border-b px-4 py-4">
        <h3 className="text-base font-bold text-foreground md:text-lg">
          Детализация
        </h3>
      </div>
      <div className="overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="text-center">№</TableHead>
              <TableHead className="text-left">Вопрос</TableHead>
              {periods.length === 1 ? (
                <>
                  <TableHead className="text-right">Удовл. потреб., %</TableHead>
                  <TableHead className="text-right">Средний балл</TableHead>
                  <TableHead className="text-right">Отклонение</TableHead>
                  <TableHead className="text-right">Оценка</TableHead>
                  <TableHead className="text-right">Ответов</TableHead>
                </>
              ) : (
                periods.map((period, index) => (
                  <TableHead
                    key={`${period.label}-${index}`}
                    className="text-right"
                  >
                    {period.label}
                  </TableHead>
                ))
              )}
            </TableRow>
          </TableHeader>
          <TableBody>
            {questions.map((question, index) => (
              <TableRow key={question.questionId}>
                <TableCell className="text-center">{index + 1}</TableCell>
                <TableCell>{question.questionText}</TableCell>
                {periods.length === 1 ? (
                  <>
                    <TableCell className="text-right">
                      {question.satisfactionPercentage.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-right">
                      {question.averageScore.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-right">
                      {question.standardDeviation.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-right">
                      {ratingLabels[question.rating]}
                    </TableCell>
                    <TableCell className="text-right">
                      {question.responseCount}
                    </TableCell>
                  </>
                ) : (
                  periods.map((period, periodIndex) => {
                    const metric = period.questionStatistics.find(
                      (q) => q.questionId === question.questionId,
                    );
                    return (
                      <TableCell
                        key={`${question.questionId}-${periodIndex}`}
                        className="text-right"
                      >
                        {metric?.satisfactionPercentage.toFixed(2) ?? "-"}
                      </TableCell>
                    );
                  })
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
