import { useEffect, useState, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import api, { type FormDetail } from "../api";
import { WeightedRatingInput } from "../components/WeightedRatingInput";
import {
  ContextSelector,
  type SubmissionContext,
} from "../components/ContextSelector";
import { ArrowLeft, CheckCircle } from "lucide-react";
import toast from "react-hot-toast";
import { getDeviceId } from "../utils/device";

export const SurveyPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState<FormDetail | null>(null);
  const [answers, setAnswers] = useState<Record<string, any>>({});
  const [context, setContext] = useState<SubmissionContext>({
    educationForm: "ДФПО",
  });

  useEffect(() => {
    api.get<FormDetail>(`/forms/${id}`).then((res) => setForm(res.data));
  }, [id]);

  const handleContextChange = useCallback((newContext: SubmissionContext) => {
    setContext(newContext);
  }, []);

  const handleSubmit = async () => {
    if (!form) return;

    // Простая валидация
    if (form.requiredFilters?.includes("Teacher") && !context.teacherId) {
      alert("Пожалуйста, выберите преподавателя!");
      return;
    }
    if (form.requiredFilters?.includes("Discipline") && !context.disciplineId) {
      alert("Пожалуйста, выберите дисциплину!");
      return;
    }

    const answersPayload = form.questions
      .map((q) => {
        const ans = answers[q.id];
        if (!ans) return null;

        if (q.type === "WeightedRating") {
          return {
            questionId: q.id,
            numericValue: ans.value,
            weight: ans.weight,
          };
        }
        if (q.type === "Number") {
          return { questionId: q.id, numericValue: ans };
        }
        return { questionId: q.id, value: ans };
      })
      .filter(Boolean);

    try {
      await api.post("/submissions", {
        formId: form.id,
        deviceId: getDeviceId(),
        educationForm: context.educationForm,
        teacherId: context.teacherId || null,
        disciplineId: context.disciplineId || null,
        answers: answersPayload,
      });
      toast.success("Анкета успешно отправлена!");
      navigate("/dashboard");
    } catch (e: any) {
      console.error(e);
      if (e.response && e.response.status === 409) {
        toast.error("Вы уже голосовали за этого преподавателя/дисциплину!");
      } else {
        toast.error("Ошибка при отправке. Проверьте данные.");
      }
    }
  };

  if (!form) return <div className="p-8 text-center">Загрузка...</div>;

  return (
    <div className="min-h-screen bg-gray-50 pb-20">
      <header className="bg-white shadow-sm px-6 py-4 mb-6 sticky top-0 z-10">
        <div className="max-w-3xl mx-auto flex items-center gap-4">
          <button
            onClick={() => navigate(-1)}
            className="text-gray-500 hover:text-gray-800"
          >
            <ArrowLeft />
          </button>
          <h1 className="text-lg font-bold truncate">{form.title}</h1>
        </div>
      </header>

      <main className="max-w-3xl mx-auto px-4 space-y-6">
        <ContextSelector
          requiredFilters={form.requiredFilters}
          onChange={handleContextChange}
        />

        {form.questions.map((q, idx) => (
          <div key={q.id} className="bg-white p-6 rounded-lg shadow-sm">
            <div className="flex gap-3 mb-4">
              <span className="shrink-0 w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center font-bold text-gray-600 text-sm">
                {idx + 1}
              </span>
              <p className="font-medium text-gray-800 pt-1">{q.text}</p>
            </div>

            <div className="pl-11">
              {q.type === "WeightedRating" && (
                <WeightedRatingInput
                  value={answers[q.id]?.value}
                  weight={answers[q.id]?.weight}
                  onChange={(v, w) =>
                    setAnswers({ ...answers, [q.id]: { value: v, weight: w } })
                  }
                />
              )}

              {q.type === "Text" && (
                <textarea
                  className="input-field min-h-25"
                  placeholder="Ваш ответ..."
                  value={answers[q.id] || ""}
                  onChange={(e) =>
                    setAnswers({ ...answers, [q.id]: e.target.value })
                  }
                />
              )}

              {q.type === "Number" && (
                <input
                  type="number"
                  className="input-field w-32"
                  placeholder="1-10"
                  value={answers[q.id] || ""}
                  onChange={(e) =>
                    setAnswers({
                      ...answers,
                      [q.id]: parseFloat(e.target.value),
                    })
                  }
                />
              )}
            </div>
          </div>
        ))}

        <button
          onClick={handleSubmit}
          className="btn-primary w-full py-4 text-lg shadow-lg flex items-center justify-center gap-2"
        >
          <CheckCircle /> Отправить анкету
        </button>
      </main>
    </div>
  );
};
