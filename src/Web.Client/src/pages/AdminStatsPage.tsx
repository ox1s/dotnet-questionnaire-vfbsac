import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import api, { type FormDetail, type Statistics, reportsApi } from "../api";
import { ArrowLeft, Download } from "lucide-react";
export const AdminStatsPage = () => {
  const { id } = useParams(); // formId
  const navigate = useNavigate();

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
      } catch (e) {
        console.error(e);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [id]);

  const handleDownload = async () => {
    if (!id) return;
    try {
      const response = await reportsApi.downloadWordReport(id);

      // Магия для скачивания файла в браузере
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `report_${id}.docx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (e) {
      alert("Ошибка при скачивании отчета");
    }
  };

  if (loading)
    return <div className="p-8 text-center">Загрузка статистики...</div>;
  if (!form || !stats)
    return <div className="p-8 text-center">Ошибка загрузки</div>;

  const numericQuestions = form.questions
    .filter((q) => q.type === "Number" || q.type === "WeightedRating")
    .sort((a, b) => a.order - b.order);

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-5xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <div className="flex items-center gap-4">
            <button
              onClick={() => navigate("/dashboard")}
              className="text-gray-500 hover:text-gray-800"
            >
              <ArrowLeft />
            </button>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">{form.title}</h1>
              <p className="text-gray-500">
                Всего анкет: {stats.totalSubmissions}
              </p>
            </div>
          </div>

          <button
            onClick={handleDownload}
            className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition"
          >
            <Download size={18} /> Скачать Word (.docx)
          </button>
        </div>

        {/* Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
            <h3 className="text-sm font-medium text-gray-500">
              Средний балл (Общий)
            </h3>
            <p className="text-3xl font-bold text-blue-600 mt-2">
              {stats.overallAverage.toFixed(2)}
            </p>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
            <h3 className="text-sm font-medium text-gray-500">
              Отклонение (Sigma)
            </h3>
            <p className="text-3xl font-bold text-gray-700 mt-2">
              {stats.overallStandardDeviation.toFixed(2)}
            </p>
          </div>
        </div>

        {/* Table */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase">
                  №
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase w-1/2">
                  Вопрос
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase text-right">
                  Среднее
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase text-right">
                  Итог (Взвеш.)
                </th>
                <th className="px-6 py-3 text-xs font-medium text-gray-500 uppercase text-right">
                  Откл.
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {numericQuestions.map((q, idx) => (
                <tr key={q.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm text-gray-500">{idx + 1}</td>
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">
                    {q.text}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600 text-right">
                    {stats.averageScores[idx]?.toFixed(2) ?? "-"}
                  </td>
                  <td className="px-6 py-4 text-sm font-bold text-blue-600 text-right">
                    {stats.resultScores[idx]?.toFixed(2) ?? "-"}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500 text-right">
                    {stats.standardDeviations[idx]?.toFixed(2) ?? "-"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {numericQuestions.length === 0 && (
            <div className="p-8 text-center text-gray-500">
              В этой анкете нет числовых вопросов для статистики.
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
