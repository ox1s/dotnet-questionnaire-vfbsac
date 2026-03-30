import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api";
import {
  Plus,
  Trash2,
  Save,
  GripVertical,
  CheckCircle2,
  ChevronUp,
  ChevronDown,
} from "lucide-react";
import { AdminLayout } from "../layouts/AdminLayout";
import toast from "react-hot-toast";

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

  const handleSave = async () => {
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
  };

  return (
    <AdminLayout
      title="Конструктор анкет"
      subtitle="Создание новой формы опроса."
      actions={
        <button
          onClick={handleSave}
          className="flex items-center justify-center gap-2 w-full md:w-auto px-6 py-2.5 bg-green-600 text-white rounded-xl hover:bg-green-700 font-bold shadow-lg shadow-green-600/20 active:scale-95 transition-all text-sm whitespace-nowrap"
        >
          <Save size={18} /> Сохранить анкету
        </button>
      }
    >
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-1 space-y-6">
          <div className="bg-surface-light p-6 rounded-2xl shadow-sm border border-slate-200">
            <h3 className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-4">
              Настройки
            </h3>

            <div className="mb-6">
              <label className="block text-sm font-bold text-slate-700 mb-2">
                Название анкеты
              </label>
              <textarea
                rows={3}
                className="input-field resize-none bg-background-light"
                placeholder="Например: Удовлетворенность качеством преподавания..."
                value={title}
                onChange={(e) => setTitle(e.target.value)}
              />
            </div>

            <div>
              <label className="block text-sm font-bold text-slate-700 mb-3">
                Контекст (Фильтры)
                <span className="block text-xs font-normal text-slate-400 mt-1">
                  Что выбирает студент перед началом?
                </span>
              </label>
              <div className="space-y-2">
                {FILTER_OPTIONS.map((opt) => {
                  const active = selectedFilters.includes(opt.key);
                  return (
                    <button
                      key={opt.key}
                      onClick={() => toggleFilter(opt.key)}
                      className={`flex w-full items-center justify-between px-4 py-3 rounded-xl border transition-all text-sm font-medium ${
                        active
                          ? "bg-primary border-primary text-white shadow-md shadow-primary/20"
                          : "bg-white border-slate-200 text-slate-600 hover:bg-slate-50"
                      }`}
                    >
                      {opt.label}
                      {active && (
                        <CheckCircle2 size={16} className="text-white" />
                      )}
                    </button>
                  );
                })}
              </div>
            </div>
          </div>
        </div>

        <div className="lg:col-span-2 space-y-6">
          <div className="bg-surface-light p-6 rounded-2xl shadow-sm border border-slate-200">
            <h3 className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-4">
              Новый вопрос
            </h3>
            <div className="flex flex-col gap-4">
              <input
                type="text"
                className="input-field bg-background-light"
                placeholder="Введите текст вопроса..."
                value={newQText}
                onChange={(e) => setNewQText(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && addQuestion()}
              />

              <div className="flex flex-col sm:flex-row gap-3">
                <select
                  className="input-field bg-background-light w-full sm:flex-1"
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

                <button
                  onClick={addQuestion}
                  className="w-full sm:w-auto px-6 py-2.5 bg-slate-800 text-white rounded-xl font-bold text-sm hover:bg-slate-900 transition-all flex items-center justify-center gap-2 whitespace-nowrap"
                >
                  <Plus size={18} /> Добавить
                </button>
              </div>
            </div>
          </div>

          <div className="space-y-3">
            {questions.map((q, idx) => (
              <div
                key={q.order}
                draggable
                onDragStart={() => handleDragStart(idx)}
                onDragEnter={() => handleDragEnter(idx)}
                onDragEnd={handleDragEnd}
                onDragOver={(e) => e.preventDefault()}
                className={`group flex items-center gap-3 p-4 bg-surface-light border border-slate-200 rounded-2xl transition-all ${
                  draggedIdx === idx
                    ? "opacity-40 shadow-inner bg-slate-50"
                    : ""
                } hover:border-slate-300 hover:shadow-md cursor-grab`}
              >
                <div className="hidden sm:block text-slate-300 hover:text-slate-500">
                  <GripVertical size={20} />
                </div>

                <div className="w-8 h-8 rounded-lg bg-slate-100 flex items-center justify-center text-xs font-bold text-slate-600 shrink-0">
                  {q.order}
                </div>

                <div className="flex-1 min-w-0">
                  <p className="text-sm font-bold text-slate-800 truncate">
                    {q.text}
                  </p>
                  <span className="inline-block mt-1 text-[10px] uppercase font-bold text-slate-400 bg-slate-50 px-2 py-0.5 rounded border border-slate-100">
                    {QUESTION_TYPES.find((t) => t.value === q.type)?.label ||
                      "Вопрос"}
                  </span>
                </div>

                <div className="flex items-center gap-1 sm:gap-2">
                  <div className="flex flex-col sm:flex-row">
                    <button
                      onClick={() => moveQuestion(idx, "up")}
                      disabled={idx === 0}
                      className="p-1 sm:p-2 text-slate-400 hover:text-primary disabled:opacity-20"
                    >
                      <ChevronUp size={20} />
                    </button>
                    <button
                      onClick={() => moveQuestion(idx, "down")}
                      disabled={idx === questions.length - 1}
                      className="p-1 sm:p-2 text-slate-400 hover:text-primary disabled:opacity-20"
                    >
                      <ChevronDown size={20} />
                    </button>
                  </div>
                  <button
                    onClick={() => removeQuestion(idx)}
                    className="p-2 text-slate-300 hover:text-accent hover:bg-accent/10 rounded-lg transition-colors ml-1"
                  >
                    <Trash2 size={18} />
                  </button>
                </div>
              </div>
            ))}

            {questions.length === 0 && (
              <div className="p-10 text-center border-2 border-dashed border-slate-200 rounded-2xl text-slate-400 text-sm font-medium bg-surface-light">
                Список вопросов пуст. Добавьте первый вопрос выше.
              </div>
            )}
          </div>
        </div>
      </div>
    </AdminLayout>
  );
};
