import React, { useEffect, useState } from "react";
import { dictionariesApi, type DictionaryItem, type TeacherItem } from "../api";

interface Props {
  requiredFilters: string[] | undefined;
  onChange: (context: SubmissionContext) => void;
}

export interface SubmissionContext {
  disciplineId?: string;
  teacherId?: string;
  departmentId?: string;
  educationForm: string;
}

export const ContextSelector: React.FC<Props> = ({
  requiredFilters,
  onChange,
}) => {
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);

  // Локальный стейт выбора
  const [context, setContext] = useState<SubmissionContext>({
    educationForm: "ДФПО",
  });

  // 1. Загружаем данные при появлении компонента
  useEffect(() => {
    const loadData = async () => {
      if (!requiredFilters) return;

      try {
        // Загружаем только то, что нужно анкете
        if (requiredFilters.includes("Teacher")) {
          const res = await dictionariesApi.getTeachers();
          setTeachers(res.data);
        }
        if (requiredFilters.includes("Discipline")) {
          const res = await dictionariesApi.getDisciplines();
          setDisciplines(res.data);
        }
      } catch (e) {
        console.error("Ошибка загрузки справочников", e);
      }
    };
    loadData();
  }, [requiredFilters]);

  // 2. Сообщаем родительскому компоненту об изменениях
  useEffect(() => {
    onChange(context);
  }, [context, onChange]);

  const handleChange = (field: keyof SubmissionContext, value: string) => {
    setContext((prev) => ({ ...prev, [field]: value }));
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow-sm border border-blue-100 mb-6">
      <h3 className="font-semibold mb-4 text-blue-900">Данные для анкеты</h3>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Форма обучения (всегда) */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Форма обучения
          </label>
          <select
            className="input-field"
            value={context.educationForm}
            onChange={(e) => handleChange("educationForm", e.target.value)}
          >
            <option value="ДФПО">Дневная (ДФПО)</option>
            <option value="ЗФПО">Заочная (ЗФПО)</option>
          </select>
        </div>

        {/* Дисциплина (если требуется) */}
        {requiredFilters?.includes("Discipline") && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Дисциплина
            </label>
            <select
              className="input-field"
              value={context.disciplineId || ""}
              onChange={(e) => handleChange("disciplineId", e.target.value)}
            >
              <option value="">-- Выберите дисциплину --</option>
              {disciplines.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </div>
        )}

        {/* Преподаватель (если требуется) */}
        {requiredFilters?.includes("Teacher") && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Преподаватель
            </label>
            <select
              className="input-field"
              value={context.teacherId || ""}
              onChange={(e) => handleChange("teacherId", e.target.value)}
            >
              <option value="">-- Выберите преподавателя --</option>
              {teachers.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.fullName}
                </option>
              ))}
            </select>
          </div>
        )}
      </div>
    </div>
  );
};
