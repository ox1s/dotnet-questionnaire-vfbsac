import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import api, { type FormDetail, type Statistics, reportsApi } from "../api";
import { Download, Users, TrendingUp, Activity } from "lucide-react";
import { AdminLayout } from "../layouts/AdminLayout";

export const AdminStatsPage = () => {
  const { id } = useParams();
  const [form, setForm] = useState<FormDetail | null>(null);
  const [stats, setStats] = useState<Statistics | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    const fetchData = async () => {
      try {
        const [formRes, statsRes] = await Promise.all([
          api.get<FormDetail>(`/forms/${id}`),
          reportsApi.getStatistics(id),
        ]);
        setForm(formRes.data);
        setStats(statsRes.data);
      } catch (e) { console.error(e); } finally { setLoading(false); }
    };
    fetchData();
  }, [id]);

  const handleDownload = async () => {
    if (!id) return;
    try {
      const response = await reportsApi.downloadWordReport(id);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `report_${id}.docx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (e) { alert("Ошибка загрузки файла"); }
  };

  if (loading) return <div className="p-10 text-center text-slate-500">Загрузка данных...</div>;
  if (!form || !stats) return <div className="p-10 text-center text-red-500">Ошибка загрузки</div>;

  const numericQuestions = form.questions
    .filter((q) => q.type === "Number" || q.type === "WeightedRating")
    .sort((a, b) => a.order - b.order);

  return (
    <AdminLayout
      title="Аналитика"
      subtitle={`Отчет по форме: ${form.title}`}
      actions={
        <button 
          onClick={handleDownload}
          className="flex items-center gap-2 px-5 py-2.5 bg-white border border-slate-200 text-slate-700 rounded-xl hover:bg-slate-50 font-bold shadow-sm text-sm transition-all"
        >
          <Download size={18} /> Экспорт в Word
        </button>
      }
    >
      {/* KPI Карточки */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div className="bg-white p-6 rounded-2xl shadow-sm border border-slate-200 flex flex-col justify-between h-32">
           <div className="flex items-center gap-3 text-slate-500">
              <div className="p-2 bg-blue-50 text-blue-600 rounded-lg"><Users size={20}/></div>
              <span className="text-sm font-bold uppercase tracking-wider">Всего анкет</span>
           </div>
           <p className="text-4xl font-bold text-slate-900">{stats.totalSubmissions}</p>
        </div>

        <div className="bg-white p-6 rounded-2xl shadow-sm border border-slate-200 flex flex-col justify-between h-32">
           <div className="flex items-center gap-3 text-slate-500">
              <div className="p-2 bg-green-50 text-green-600 rounded-lg"><TrendingUp size={20}/></div>
              <span className="text-sm font-bold uppercase tracking-wider">Средний балл</span>
           </div>
           <p className="text-4xl font-bold text-slate-900">{stats.overallAverage.toFixed(2)}</p>
        </div>

        <div className="bg-white p-6 rounded-2xl shadow-sm border border-slate-200 flex flex-col justify-between h-32">
           <div className="flex items-center gap-3 text-slate-500">
              <div className="p-2 bg-purple-50 text-purple-600 rounded-lg"><Activity size={20}/></div>
              <span className="text-sm font-bold uppercase tracking-wider">Отклонение</span>
           </div>
           <p className="text-4xl font-bold text-slate-900">{stats.overallStandardDeviation.toFixed(2)}</p>
        </div>
      </div>

      {/* Таблица Деталей */}
      <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
        <div className="px-6 py-4 border-b border-slate-100">
           <h3 className="text-lg font-bold text-slate-900">Детализация по вопросам</h3>
        </div>
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50/50">
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase w-16 text-center">№</th>
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase">Вопрос</th>
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase text-right">Среднее</th>
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase text-right">Итог</th>
              <th className="py-4 px-6 text-xs font-bold text-slate-500 uppercase text-right">Sigma</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {numericQuestions.map((q, idx) => (
              <tr key={q.id} className="hover:bg-slate-50 transition-colors">
                <td className="py-4 px-6 text-center">
                   <span className="inline-block w-6 h-6 rounded bg-slate-100 text-slate-600 text-xs font-bold leading-6">{idx + 1}</span>
                </td>
                <td className="py-4 px-6 text-sm font-medium text-slate-900">{q.text}</td>
                <td className="py-4 px-6 text-sm text-slate-500 text-right font-mono">
                  {stats.averageScores[idx]?.toFixed(2) ?? "-"}
                </td>
                <td className="py-4 px-6 text-sm font-bold text-primary text-right font-mono">
                  {stats.resultScores[idx]?.toFixed(2) ?? "-"}
                </td>
                <td className="py-4 px-6 text-sm text-slate-400 text-right font-mono">
                  {stats.standardDeviations[idx]?.toFixed(2) ?? "-"}
                </td>
              </tr>
            ))}
            {numericQuestions.length === 0 && (
               <tr><td colSpan={5} className="p-8 text-center text-slate-400">Нет числовых данных</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </AdminLayout>
  );
};
