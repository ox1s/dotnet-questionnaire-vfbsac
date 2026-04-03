import React, { useEffect, useState } from "react";
import {
  dictionariesApi,
  type DictionaryItem,
  type TeacherItem,
} from "../../api";
import { getLinkedFilterOptions } from "@/utils/linked-filters";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

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
  const EMPTY_VALUE = "__empty__";
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

        if (field === "departmentId" && value !== prev.departmentId) {
          nextContext.disciplineId = undefined;
        }

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
    <div className="mb-6 border bg-card p-6 shadow-sm">
      <h3 className="mb-4 text-base font-semibold text-foreground">
        Данные для анкеты
      </h3>
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <ContextField label="Форма обучения">
          <Select
            value={context.educationForm}
            onValueChange={(value) => handleChange("educationForm", value)}
          >
            <SelectTrigger className="w-full bg-background text-sm">
              <SelectValue placeholder="Выберите форму обучения" />
            </SelectTrigger>
            <SelectContent position="popper">
              <SelectItem value="ДФПО">Дневная (ДФПО)</SelectItem>
              <SelectItem value="ЗФПО">Заочная (ЗФПО)</SelectItem>
            </SelectContent>
          </Select>
        </ContextField>

        {requiredFilters?.includes("Department") && (
          <ContextField label="Кафедра">
            <Select
              value={context.departmentId ?? EMPTY_VALUE}
              onValueChange={(value) =>
                handleChange("departmentId", value === EMPTY_VALUE ? "" : value)
              }
            >
              <SelectTrigger className="w-full bg-background text-sm">
                <SelectValue placeholder="Выберите кафедру" />
              </SelectTrigger>
              <SelectContent position="popper">
                <SelectItem value={EMPTY_VALUE}>Не выбрано</SelectItem>
                {linkedOptions.departments.map((d) => (
                  <SelectItem key={d.id} value={d.id}>
                    {d.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </ContextField>
        )}

        {requiredFilters?.includes("Discipline") && (
          <ContextField label="Дисциплина">
            <Select
              value={context.disciplineId ?? EMPTY_VALUE}
              onValueChange={(value) =>
                handleChange("disciplineId", value === EMPTY_VALUE ? "" : value)
              }
              disabled={linkedOptions.disciplines.length === 0}
            >
              <SelectTrigger className="w-full bg-background text-sm">
                <SelectValue placeholder="Выберите дисциплину" />
              </SelectTrigger>
              <SelectContent position="popper">
                <SelectItem value={EMPTY_VALUE}>Не выбрано</SelectItem>
                {linkedOptions.disciplines.map((d) => (
                  <SelectItem key={d.id} value={d.id}>
                    {d.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </ContextField>
        )}

        {requiredFilters?.includes("Teacher") && (
          <ContextField label="Преподаватель">
            <Select
              value={context.teacherId ?? EMPTY_VALUE}
              onValueChange={(value) =>
                handleChange("teacherId", value === EMPTY_VALUE ? "" : value)
              }
            >
              <SelectTrigger className="w-full bg-background text-sm">
                <SelectValue placeholder="Выберите преподавателя" />
              </SelectTrigger>
              <SelectContent position="popper">
                <SelectItem value={EMPTY_VALUE}>Не выбрано</SelectItem>
                {teachers.map((t) => (
                  <SelectItem key={t.id} value={t.id}>
                    {t.fullName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </ContextField>
        )}
      </div>
    </div>
  );
};

function ContextField({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-2">
      <Label className="text-xs text-muted-foreground">{label}</Label>
      {children}
    </div>
  );
}
