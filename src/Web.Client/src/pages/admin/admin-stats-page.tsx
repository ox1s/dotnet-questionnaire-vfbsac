import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { toast } from "sonner";
import { Download, RefreshCw } from "lucide-react";
import { format } from "date-fns";

import { Button } from "@/components/ui/button";
import {
  getLinkedFilterOptions,
  sanitizeLinkedFilters,
} from "@/utils/linked-filters";

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
  type TextAnswerItem,
} from "../../api";
import { AdminLayout } from "@/components/admin/admin-shared";
import { AnalyticsFilterPanel } from "@/components/admin/admin-stats/analytics-filter-panel";
import { AnalyticsSummaryCards } from "@/components/admin/admin-stats/analytics-summary-cards";
import { AnalyticsChart } from "@/components/admin/admin-stats/analytics-chart";
import { QuestionsTable } from "@/components/admin/admin-stats/questions-table";
import { AdvicesSection } from "@/components/admin/admin-stats/advices-section";
import { TextAnswersSection } from "@/components/admin/admin-stats/text-answers-section";
import {
  getPreviousPeriodRange,
  getSemesterRange,
  type CompareField,
  type Mode,
  type RangeState,
} from "@/components/admin/admin-stats/admin-stats-utils";

export const AdminStatsPage = () => {
  const { id } = useParams();
  const [form, setForm] = useState<FormDetail | null>(null);
  const [report, setReport] = useState<PeriodAnalyticsResponse[] | null>(null);
  const [advices, setAdvices] = useState<AdviceItem[]>([]);
  const [textAnswers, setTextAnswers] = useState<TextAnswerItem[]>([]);
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
    if (!teacher.departmentIds || teacher.departmentIds.length === 0) {
      return teacher.fullName;
    }

    const departmentNames = teacher.departmentIds
      .map(
        (departmentId) =>
          departments.find((department) => department.id === departmentId)
            ?.name,
      )
      .filter((name): name is string => Boolean(name));

    return departmentNames.length > 0
      ? `${teacher.fullName} (${departmentNames.join(", ")})`
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
        ? teachers.filter((t) =>
            t.departmentIds?.includes(filtersWithoutCompareField.departmentId!),
          )
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
      setTextAnswers([]);
      return;
    }

    setRefreshing(true);
    try {
      let reportResponse: { data: PeriodAnalyticsResponse[] };

      if (mode === "single") {
        const singleResponse = await reportsApi.getAnalyticsByPeriod(
          request as AnalyticsByPeriodRequest,
        );
        const result = singleResponse.data;

        reportResponse = {
          data: [
            {
              label: singleRange.label,
              periodStart: singleRange.dateFrom,
              periodEnd: singleRange.dateTo,
              questionStatistics: result.questions,
              overall: result.overall,
              submissionCount: result.submissionCount,
            },
          ],
        };
      } else if (mode === "periods") {
        reportResponse = await reportsApi.getAnalyticsByPeriods(
          request as GetAnalyticsByPeriodsRequest,
        );
      } else {
        const groupsResponse = await reportsApi.getAnalyticsByGroups(
          request as GetAnalyticsByGroupsRequest,
        );

        // Filter groups by selectedIds
        const filteredGroups = groupsResponse.data.filter((item) =>
          selectedIds.includes(item.groupKey)
        );

        reportResponse = {
          data: filteredGroups.map((item) => ({
            label: item.groupName,
            periodStart: singleRange.dateFrom,
            periodEnd: singleRange.dateTo,
            questionStatistics: item.questionStatistics,
            overall: item.overall,
            submissionCount: item.submissionCount,
          })),
        };
      }

      const advicesResponse = await reportsApi.getAdvices(
        id,
        filters.teacherId,
      );
      const textAnswersResponse = await reportsApi.getTextAnswers({
        formId: id,
        filterSet: filters,
        periodStart: new Date(singleRange.dateFrom).toISOString(),
        periodEnd: new Date(singleRange.dateTo).toISOString(),
      });

      setReport(reportResponse.data);
      setAdvices(
        filters.departmentId
          ? advicesResponse.data.filter(
              (item) => item.departmentId === filters.departmentId,
            )
          : advicesResponse.data,
      );
      setTextAnswers(textAnswersResponse.data);
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
      let filename = `analytics-${format(new Date(), "yyyy-MM-dd")}.xlsx`;

      if (mode === "single") {
        response = await reportsApi.exportAnalyticsByPeriod(
          request as AnalyticsByPeriodRequest,
        );
        filename = `analytics-period-${format(new Date(), "yyyy-MM-dd")}.xlsx`;
      } else if (mode === "periods") {
        response = await reportsApi.exportAnalyticsByPeriods(
          request as GetAnalyticsByPeriodsRequest,
        );
        filename = `analytics-periods-${format(new Date(), "yyyy-MM-dd")}.xlsx`;
      } else {
        response = await reportsApi.exportAnalyticsByGroups(
          request as GetAnalyticsByGroupsRequest,
        );
        filename = `analytics-groups-${format(new Date(), "yyyy-MM-dd")}.xlsx`;
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
            <span className="hidden md:inline">Экспорт в Excel</span>
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
            <AnalyticsFilterPanel
              mode={mode}
              setMode={setMode}
              singleRange={singleRange}
              setSingleRange={setSingleRange}
              periods={periods}
              setPeriods={setPeriods}
              updatePeriod={updatePeriod}
              filters={filters}
              updateFilter={updateFilter}
              teachers={teachers}
              teacherLabel={teacherLabel}
              availableLinkedOptions={availableLinkedOptions}
              compareField={compareField}
              setCompareField={setCompareField}
              selectedIds={selectedIds}
              setSelectedIds={setSelectedIds}
              optionsFor={optionsFor}
              refreshing={refreshing}
              onRefresh={() => void loadReport()}
            />

            {refreshing ? (
              <div className="flex justify-center p-10 text-muted-foreground">
                <RefreshCw className="animate-spin" size={24} />
              </div>
            ) : null}
            {report !== null && report.length > 0 ? (
              <AnalyticsSummaryCards report={report} />
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

            {report && report.length > 0 ? (
              <AnalyticsChart report={report} questions={questions} />
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

            {report && report.length > 0 ? (
              <TextAnswersSection answers={textAnswers} />
            ) : null}
          </>
        )}
      </div>
    </AdminLayout>
  );
};
