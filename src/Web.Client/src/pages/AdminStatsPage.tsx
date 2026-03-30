import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import api, {
  type FormDetail,
  type Statistics,
  reportsApi,
  dictionariesApi,
  type TeacherItem,
  type DictionaryItem,
  type StatisticsFilters,
} from "../api";
import {
  Download,
  Users,
  TrendingUp,
  Activity,
  Filter,
  RefreshCw,
} from "lucide-react";
import { AdminLayout } from "../layouts/AdminLayout";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  ReferenceLine,
  Cell,
} from "recharts";

export const AdminStatsPage = () => {
  const { id } = useParams();

  // Состояния данных
  const [form, setForm] = useState<FormDetail | null>(null);
  const [stats, setStats] = useState<Statistics | null>(null);
  const [loadingInitial, setLoadingInitial] = useState(true);
  const [loadingStats, setLoadingStats] = useState(false);

  // Справочники для фильтров
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);
  const [specialities, setSpecialities] = useState<DictionaryItem[]>([]);
  const [specializations, setSpecializations] = useState<DictionaryItem[]>([]);

  // Состояние фильтров
  const [filters, setFilters] = useState<StatisticsFilters>({});

  // 1. Первоначальная загрузка формы и справочников
  useEffect(() => {
    if (!id) return;
    const fetchInitialData = async () => {
      try {
        const [formRes, tchRes, depRes, discRes, specRes, speczRes] =
          await Promise.all([
            api.get<FormDetail>(`/forms/${id}`),
            dictionariesApi.getTeachers(),
            dictionariesApi.getDepartments(),
            dictionariesApi.getDisciplines(),
            dictionariesApi.getSpecialities(),
            dictionariesApi.getSpecializations(),
          ]);

        setForm(formRes.data);
        setTeachers(tchRes.data);
        setDepartments(depRes.data);
        setDisciplines(discRes.data);
        setSpecialities(specRes.data);
        setSpecializations(speczRes.data);
      } catch (e) {
        console.error("Ошибка при загрузке базовых данных", e);
      } finally {
        setLoadingInitial(false);
      }
    };
    fetchInitialData();
  }, [id]);

  // 2. Загрузка статистики (срабатывает при первом рендере и при изменении фильтров)
  const fetchStatistics = async () => {
    if (!id) return;
    setLoadingStats(true);
    try {
      const statsRes = await reportsApi.getStatistics(id, filters);
      setStats(statsRes.data);
    } catch (e) {
      console.error("Ошибка при загрузке статистики", e);
    } finally {
      setLoadingStats(false);
    }
  };

  useEffect(() => {
    // Вызываем первую загрузку статистики после того как id доступен
    fetchStatistics();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const handleApplyFilters = () => {
    fetchStatistics();
  };

  const handleResetFilters = () => {
    setFilters({});
    // Изменение состояния асинхронно, поэтому лучше передать пустой объект напрямую в fetch
    setLoadingStats(true);
    reportsApi.getStatistics(id!, {}).then((res) => {
      setStats(res.data);
      setLoadingStats(false);
    });
  };

  const handleDownload = async () => {
    if (!id) return;
    try {
      const response = await reportsApi.downloadWordReport(id, filters);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `report_${id}.docx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (e) {
      alert("Ошибка загрузки файла");
    }
  };

  const renderFilterChange = (
    field: keyof StatisticsFilters,
    value: string,
  ) => {
    setFilters((prev) => ({
      ...prev,
      [field]: value === "" ? undefined : value,
    }));
  };

  const renderContent = () => {
    if (loadingInitial) {
      return (
        <div className="flex flex-col items-center justify-center p-20 text-slate-400">
          <div className="w-8 h-8 border-4 border-primary border-t-transparent rounded-full animate-spin mb-4"></div>
          <p>Загрузка данных...</p>
        </div>
      );
    }

    if (!form || !stats) {
      return (
        <div className="p-10 text-center text-accent bg-accent/10 rounded-2xl border border-accent/20">
          Ошибка при загрузке данных. Возможно, анкета была удалена.
        </div>
      );
    }

    const numericQuestions = form.questions
      .filter(
        (q) =>
          q.type === "Number" ||
          q.type === "WeightedRating" ||
          q.type === "Rating",
      )
      .sort((a, b) => a.order - b.order);

    const chartData = numericQuestions.map((q, idx) => ({
      name: `В${idx + 1}`,
      fullName: q.text,
      score: stats.resultScores[idx] || 0,
      average: stats.averageScores[idx] || 0,
    }));

    const CustomTooltip = ({ active, payload }: any) => {
      if (active && payload && payload.length) {
        return (
          <div className="bg-slate-800 text-white p-3 rounded-lg shadow-xl text-xs max-w-50 sm:max-w-62.5 whitespace-normal">
            <p className="font-bold mb-1 leading-tight">
              {payload[0].payload.fullName}
            </p>
            <p className="text-slate-300 mt-2">
              Балл:{" "}
              <span className="text-white font-bold text-lg">
                {payload[0].value.toFixed(2)}
              </span>
            </p>
          </div>
        );
      }
      return null;
    };

    return (
      <>
        {/* Блок фильтров */}
        <div className="bg-white p-5 rounded-2xl shadow-sm border border-slate-200 mb-6 animate-in fade-in slide-in-from-top-4 duration-500">
          <div className="flex items-center gap-2 mb-4 text-slate-800 font-bold">
            <Filter size={18} />
            <h4>Фильтры аналитики</h4>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
            <select
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-primary/20"
              value={filters.teacherId || ""}
              onChange={(e) => renderFilterChange("teacherId", e.target.value)}
            >
              <option value="">Все преподаватели</option>
              {teachers.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.fullName}
                </option>
              ))}
            </select>

            <select
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-primary/20"
              value={filters.disciplineId || ""}
              onChange={(e) =>
                renderFilterChange("disciplineId", e.target.value)
              }
            >
              <option value="">Все дисциплины</option>
              {disciplines.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>

            <select
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-primary/20"
              value={filters.departmentId || ""}
              onChange={(e) =>
                renderFilterChange("departmentId", e.target.value)
              }
            >
              <option value="">Все кафедры</option>
              {departments.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>

            <select
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-primary/20"
              value={filters.specialityId || ""}
              onChange={(e) =>
                renderFilterChange("specialityId", e.target.value)
              }
            >
              <option value="">Все специальности</option>
              {specialities.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>

            <select
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-primary/20"
              value={filters.specializationId || ""}
              onChange={(e) =>
                renderFilterChange("specializationId", e.target.value)
              }
            >
              <option value="">Все специализации</option>
              {specializations.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>

            <input
              type="text"
              placeholder="Название организации..."
              className="px-3 py-2 border border-slate-200 rounded-lg text-sm bg-slate-50 focus:bg-white focus:outline-none focus:ring-2 focus:ring-primary/20"
              value={filters.organizationName || ""}
              onChange={(e) =>
                renderFilterChange("organizationName", e.target.value)
              }
            />
          </div>

          <div className="flex gap-3 justify-end">
            <button
              onClick={handleResetFilters}
              className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-900 transition-colors"
            >
              Сбросить
            </button>
            <button
              onClick={handleApplyFilters}
              disabled={loadingStats}
              className="flex items-center gap-2 px-5 py-2 bg-primary text-white rounded-lg text-sm font-bold shadow-sm hover:bg-primary/90 transition-all disabled:opacity-70"
            >
              {loadingStats ? (
                <RefreshCw className="animate-spin" size={16} />
              ) : null}
              Применить фильтры
            </button>
          </div>
        </div>

        {/* Индикатор загрузки при обновлении статистики */}
        {loadingStats ? (
          <div className="flex justify-center p-10 text-slate-400">
            <RefreshCw className="animate-spin mb-4" size={24} />
          </div>
        ) : (
          <>
            {/* Карточки метрик */}
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 md:gap-6 mb-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
              <div className="bg-white p-5 md:p-6 rounded-2xl shadow-sm border border-slate-200 flex flex-col justify-between min-h-27.5">
                <div className="flex items-center gap-3 text-slate-500 mb-2">
                  <div className="p-2 bg-blue-50 text-blue-600 rounded-lg">
                    <Users size={18} />
                  </div>
                  <span className="text-xs md:text-sm font-bold uppercase tracking-wider">
                    Всего анкет
                  </span>
                </div>
                <p className="text-3xl md:text-4xl font-bold text-slate-900">
                  {stats.totalSubmissions}
                </p>
              </div>

              <div className="bg-white p-5 md:p-6 rounded-2xl shadow-sm border border-slate-200 flex flex-col justify-between min-h-27.5">
                <div className="flex items-center gap-3 text-slate-500 mb-2">
                  <div className="p-2 bg-green-50 text-green-600 rounded-lg">
                    <TrendingUp size={18} />
                  </div>
                  <span className="text-xs md:text-sm font-bold uppercase tracking-wider">
                    Средний балл
                  </span>
                </div>
                <p className="text-3xl md:text-4xl font-bold text-slate-900">
                  {stats.overallAverage.toFixed(2)}
                </p>
              </div>

              <div className="bg-white p-5 md:p-6 rounded-2xl shadow-sm border border-slate-200 flex flex-col justify-between min-h-27.5 sm:col-span-2 md:col-span-1">
                <div className="flex items-center gap-3 text-slate-500 mb-2">
                  <div className="p-2 bg-purple-50 text-purple-600 rounded-lg">
                    <Activity size={18} />
                  </div>
                  <span className="text-xs md:text-sm font-bold uppercase tracking-wider">
                    Отклонение
                  </span>
                </div>
                <p className="text-3xl md:text-4xl font-bold text-slate-900">
                  {stats.overallStandardDeviation.toFixed(2)}
                </p>
              </div>
            </div>

            {/* График */}
            <div className="bg-white p-4 md:p-6 rounded-2xl shadow-sm border border-slate-200 mb-8 overflow-hidden animate-in fade-in slide-in-from-bottom-6 duration-700">
              <h3 className="text-base md:text-lg font-bold text-slate-900 mb-6">
                Распределение оценок по вопросам
              </h3>
              <div className="h-64 md:h-80 w-full min-w-70">
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
                      content={<CustomTooltip />}
                      cursor={{ fill: "#f1f5f9" }}
                    />
                    <ReferenceLine
                      y={stats.overallAverage}
                      stroke="#10b981"
                      strokeDasharray="3 3"
                    />
                    <Bar dataKey="score" radius={[4, 4, 0, 0]} maxBarSize={40}>
                      {chartData.map((entry, index) => (
                        <Cell
                          key={`cell-${index}`}
                          fill={
                            entry.score >= 8
                              ? "var(--color-primary)"
                              : entry.score >= 5
                                ? "var(--color-secondary)"
                                : "var(--color-accent)"
                          }
                        />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </div>
              <p className="text-[10px] md:text-xs text-center text-slate-400 mt-6 leading-relaxed">
                Цвета столбцов:{" "}
                <span className="text-primary font-bold">
                  ● &gt;8 (Отлично)
                </span>
                ,{" "}
                <span className="text-secondary font-bold">● 5-8 (Средне)</span>
                , <span className="text-accent font-bold">● &lt;5 (Плохо)</span>
              </p>
            </div>

            {/* Таблица */}
            <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden mb-8 animate-in fade-in slide-in-from-bottom-8 duration-700">
              <div className="px-4 md:px-6 py-4 border-b border-slate-100">
                <h3 className="text-base md:text-lg font-bold text-slate-900">
                  Детализация
                </h3>
              </div>

              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse min-w-125">
                  <thead>
                    <tr className="bg-slate-50/50">
                      <th className="py-3 px-4 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase w-12 text-center">
                        №
                      </th>
                      <th className="py-3 px-4 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase">
                        Вопрос
                      </th>
                      <th className="py-3 px-4 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase text-right w-24">
                        Среднее
                      </th>
                      <th className="py-3 px-4 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase text-right w-24">
                        Итог
                      </th>
                      <th className="py-3 px-4 md:py-4 md:px-6 text-[10px] md:text-xs font-bold text-slate-500 uppercase text-right w-24">
                        Sigma
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {numericQuestions.map((q, idx) => (
                      <tr
                        key={q.id}
                        className="hover:bg-slate-50 transition-colors"
                      >
                        <td className="py-3 px-4 md:py-4 md:px-6 text-center">
                          <span className="inline-block w-6 h-6 rounded bg-slate-100 text-slate-600 text-[10px] md:text-xs font-bold leading-6">
                            {idx + 1}
                          </span>
                        </td>
                        <td className="py-3 px-4 md:py-4 md:px-6 text-xs md:text-sm font-medium text-slate-900 leading-snug">
                          {q.text}
                        </td>
                        <td className="py-3 px-4 md:py-4 md:px-6 text-xs md:text-sm text-slate-500 text-right font-mono">
                          {stats.averageScores[idx]?.toFixed(2) ?? "-"}
                        </td>
                        <td className="py-3 px-4 md:py-4 md:px-6 text-xs md:text-sm font-bold text-primary text-right font-mono">
                          {stats.resultScores[idx]?.toFixed(2) ?? "-"}
                        </td>
                        <td className="py-3 px-4 md:py-4 md:px-6 text-xs md:text-sm text-slate-400 text-right font-mono">
                          {stats.standardDeviations[idx]?.toFixed(2) ?? "-"}
                        </td>
                      </tr>
                    ))}
                    {numericQuestions.length === 0 && (
                      <tr>
                        <td
                          colSpan={5}
                          className="p-8 text-center text-slate-400 text-sm"
                        >
                          Нет числовых данных
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </>
        )}
      </>
    );
  };

  return (
    <AdminLayout
      title="Аналитика"
      subtitle={form ? `Отчет по форме: ${form.title}` : "Загрузка..."}
      actions={
        form && stats ? (
          <button
            onClick={handleDownload}
            className="flex items-center gap-2 px-5 py-2.5 bg-white border border-slate-200 text-slate-700 rounded-xl hover:bg-slate-50 font-bold shadow-sm text-sm transition-all whitespace-nowrap w-full md:w-auto justify-center"
          >
            <Download size={18} /> Экспорт в Word
          </button>
        ) : null
      }
    >
      {renderContent()}
    </AdminLayout>
  );
};
