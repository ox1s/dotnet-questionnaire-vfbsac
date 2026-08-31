import type { Dispatch, SetStateAction } from "react";
import { Filter, RefreshCw } from "lucide-react";
import { FilterSelect } from "@/components/shared/filter-select";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { StatisticsFilters, TeacherItem } from "../../../api";
import type { LinkedFilterOptions } from "@/utils/linked-filters";
import { AnalyticsField } from "./analytics-field";
import { DatePickerInput } from "./date-picker-input";
import {
  createRangeState,
  type CompareField,
  type Mode,
  type RangeState,
} from "./admin-stats-utils";

const compareFieldOptions: { value: CompareField; label: string }[] = [
  { value: "departmentId", label: "Кафедры" },
  { value: "specialityId", label: "Специальности" },
  { value: "specializationId", label: "Специализации" },
  { value: "disciplineId", label: "Дисциплины" },
  { value: "teacherId", label: "Преподаватели" },
];

const EDUCATION_FORM_OPTIONS = [
  { id: "ДФПО", label: "Дневная (ДФПО)" },
  { id: "ЗФПО", label: "Заочная (ЗФПО)" },
];

export function AnalyticsFilterPanel({
  mode,
  setMode,
  singleRange,
  setSingleRange,
  periods,
  setPeriods,
  updatePeriod,
  filters,
  updateFilter,
  teachers,
  teacherLabel,
  availableLinkedOptions,
  compareField,
  setCompareField,
  selectedIds,
  setSelectedIds,
  optionsFor,
  refreshing,
  onRefresh,
  showOrganizationFilter,
}: {
  mode: Mode;
  setMode: (mode: Mode) => void;
  singleRange: RangeState;
  setSingleRange: Dispatch<SetStateAction<RangeState>>;
  periods: RangeState[];
  setPeriods: Dispatch<SetStateAction<RangeState[]>>;
  updatePeriod: (index: number, field: keyof RangeState, value: string) => void;
  filters: StatisticsFilters;
  updateFilter: (field: keyof StatisticsFilters, value: string) => void;
  teachers: TeacherItem[];
  teacherLabel: (teacher: TeacherItem) => string;
  availableLinkedOptions: LinkedFilterOptions;
  compareField: CompareField;
  setCompareField: (field: CompareField) => void;
  selectedIds: string[];
  setSelectedIds: Dispatch<SetStateAction<string[]>>;
  optionsFor: () => { value: string; label: string }[];
  refreshing: boolean;
  onRefresh: () => void;
  // Only Employer-targeted forms ever populate OrganizationName (see
  // CreateSubmissionCommandHandler), so the filter is meaningless noise on
  // any other form's stats page.
  showOrganizationFilter: boolean;
}) {
  return (
    <div className="mb-6 border bg-card p-6 shadow-sm">
      <div className="mb-5 flex items-center gap-2 font-bold text-foreground">
        <Filter size={18} />
        <h4>Режим аналитики</h4>
      </div>
      <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-3">
        <Button
          variant={mode === "single" ? "default" : "outline"}
          onClick={() => setMode("single")}
          size="lg"
        >
          Статистика за период
        </Button>
        <Button
          variant={mode === "periods" ? "default" : "outline"}
          onClick={() => setMode("periods")}
          size="lg"
        >
          Сравнение периодов
        </Button>
        <Button
          variant={mode === "groups" ? "default" : "outline"}
          onClick={() => setMode("groups")}
          size="lg"
        >
          Сравнение групп
        </Button>
      </div>

      {mode === "single" || mode === "groups" ? (
        <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-3">
          <AnalyticsField label="Название периода">
            <Input
              className="w-full text-sm"
              value={singleRange.label}
              placeholder="Название периода"
              onChange={(event) =>
                setSingleRange((previous) => ({
                  ...previous,
                  label: event.target.value,
                }))
              }
            />
          </AnalyticsField>
          <AnalyticsField label="Дата начала">
            <DatePickerInput
              value={singleRange.dateFrom}
              onChange={(val) =>
                setSingleRange((previous) => ({
                  ...previous,
                  dateFrom: val,
                }))
              }
            />
          </AnalyticsField>
          <AnalyticsField label="Дата окончания">
            <DatePickerInput
              value={singleRange.dateTo}
              onChange={(val) =>
                setSingleRange((previous) => ({
                  ...previous,
                  dateTo: val,
                }))
              }
            />
          </AnalyticsField>
        </div>
      ) : (
        <div className="mb-6 space-y-4">
          {periods.map((item, index) => (
            <div
              key={item.id}
              className="grid grid-cols-1 gap-4 md:grid-cols-4"
            >
              <AnalyticsField label="Название периода">
                <Input
                  className="bg-background"
                  value={item.label}
                  placeholder={`Период ${index + 1}`}
                  onChange={(event) =>
                    updatePeriod(index, "label", event.target.value)
                  }
                />
              </AnalyticsField>
              <AnalyticsField label="Дата начала">
                <DatePickerInput
                  value={item.dateFrom}
                  onChange={(val) => updatePeriod(index, "dateFrom", val)}
                />
              </AnalyticsField>
              <AnalyticsField label="Дата окончания">
                <DatePickerInput
                  value={item.dateTo}
                  onChange={(val) => updatePeriod(index, "dateTo", val)}
                />
              </AnalyticsField>
              <AnalyticsField label="Действие">
                <Button
                  variant="outline"
                  onClick={() =>
                    setPeriods((previous) =>
                      previous.length > 1
                        ? previous.filter(
                            (_, itemIndex) => itemIndex !== index,
                          )
                        : previous,
                    )
                  }
                  className="w-full justify-center"
                >
                  Удалить
                </Button>
              </AnalyticsField>
            </div>
          ))}
          <Button
            variant="outline"
            className="w-full sm:w-auto"
            onClick={() =>
              setPeriods((previous) => [
                ...previous,
                createRangeState(
                  `Период ${previous.length + 1}`,
                  singleRange.dateFrom,
                  singleRange.dateTo,
                ),
              ])
            }
          >
            Добавить период
          </Button>
        </div>
      )}

      <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
        <AnalyticsField label="Преподаватель">
          <FilterSelect
            value={filters.teacherId}
            onChange={(val) => updateFilter("teacherId", val)}
            disabled={mode === "groups" && compareField === "teacherId"}
            placeholder="Все преподаватели"
            options={teachers.map((t) => ({
              id: t.id,
              label: teacherLabel(t),
            }))}
          />
        </AnalyticsField>

        <AnalyticsField label="Предмет">
          <FilterSelect
            value={filters.disciplineId}
            onChange={(val) => updateFilter("disciplineId", val)}
            disabled={mode === "groups" && compareField === "disciplineId"}
            placeholder="Все дисциплины"
            options={availableLinkedOptions.disciplines.map((d) => ({
              id: d.id,
              label: d.name,
            }))}
          />
        </AnalyticsField>

        <AnalyticsField label="Филиал кафедры">
          <FilterSelect
            value={filters.departmentId}
            onChange={(val) => updateFilter("departmentId", val)}
            disabled={mode === "groups" && compareField === "departmentId"}
            placeholder="Все кафедры"
            options={availableLinkedOptions.departments.map((d) => ({
              id: d.id,
              label: d.name,
            }))}
          />
        </AnalyticsField>

        <AnalyticsField label="Специальность">
          <FilterSelect
            value={filters.specialityId}
            onChange={(val) => updateFilter("specialityId", val)}
            disabled={mode === "groups" && compareField === "specialityId"}
            placeholder="Все специальности"
            options={availableLinkedOptions.specialities.map((s) => ({
              id: s.id,
              label: s.name,
            }))}
          />
        </AnalyticsField>

        <AnalyticsField label="Специализация">
          <FilterSelect
            value={filters.specializationId}
            onChange={(val) => updateFilter("specializationId", val)}
            disabled={
              mode === "groups" && compareField === "specializationId"
            }
            placeholder="Все специализации"
            options={availableLinkedOptions.specializations.map((s) => ({
              id: s.id,
              label: s.name,
            }))}
          />
        </AnalyticsField>

        <AnalyticsField label="Форма обучения">
          <FilterSelect
            value={filters.educationForm}
            onChange={(val) => updateFilter("educationForm", val)}
            placeholder="Все формы обучения"
            options={EDUCATION_FORM_OPTIONS}
          />
        </AnalyticsField>

        {showOrganizationFilter && (
          <AnalyticsField label="Организация">
            <Input
              type="text"
              className="w-full text-sm"
              placeholder="Название организации..."
              value={filters.organizationName || ""}
              onChange={(event) =>
                updateFilter("organizationName", event.target.value)
              }
            />
          </AnalyticsField>
        )}
      </div>

      {mode === "groups" ? (
        <div className="mb-6 grid grid-cols-1 gap-4 xl:grid-cols-3">
          <AnalyticsField label="Поле сравнения" className="xl:col-span-1">
            <Select
              value={compareField}
              onValueChange={(value) => {
                setCompareField(value as CompareField);
                setSelectedIds([]);
              }}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="Выберите поле" />
              </SelectTrigger>
              <SelectContent position="popper">
                {compareFieldOptions.map((item) => (
                  <SelectItem key={item.value} value={item.value}>
                    {item.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </AnalyticsField>

          <AnalyticsField
            label="Сравниваемые значения"
            className="xl:col-span-2"
          >
            <Card className="gap-3 border bg-muted/40 py-3">
              <CardContent className="space-y-2">
                {optionsFor().length > 0 ? (
                  optionsFor().map((item) => {
                    const selected = selectedIds.includes(item.value);

                    return (
                      <Button
                        key={item.value}
                        type="button"
                        variant={selected ? "default" : "outline"}
                        className="w-full justify-start"
                        onClick={() =>
                          setSelectedIds((previous) =>
                            previous.includes(item.value)
                              ? previous.filter(
                                  (current) => current !== item.value,
                                )
                              : [...previous, item.value],
                          )
                        }
                      >
                        {item.label}
                      </Button>
                    );
                  })
                ) : (
                  <p className="text-xs text-muted-foreground">
                    Нет доступных значений для выбранного поля.
                  </p>
                )}
              </CardContent>
            </Card>
          </AnalyticsField>
        </div>
      ) : null}

      <div className="flex justify-end pt-2">
        <Button
          onClick={onRefresh}
          disabled={refreshing}
          size="lg"
          variant="default"
        >
          {refreshing ? (
            <RefreshCw className="animate-spin mr-2" size={16} />
          ) : null}{" "}
          Обновить аналитику
        </Button>
      </div>
    </div>
  );
}
