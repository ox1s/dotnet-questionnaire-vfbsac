import { useEffect, useState, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import api, { type FormDetail } from "../../api";
import { WeightedRatingInput } from "@/components/survey/weighted-rating-input";
import {
  ContextSelector,
  type SubmissionContext,
} from "@/components/survey/context-selector";
import { ArrowLeft, CheckCircle } from "lucide-react";
import { toast } from "sonner";
import { getDeviceId } from "../../utils/device";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Separator } from "@/components/ui/separator";

// A WeightedRating answer stores both the respondent's score and their importance
// weight; Text/Number answers store the raw input value directly.
interface WeightedRatingAnswer {
  value: number | undefined;
  weight: number | undefined;
}

type AnswerValue = string | number | WeightedRatingAnswer;

export const SurveyPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState<FormDetail | null>(null);
  const [answers, setAnswers] = useState<Record<string, AnswerValue>>({});
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

    if (form.requiredFilters?.includes("Teacher") && !context.teacherId) {
      toast.error("Пожалуйста, выберите преподавателя!");
      return;
    }
    if (form.requiredFilters?.includes("Department") && !context.departmentId) {
      toast.error("Пожалуйста, выберите кафедру!");
      return;
    }
    if (form.requiredFilters?.includes("Discipline") && !context.disciplineId) {
      toast.error("Пожалуйста, выберите дисциплину!");
      return;
    }
    if (form.requiredFilters?.includes("Speciality") && !context.specialityId) {
      toast.error("Пожалуйста, выберите специальность!");
      return;
    }

    const answersPayload = form.questions
      .map((q) => {
        const ans = answers[q.id];
        if (!ans) return null;

        if (q.type === "WeightedRating") {
          const weighted = ans as WeightedRatingAnswer;
          return {
            questionId: q.id,
            numericValue: weighted.value,
            weight: weighted.weight,
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
        departmentId: context.departmentId || null,
        disciplineId: context.disciplineId || null,
        specialityId: context.specialityId || null,
        answers: answersPayload,
      });
      toast.success("Анкета успешно отправлена!");
      navigate("/dashboard");
    } catch (e) {
      console.error(e);
      if (axios.isAxiosError(e) && e.response?.status === 409) {
        toast.error("Вы уже голосовали за этого преподавателя/дисциплину!");
      } else {
        toast.error("Ошибка при отправке. Проверьте данные.");
      }
    }
  };

  if (!form) return <div className="p-8 text-center">Загрузка...</div>;

  return (
    <div className="min-h-screen bg-muted/40 pb-20">
      <header className="bg-background border-b sticky top-0 z-10">
        <div className="max-w-3xl mx-auto flex items-center gap-4 px-6 py-4">
          <Button variant="outline" size="icon" onClick={() => navigate(-1)}>
            <ArrowLeft />
          </Button>

          <h1 className="text-lg font-semibold truncate">{form.title}</h1>
        </div>
      </header>

      <main className="max-w-3xl mx-auto px-4 py-6 space-y-6">
        <ContextSelector
          requiredFilters={form.requiredFilters}
          onChange={handleContextChange}
        />
        {form.questions.map((q, idx) => (
          <Card key={q.id}>
            <CardContent className="p-6 space-y-4">
              <div className="flex gap-3">
                <div className="flex size-8 items-center justify-center bg-muted text-sm font-medium">
                  {idx + 1}
                </div>
                <p className="font-medium leading-snug">{q.text}</p>
              </div>

              <Separator />

              <div className="pl-11">
                {q.type === "WeightedRating" && (
                  <WeightedRatingInput
                    value={(answers[q.id] as WeightedRatingAnswer | undefined)?.value}
                    weight={(answers[q.id] as WeightedRatingAnswer | undefined)?.weight}
                    onChange={(v, w) =>
                      setAnswers({
                        ...answers,
                        [q.id]: { value: v, weight: w },
                      })
                    }
                  />
                )}

                {q.type === "Text" && (
                  <Textarea
                    placeholder="Ваш ответ..."
                    value={(answers[q.id] as string | undefined) || ""}
                    onChange={(e) =>
                      setAnswers({ ...answers, [q.id]: e.target.value })
                    }
                  />
                )}

                {q.type === "Number" && (
                  <Input
                    type="number"
                    placeholder="1–10"
                    className="w-32"
                    value={(answers[q.id] as number | undefined) || ""}
                    onChange={(e) =>
                      setAnswers({
                        ...answers,
                        [q.id]: parseFloat(e.target.value),
                      })
                    }
                  />
                )}
              </div>
            </CardContent>
          </Card>
        ))}

        <div className="pt-4">
          <Button className="w-full" size="lg" onClick={handleSubmit}>
            <CheckCircle data-icon="inline-start" />
            Отправить анкету
          </Button>
        </div>
      </main>
    </div>
  );
};
