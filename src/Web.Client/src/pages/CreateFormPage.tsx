import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api";
import { ArrowLeft, Plus, Trash2, Save, CheckSquare, Square } from "lucide-react";

// Типы, соответствующие бэкенду (C# Enum QuestionType)
enum QuestionType {
  Text = 1,
  Number = 2,
  // MultipleChoice = 3, // Пока не реализовано в UI
  // SingleChoice = 4,   // Пока не реализовано в UI
  // Rating = 5,
  WeightedRating = 6,
}

// Типы, соответствующие бэкенду (C# Enum FilterField)
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
  { value: QuestionType.WeightedRating, label: "Оценка с весом (Важность + Оценка)" },
  { value: QuestionType.Number, label: "Простая оценка (1-10)" },
  { value: QuestionType.Text, label: "Текстовый ответ" },
];

export const CreateFormPage = () => {
  const navigate = useNavigate();

  // Состояние формы
  const [title, setTitle] = useState("");
  const [selectedFilters, setSelectedFilters] = useState<FilterField[]>([]);
  const [questions, setQuestions] = useState<QuestionDraft[]>([]);

  // Состояние для нового вопроса (инпут)
  const [newQText, setNewQText] = useState("");
  const [newQType, setNewQType] = useState<QuestionType>(QuestionType.WeightedRating);

  // --- Хендлеры ---

  const toggleFilter = (filter: FilterField) => {
    setSelectedFilters((prev) =>
      prev.includes(filter)
        ? prev.filter((f) => f !== filter)
        : [...prev, filter]
    );
  };

  const addQuestion = () => {
    if (!newQText.trim()) {
      alert("Введите текст вопроса");
      return;
    }

    const newQuestion: QuestionDraft = {
      text: newQText,
      type: newQType,
      order: questions.length + 1,
    };

    setQuestions([...questions, newQuestion]);
    setNewQText(""); // Очистить инпут
    // Тип не сбрасываем, часто удобно добавлять однотипные вопросы подряд
  };

  const removeQuestion = (index: number) => {
    const updated = questions.filter((_, i) => i !== index);
    // Пересчитываем порядок (order), чтобы не было дырок
    const reordered = updated.map((q, i) => ({ ...q, order: i + 1 }));
    setQuestions(reordered);
  };

  const handleSave = async () => {
    if (!title.trim()) {
      alert("Укажите название анкеты");
      return;
    }
    if (questions.length === 0) {
      alert("Добавьте хотя бы один вопрос");
      return;
    }

    const payload = {
      title,
      requiredFilters: selectedFilters,
      questions: questions,
    };

    try {
      await api.post("/forms", payload);
      alert("Анкета успешно создана!");
      navigate("/dashboard");
    } catch (e) {
      console.error(e);
      alert("Ошибка при сохранении. Проверьте консоль.");
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 pb-20">
      {/* Header */}
      <header className="bg-white shadow-sm px-6 py-4 sticky top-0 z-10 border-b border-gray-200">
        <div className="max-w-4xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-4">
            <button
              onClick={() => navigate("/dashboard")}
              className="text-gray-500 hover:text-gray-800 transition"
            >
              <ArrowLeft />
            </button>
            <h1 className="text-xl font-bold text-gray-900">Конструктор анкет</h1>
          </div>
          <button
            onClick={handleSave}
            className="btn-primary flex items-center gap-2"
          >
            <Save size={18} /> Сохранить
          </button>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-8 space-y-8">
        
        {/* 1. Настройки анкеты */}
        <section className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h2 className="text-lg font-semibold mb-4 text-blue-900">1. Основная информация</h2>
          
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Название анкеты
            </label>
            <input
              type="text"
              className="input-field text-lg"
              placeholder="Например: Оценка качества преподавания"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Обязательный контекст (о ком/чем опрос?)
            </label>
            <div className="flex flex-wrap gap-3">
              {FILTER_OPTIONS.map((opt) => {
                const isSelected = selectedFilters.includes(opt.key);
                return (
                  <button
                    key={opt.key}
                    onClick={() => toggleFilter(opt.key)}
                    className={`flex items-center gap-2 px-4 py-2 rounded-md border transition-all ${
                      isSelected
                        ? "bg-blue-50 border-blue-500 text-blue-700 font-medium"
                        : "bg-white border-gray-300 text-gray-600 hover:border-gray-400"
                    }`}
                  >
                    {isSelected ? <CheckSquare size={18} /> : <Square size={18} />}
                    {opt.label}
                  </button>
                );
              })}
            </div>
            <p className="text-xs text-gray-400 mt-2">
              Пользователь должен будет выбрать эти данные перед началом опроса.
            </p>
          </div>
        </section>

        {/* 2. Конструктор вопросов */}
        <section className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h2 className="text-lg font-semibold mb-4 text-blue-900">2. Вопросы</h2>

          {/* Список добавленных */}
          <div className="space-y-3 mb-6">
            {questions.length === 0 && (
              <div className="text-center py-8 text-gray-400 border-2 border-dashed border-gray-200 rounded-lg">
                Вопросов пока нет. Добавьте первый вопрос ниже.
              </div>
            )}

            {questions.map((q, idx) => (
              <div
                key={idx}
                className="flex items-center justify-between p-4 bg-gray-50 border border-gray-200 rounded-lg group"
              >
                <div className="flex items-start gap-3">
                  <span className="flex-shrink-0 w-6 h-6 bg-blue-100 text-blue-700 rounded-full flex items-center justify-center text-xs font-bold mt-0.5">
                    {q.order}
                  </span>
                  <div>
                    <p className="font-medium text-gray-900">{q.text}</p>
                    <span className="text-xs text-gray-500 bg-gray-200 px-2 py-0.5 rounded">
                      {QUESTION_TYPES.find((t) => t.value === q.type)?.label}
                    </span>
                  </div>
                </div>
                <button
                  onClick={() => removeQuestion(idx)}
                  className="text-gray-400 hover:text-red-600 p-2 transition-colors"
                  title="Удалить вопрос"
                >
                  <Trash2 size={18} />
                </button>
              </div>
            ))}
          </div>

          {/* Форма добавления */}
          <div className="bg-blue-50 p-4 rounded-lg border border-blue-100">
            <h3 className="text-sm font-medium text-blue-900 mb-3">Новый вопрос</h3>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div className="md:col-span-3">
                <input
                  type="text"
                  className="input-field"
                  placeholder="Текст вопроса..."
                  value={newQText}
                  onChange={(e) => setNewQText(e.target.value)}
                  onKeyDown={(e) => e.key === "Enter" && addQuestion()}
                />
              </div>
              <div>
                <select
                  className="input-field"
                  value={newQType}
                  onChange={(e) => setNewQType(Number(e.target.value) as QuestionType)}
                >
                  {QUESTION_TYPES.map((t) => (
                    <option key={t.value} value={t.value}>
                      {t.label}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <button
              onClick={addQuestion}
              className="mt-3 w-full flex items-center justify-center gap-2 bg-white border border-blue-200 text-blue-700 hover:bg-blue-100 py-2 rounded-md transition font-medium text-sm"
            >
              <Plus size={16} /> Добавить в список
            </button>
          </div>
        </section>
      </main>
    </div>
  );
};
