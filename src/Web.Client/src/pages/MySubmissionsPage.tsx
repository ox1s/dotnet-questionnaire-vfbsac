import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  submissionsApi,
  dictionariesApi,
  type SubmissionListItem,
  type DictionaryItem,
  type TeacherItem,
} from "../api";
import { ArrowLeft, Clock, CheckCircle2 } from "lucide-react";

export const MySubmissionsPage = () => {
  const navigate = useNavigate();
  const [submissions, setSubmissions] = useState<SubmissionListItem[]>([]);
  const [loading, setLoading] = useState(true);

  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [subRes, teachRes, discRes] = await Promise.all([
          submissionsApi.getMyList(),
          dictionariesApi.getTeachers(),
          dictionariesApi.getDisciplines(),
        ]);
        setSubmissions(subRes.data);
        setTeachers(teachRes.data);
        setDisciplines(discRes.data);
      } catch (e) {
        console.error(e);
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  const resolveName = (
    id?: string,
    list?: { id: string; name?: string; fullName?: string }[],
  ) => {
    if (!id) return null;
    const found = list?.find((x) => x.id === id);
    return found ? found.name || found.fullName : "???";
  };

  return (
    <div className="min-h-screen bg-slate-50">
      <nav className="bg-white border-b border-slate-200 px-6 py-4 mb-6">
        <div className="max-w-3xl mx-auto flex items-center gap-4">
          <button
            onClick={() => navigate("/dashboard")}
            className="text-slate-500 hover:text-slate-800"
          >
            <ArrowLeft />
          </button>
          <h1 className="text-lg font-bold">Моя история</h1>
        </div>
      </nav>

      <main className="max-w-3xl mx-auto px-4 pb-10">
        {loading ? (
          <div className="text-center p-10 text-slate-500">Загрузка...</div>
        ) : submissions.length === 0 ? (
          <div className="text-center p-10 border-2 border-dashed border-slate-200 rounded-2xl text-slate-400">
            Вы еще не заполнили ни одной анкеты.
          </div>
        ) : (
          <div className="space-y-4">
            {submissions.map((s) => {
              const teacherName = resolveName(s.context.teacherId, teachers);
              const discName = resolveName(s.context.disciplineId, disciplines);

              return (
                <div
                  key={s.id}
                  className="bg-white p-5 rounded-xl shadow-sm border border-slate-200 flex flex-col sm:flex-row justify-between gap-4"
                >
                  <div>
                    <div className="flex items-center gap-2 text-green-600 mb-2 font-bold text-sm uppercase tracking-wide">
                      <CheckCircle2 size={16} /> Отправлено
                    </div>

                    {teacherName && (
                      <p className="text-slate-900 font-bold text-lg">
                        {teacherName}
                      </p>
                    )}
                    {discName && (
                      <p className="text-slate-600 font-medium">{discName}</p>
                    )}

                    {!teacherName && !discName && (
                      <p className="text-slate-900">
                        Анонимная анкета (без контекста)
                      </p>
                    )}
                  </div>

                  <div className="flex items-center gap-2 text-slate-400 text-sm whitespace-nowrap">
                    <Clock size={16} />
                    {new Date(s.submittedAt).toLocaleDateString()}{" "}
                    {new Date(s.submittedAt).toLocaleTimeString().slice(0, 5)}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </main>
    </div>
  );
};
