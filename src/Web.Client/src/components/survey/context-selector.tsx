import React, { useEffect, useState } from "react";
import {
  dictionariesApi,
  type DictionaryItem,
  type TeacherItem,
} from "../../api";
import { getLinkedFilterOptions } from "@/utils/linked-filters";

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
  const [departments, setDepartments] = useState<DictionaryItem[]>([]);
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [disciplines, setDisciplines] = useState<DictionaryItem[]>([]);

  const [context, setContext] = useState<SubmissionContext>({
    educationForm: "ДФПО",
  });

  useEffect(() => {
    const loadData = async () => {
      if (!requiredFilters) return;

      try {
        if (requiredFilters.includes("Department")) {
          const res = await dictionariesApi.getDepartments();
          setDepartments(res.data.filter((item) => !item.isDeleted));
        }
        if (requiredFilters.includes("Teacher")) {
          const res = await dictionariesApi.getTeachers();
          setTeachers(res.data.filter((item) => !item.isDeleted));
        }
        if (requiredFilters.includes("Discipline")) {
          const res = await dictionariesApi.getDisciplines();
          setDisciplines(res.data.filter((item) => !item.isDeleted));
        }
      } catch (e) {
        console.error("Ошибка загрузки справочников", e);
      }
    };
    loadData();
  }, [requiredFilters]);

  useEffect(() => {
    onChange(context);
  }, [context, onChange]);

  const handleChange = (field: keyof SubmissionContext, value: string) => {
    setContext((prev) => {
      if (field === "departmentId" || field === "disciplineId") {
        const nextContext = {
          ...prev,
          [field]: value || undefined,
        };

        const selectedDiscipline = nextContext.disciplineId
          ? disciplines.find(
              (discipline) => discipline.id === nextContext.disciplineId,
            )
          : undefined;

        if (field === "disciplineId" && selectedDiscipline?.departmentId) {
          nextContext.departmentId = selectedDiscipline.departmentId;
        }

        const options = getLinkedFilterOptions(nextContext, {
          departments,
          disciplines,
        });

        if (
          nextContext.departmentId &&
          !options.departments.some(
            (department) => department.id === nextContext.departmentId,
          )
        ) {
          nextContext.departmentId = undefined;
        }

        if (
          nextContext.disciplineId &&
          !options.disciplines.some(
            (discipline) => discipline.id === nextContext.disciplineId,
          )
        ) {
          nextContext.disciplineId = undefined;
        }

        return nextContext;
      }

      return { ...prev, [field]: value || undefined };
    });
  };

  const linkedOptions = getLinkedFilterOptions(context, {
    departments,
    disciplines,
  });

  return (
    <div className="bg-white p-6 shadow-sm border border-blue-100 mb-6">
      <h3 className="font-semibold mb-4 text-blue-900">Данные для анкеты</h3>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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

        {requiredFilters?.includes("Department") && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Кафедра
            </label>
            <select
              className="input-field"
              value={context.departmentId || ""}
              onChange={(e) => handleChange("departmentId", e.target.value)}
            >
              <option value="">-- Выберите кафедру --</option>
              {linkedOptions.departments.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </div>
        )}

        {requiredFilters?.includes("Discipline") && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Дисциплина
            </label>
            <select
              className="input-field"
              value={context.disciplineId || ""}
              onChange={(e) => handleChange("disciplineId", e.target.value)}
              disabled={linkedOptions.disciplines.length === 0}
            >
              <option value="">-- Выберите дисциплину --</option>
              {linkedOptions.disciplines.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </div>
        )}

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
