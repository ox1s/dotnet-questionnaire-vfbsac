import { useEffect, useState, type ReactNode } from "react";
import { useParams } from "react-router-dom";
import { toast } from "sonner";
import { RefreshCw } from "lucide-react";

import api, { getApiErrorMessage, type FormDetail } from "../../api";
import { AdminLayout } from "@/components/admin/admin-shared";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";

// Kept in sync with QUESTION_TYPES in create-form-page.tsx so a form reads the
// same way in the builder and in the preview.
const QUESTION_TYPE_LABELS: Record<string, string> = {
  WeightedRating: "Рейтинг (Оценка + Важность)",
  Number: "Числовая оценка (1-10)",
  Text: "Текстовый комментарий",
  MultipleChoice: "Множественный выбор",
  SingleChoice: "Одиночный выбор",
};

// Kept in sync with FILTER_OPTIONS in create-form-page.tsx; Specialization has
// no builder option yet but can still appear on seeded/legacy forms.
const FILTER_FIELD_LABELS: Record<string, string> = {
  Teacher: "Преподаватель",
  Discipline: "Предмет",
  Department: "Филиал кафедры",
  Speciality: "Специальность",
  Specialization: "Специализация",
};

// Not a component (no JSX instantiation): callers need the actual return
// value up front to decide whether to render the separator/wrapper around it.
const renderQuestionPreview = (type: string): ReactNode | null => {
  switch (type) {
    case "Text":
      return (
        <Textarea disabled placeholder="Здесь респондент введёт текстовый ответ..." />
      );
    case "Number":
      return <Input disabled type="number" placeholder="1–10" className="w-32" />;
    case "WeightedRating":
      return (
        <div className="flex flex-col gap-3 border bg-muted/30 p-4">
          <div className="flex items-start gap-4">
            <div className="flex-1">
              <Label className="mb-1.5 text-xs text-muted-foreground">
                Важность (1-10)
              </Label>
              <Input disabled type="number" placeholder="10" />
            </div>
            <div className="mt-6 text-2xl font-light text-muted-foreground/40">
              /
            </div>
            <div className="flex-1">
              <Label className="mb-1.5 text-xs text-muted-foreground">
                Оценка (1-10)
              </Label>
              <Input disabled type="number" placeholder="8" />
            </div>
          </div>
          <div className="mt-1 text-xs text-muted-foreground">
            Слева укажите важность критерия, справа - реальную оценку.
          </div>
        </div>
      );
    case "MultipleChoice":
    case "SingleChoice":
      return (
        <p className="text-sm text-muted-foreground italic">
          Варианты ответа для этого типа вопроса пока не сохраняются в
          системе, поэтому здесь их показать нельзя.
        </p>
      );
    default:
      return null;
  }
};

export const AdminFormPreviewPage = () => {
  const { id } = useParams();
  const [form, setForm] = useState<FormDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);

  useEffect(() => {
    if (!id) return;
    api
      .get<FormDetail>(`/forms/${id}`)
      .then((res) => setForm(res.data))
      .catch((error) => {
        setLoadError(true);
        toast.error(getApiErrorMessage(error, "Не удалось загрузить анкету"));
      })
      .finally(() => setLoading(false));
  }, [id]);

  return (
    <AdminLayout
      title="Дашборд"
      subtitle={
        loading
          ? "Загрузка..."
          : loadError
            ? "Ошибка загрузки"
            : form
              ? `Просмотр анкеты: ${form.title}`
              : "Анкета не найдена"
      }
    >
      {loading ? (
        <div className="flex h-full min-h-[50vh] items-center justify-center text-muted-foreground">
          <RefreshCw className="animate-spin mr-2" size={24} /> Загрузка...
        </div>
      ) : loadError ? (
        <div className="border bg-card p-10 text-center text-muted-foreground">
          Не удалось загрузить анкету. Попробуйте обновить страницу.
        </div>
      ) : !form ? (
        <div className="border bg-card p-10 text-center text-muted-foreground">
          Анкета не найдена.
        </div>
      ) : (
        <div className="max-w-3xl mx-auto w-full space-y-6">
          <Card>
            <CardContent className="p-6 space-y-3">
              <div className="flex items-center gap-2 flex-wrap">
                <h2 className="text-lg font-semibold">{form.title}</h2>
                <Badge variant={form.isActive ? "default" : "destructive"}>
                  {form.isActive ? "Активна" : "Неактивна"}
                </Badge>
              </div>
              <div className="flex flex-wrap gap-2">
                {form.requiredFilters && form.requiredFilters.length > 0 ? (
                  form.requiredFilters.map((filter) => (
                    <Badge key={filter} variant="secondary">
                      {FILTER_FIELD_LABELS[filter] ?? filter}
                    </Badge>
                  ))
                ) : (
                  <Badge variant="outline">Без фильтров</Badge>
                )}
              </div>
            </CardContent>
          </Card>

          {form.questions.length === 0 ? (
            <div className="border bg-card p-10 text-center text-muted-foreground">
              У этой анкеты пока нет вопросов.
            </div>
          ) : (
            form.questions.map((q, idx) => {
              const preview = renderQuestionPreview(q.type);
              return (
                <Card key={q.id}>
                  <CardContent className="p-6 space-y-4">
                    <div className="flex gap-3 items-start justify-between">
                      <div className="flex gap-3">
                        <div className="flex size-8 shrink-0 items-center justify-center bg-muted text-sm font-medium">
                          {idx + 1}
                        </div>
                        <p className="font-medium leading-snug">{q.text}</p>
                      </div>
                      <Badge variant="outline" className="shrink-0">
                        {QUESTION_TYPE_LABELS[q.type] ?? q.type}
                      </Badge>
                    </div>

                    {preview !== null && (
                      <>
                        <Separator />
                        <div className="pl-11">{preview}</div>
                      </>
                    )}
                  </CardContent>
                </Card>
              );
            })
          )}
        </div>
      )}
    </AdminLayout>
  );
};
