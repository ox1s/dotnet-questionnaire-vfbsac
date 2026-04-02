import { useCallback, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../../api";
import {
  Plus,
  Trash2,
  Save,
  GripVertical,
  CheckCircle2,
  ChevronUp,
  ChevronDown,
} from "lucide-react";

import { toast } from "sonner";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";

const QuestionType = {
  Text: 1,
  Number: 2,
  WeightedRating: 6,
} as const;

type QuestionType = (typeof QuestionType)[keyof typeof QuestionType];
type FilterField = "Department" | "Discipline" | "Teacher" | "Speciality";

interface QuestionDraft {
  id?: string;
  text: string;
  type: QuestionType;
  order: number;
}

const FILTER_OPTIONS: { key: FilterField; label: string }[] = [
  { key: "Teacher", label: "Преподаватель" },
  { key: "Discipline", label: "Дисциплина" },
  { key: "Department", label: "Кафедра" },
  { key: "Speciality", label: "Специальность" },
];

const QUESTION_TYPES = [
  { value: QuestionType.WeightedRating, label: "Рейтинг (Оценка + Важность)" },
  { value: QuestionType.Number, label: "Числовая оценка (1-10)" },
  { value: QuestionType.Text, label: "Текстовый комментарий" },
];

export const CreateFormPage = () => {
  const navigate = useNavigate();

  const [title, setTitle] = useState("");
  const [selectedFilters, setSelectedFilters] = useState<FilterField[]>([]);
  const [questions, setQuestions] = useState<QuestionDraft[]>([]);

  const [newQText, setNewQText] = useState("");
  const [newQType, setNewQType] = useState<QuestionType>(
    QuestionType.WeightedRating,
  );
  const [draggedIdx, setDraggedIdx] = useState<number | null>(null);

  const toggleFilter = (filter: FilterField) => {
    setSelectedFilters((prev) =>
      prev.includes(filter)
        ? prev.filter((f) => f !== filter)
        : [...prev, filter],
    );
  };

  const addQuestion = () => {
    if (!newQText.trim()) return toast.error("Введите текст вопроса");
    setQuestions([
      ...questions,
      { text: newQText, type: newQType, order: questions.length + 1 },
    ]);
    setNewQText("");
  };

  const removeQuestion = (index: number) => {
    const updated = questions
      .filter((_, i) => i !== index)
      .map((q, i) => ({ ...q, order: i + 1 }));
    setQuestions(updated);
  };

  const moveQuestion = (index: number, direction: "up" | "down") => {
    if (direction === "up" && index === 0) return;
    if (direction === "down" && index === questions.length - 1) return;

    const newQs = [...questions];
    const swapIdx = direction === "up" ? index - 1 : index + 1;
    [newQs[index], newQs[swapIdx]] = [newQs[swapIdx], newQs[index]];
    setQuestions(newQs.map((q, i) => ({ ...q, order: i + 1 })));
  };

  const handleDragStart = (index: number) => setDraggedIdx(index);
  const handleDragEnter = (index: number) => {
    if (draggedIdx === null || draggedIdx === index) return;
    const newQuestions = [...questions];
    const draggedItem = newQuestions.splice(draggedIdx, 1)[0];
    newQuestions.splice(index, 0, draggedItem);
    setQuestions(newQuestions.map((q, i) => ({ ...q, order: i + 1 })));
    setDraggedIdx(index);
  };
  const handleDragEnd = () => setDraggedIdx(null);

  const handleSave = useCallback(async () => {
    if (!title.trim() || questions.length === 0) {
      toast.error("Заполните название и добавьте вопросы");
      return;
    }

    try {
      await api.post("/forms", {
        title,
        requiredFilters: selectedFilters,
        questions,
      });
      toast.success("Новая анкета создана!");
      navigate("/dashboard");
    } catch (e) {
      console.error(e);
      toast.error("Ошибка при сохранении");
    }
  }, [navigate, questions, selectedFilters, title]);

  const saveAction = useMemo(
    () => (
      <Button onClick={handleSave}>
        <Save size={16} className="mr-2" /> Сохранить анкету
      </Button>
    ),
    [handleSave],
  );

  useAdminPageConfig(
    {
      title: "Дашборд",
      subtitle: "Конструктор анкет",
      actions: saveAction,
    },
    [saveAction],
  );

  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
      <div className="space-y-6 lg:col-span-1">
        <Card>
          <CardHeader>
            <CardTitle className="text-sm text-muted-foreground uppercase tracking-wider">
              Настройки
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="space-y-2">
              <Label>Название анкеты</Label>
              <Textarea
                rows={3}
                placeholder="Например: Удовлетворенность качеством преподавания..."
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                className="bg-muted/50"
              />
            </div>
            <div className="space-y-3">
              <div>
                <Label>Контекст (Фильтры)</Label>
                <p className="text-xs text-muted-foreground mt-1">
                  Что выбирает студент перед началом?
                </p>
              </div>
              <div className="space-y-2">
                {FILTER_OPTIONS.map((opt) => {
                  const active = selectedFilters.includes(opt.key);
                  return (
                    <button
                      key={opt.key}
                      onClick={() => toggleFilter(opt.key)}
                      className={`flex w-full items-center justify-between border px-4 py-3 text-sm font-medium transition-all ${
                        active
                          ? "bg-primary border-primary text-primary-foreground shadow-sm"
                          : "bg-background border-border text-foreground hover:bg-muted"
                      }`}
                    >
                      {opt.label}
                      {active && <CheckCircle2 size={16} />}
                    </button>
                  );
                })}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="space-y-6 lg:col-span-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-sm text-muted-foreground uppercase tracking-wider">
              Новый вопрос
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col gap-4">
              <Input
                placeholder="Введите текст вопроса..."
                value={newQText}
                onChange={(e) => setNewQText(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && addQuestion()}
                className="bg-muted/50"
              />
              <div className="flex flex-col gap-3 sm:flex-row">
                <select
                  className="flex h-10 w-full border border-input bg-muted/50 px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 sm:flex-1"
                  value={newQType}
                  onChange={(e) =>
                    setNewQType(Number(e.target.value) as QuestionType)
                  }
                >
                  {QUESTION_TYPES.map((t) => (
                    <option key={t.value} value={t.value}>
                      {t.label}
                    </option>
                  ))}
                </select>
                <Button onClick={addQuestion} className="w-full sm:w-auto">
                  <Plus size={16} className="mr-2" /> Добавить
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="space-y-3">
          {questions.map((q, idx) => (
            <div
              key={q.order}
              draggable
              onDragStart={() => handleDragStart(idx)}
              onDragEnter={() => handleDragEnter(idx)}
              onDragEnd={handleDragEnd}
              onDragOver={(e) => e.preventDefault()}
              className={`group flex cursor-grab items-center gap-3 border border-border bg-card p-4 transition-all ${
                draggedIdx === idx ? "opacity-40 shadow-inner bg-muted" : ""
              } hover:border-primary/30 hover:shadow-sm`}
            >
              <div className="hidden text-muted-foreground hover:text-foreground sm:block">
                <GripVertical size={20} />
              </div>
              <div className="flex h-8 w-8 shrink-0 items-center justify-center bg-muted text-xs font-bold text-muted-foreground">
                {q.order}
              </div>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-bold text-foreground">
                  {q.text}
                </p>
                <Badge
                  variant="secondary"
                  className="mt-1 text-[10px] uppercase"
                >
                  {QUESTION_TYPES.find((t) => t.value === q.type)?.label ||
                    "Вопрос"}
                </Badge>
              </div>
              <div className="flex items-center gap-1 sm:gap-2">
                <div className="flex flex-col sm:flex-row">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => moveQuestion(idx, "up")}
                    disabled={idx === 0}
                    className="h-8 w-8 text-muted-foreground"
                  >
                    <ChevronUp size={16} />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => moveQuestion(idx, "down")}
                    disabled={idx === questions.length - 1}
                    className="h-8 w-8 text-muted-foreground"
                  >
                    <ChevronDown size={16} />
                  </Button>
                </div>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => removeQuestion(idx)}
                  className="ml-1 h-8 w-8 text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                >
                  <Trash2 size={16} />
                </Button>
              </div>
            </div>
          ))}
          {questions.length === 0 && (
            <div className="border-2 border-dashed border-border bg-card p-10 text-center text-sm font-medium text-muted-foreground">
              Список вопросов пуст. Добавьте первый вопрос выше.
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
