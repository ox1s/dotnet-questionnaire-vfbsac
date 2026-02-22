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

const QuestionType = {
  Text: 1,
  Number: 2,
  WeightedRating: 6,
} as const;

type QuestionType = (typeof QuestionType)[keyof typeof QuestionType];

type FilterField = "Department" | "Discipline" | "Teacher" | "Speciality";

interface QuestionDraft {
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
    if (!newQText.trim()) {
      alert("Введите текст вопроса");
      return;
    }
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

    // Меняем элементы местами
    [newQs[index], newQs[swapIdx]] = [newQs[swapIdx], newQs[index]];

    // Пересчитываем order
    setQuestions(newQs.map((q, i) => ({ ...q, order: i + 1 })));
  };
  // Начало перетаскивания
  const handleDragStart = (index: number) => {
    setDraggedIdx(index);
  };

  // Элемент пролетает над другим элементом
  const handleDragEnter = (index: number) => {
    if (draggedIdx === null || draggedIdx === index) return;

    // Меняем элементы местами
    const newQuestions = [...questions];
    const draggedItem = newQuestions.splice(draggedIdx, 1)[0];
    newQuestions.splice(index, 0, draggedItem);

    // Пересчитываем поле order (1, 2, 3...)
    const updated = newQuestions.map((q, i) => ({ ...q, order: i + 1 }));

    setDraggedIdx(index); // Обновляем индекс, так как элемент сдвинулся
    setQuestions(updated);
  };

  // Конец перетаскивания (отпустили мышь)
  const handleDragEnd = () => {
    setDraggedIdx(null);
  };

  const handleSave = async () => {
    if (!title.trim() || questions.length === 0) {
      alert("Заполните название и добавьте вопросы");
      return;
    }
    try {
      await api.post("/forms", {
        title,
        requiredFilters: selectedFilters,
        questions,
      });
      navigate("/dashboard");
    } catch (e) {
      alert("Ошибка сохранения");
    }
  };

  return (
    <AdminLayout
      title="Конструктор анкет"
      subtitle="Создание новой формы опроса."
      actions={
        <button
          onClick={handleSave}
          className="flex items-center gap-2 px-6 py-2.5 bg-green-600 text-white rounded-xl hover:bg-green-700 font-bold shadow-lg shadow-green-600/20 active:scale-95 transition-all text-sm"
        >
          <Save size={18} /> Сохранить анкету
        </button>
      }
    >
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Левая колонка: Настройки */}
        <div className="lg:col-span-1 space-y-6">
          <div className="bg-white p-6 rounded-2xl shadow-sm border border-slate-200">
            <h3 className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-4">
              Настройки
            </h3>

            <div className="mb-6">
              <label className="block text-sm font-bold text-slate-700 mb-2">
                Название анкеты
              </label>
              <textarea
                rows={3}
                className="w-full p-3 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:ring-2 focus:ring-slate-900/10 focus:border-slate-900 outline-none resize-none"
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
                          ? "bg-slate-800 border-slate-800 text-white shadow-md"
                          : "bg-white border-slate-200 text-slate-600 hover:bg-slate-50"
                      }`}
                    >
                      {opt.label}
                      {active && (
                        <CheckCircle2 size={16} className="text-green-400" />
                      )}
                    </button>
                  );
                })}
              </div>
            </div>
          </div>
        </div>

        {/* Правая колонка: Вопросы */}
        <div className="lg:col-span-2 space-y-6">
          {/* Добавление вопроса */}
          <div className="bg-white p-6 rounded-2xl shadow-sm border border-slate-200">
            <h3 className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-4">
              Новый вопрос
            </h3>
            <div className="flex flex-col gap-4">
              <input
                type="text"
                className="w-full p-3.5 bg-slate-50 border border-slate-200 rounded-xl text-sm font-medium focus:ring-2 focus:ring-slate-900/10 outline-none"
                placeholder="Введите текст вопроса..."
                value={newQText}
                onChange={(e) => setNewQText(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && addQuestion()}
              />
              
              {/* ИСПРАВЛЕНИЕ ТУТ: flex-col для телефонов, sm:flex-row для ПК */}
              <div className="flex flex-col sm:flex-row gap-3">
                <select
                  className="w-full sm:flex-1 p-3 bg-slate-50 border border-slate-200 rounded-xl text-sm font-medium focus:ring-2 focus:ring-slate-900/10 outline-none"
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
                  className="w-full sm:w-auto px-6 py-3 bg-slate-800 text-white rounded-xl font-bold text-sm hover:bg-slate-900 hover:shadow-lg transition-all flex items-center justify-center gap-2 whitespace-nowrap"
                >
                  <Plus size={18} /> Добавить
                </button>
              </div>
            </div>
          </div>

          {/* Список вопросов */}
          <div className="space-y-3">
            {questions.map((q, idx) => (
              <div
                key={q.order}
                draggable
                onDragStart={() => handleDragStart(idx)}
                onDragEnter={() => handleDragEnter(idx)}
                onDragEnd={handleDragEnd}
                onDragOver={(e) => e.preventDefault()}
                className={`group flex items-center gap-3 p-4 bg-white border border-slate-200 rounded-2xl transition-all ${
                  draggedIdx === idx ? "opacity-40 shadow-inner bg-slate-50" : "hover:border-slate-300 hover:shadow-md"
                }`}
              >
                {/* Иконка перетаскивания только для ПК */}
                <div className="hidden sm:block cursor-move text-slate-300 hover:text-slate-500">
                  <GripVertical size={20} />
                </div>
                
                <div className="w-8 h-8 rounded-lg bg-slate-100 flex items-center justify-center text-xs font-bold text-slate-600 shrink-0">
                  {q.order}
                </div>
                
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-bold text-slate-800 truncate">{q.text}</p>
                  <span className="inline-block mt-1 text-[10px] uppercase font-bold text-slate-400 bg-slate-50 px-2 py-0.5 rounded border border-slate-100">
                    {QUESTION_TYPES.find((t) => t.value === q.type)?.label}
                  </span>
                </div>
                
                {/* Панель управления вопросом (Стрелки + Удалить) */}
                <div className="flex items-center gap-1 sm:gap-2">
                  <div className="flex flex-col sm:flex-row">
                    <button 
                      onClick={() => moveQuestion(idx, 'up')} 
                      disabled={idx === 0}
                      className="p-1 sm:p-2 text-slate-400 hover:text-primary disabled:opacity-20"
                    >
                      <ChevronUp size={20} />
                    </button>
                    <button 
                      onClick={() => moveQuestion(idx, 'down')} 
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
              <div className="p-10 text-center border-2 border-dashed border-slate-200 rounded-2xl text-slate-400">
                Список вопросов пуст
              </div>
            )}
          </div>
        </div>
      </div>
    </AdminLayout>
  );
};
