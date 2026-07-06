import type { DictionaryItem, StatisticsFilters } from "@/api";

type LinkedFilterField =
  | "departmentId"
  | "disciplineId"
  | "specialityId"
  | "specializationId";

export interface LinkedFilterSources {
  departments?: DictionaryItem[];
  disciplines?: DictionaryItem[];
  specialities?: DictionaryItem[];
  specializations?: DictionaryItem[];
}

export interface LinkedFilterOptions {
  departments: DictionaryItem[];
  disciplines: DictionaryItem[];
  specialities: DictionaryItem[];
  specializations: DictionaryItem[];
}

const linkedFilterFields: LinkedFilterField[] = [
  "departmentId",
  "disciplineId",
  "specialityId",
  "specializationId",
];

export function getLinkedFilterOptions(
  filters: Pick<
    StatisticsFilters,
    "departmentId" | "disciplineId" | "specialityId" | "specializationId"
  >,
  sources: LinkedFilterSources,
): LinkedFilterOptions {
  const departments = sources.departments ?? [];
  const disciplines = sources.disciplines ?? [];
  const specialities = sources.specialities ?? [];
  const specializations = sources.specializations ?? [];

  const selectedDiscipline = filters.disciplineId
    ? disciplines.find((item) => item.id === filters.disciplineId)
    : undefined;
  const selectedSpecialization = filters.specializationId
    ? specializations.find((item) => item.id === filters.specializationId)
    : undefined;

  const availableDepartments = selectedDiscipline
    ? departments.filter(
        (item) => item.id === selectedDiscipline.departmentId,
      )
    : departments;

  const availableDisciplines = filters.departmentId
    ? disciplines.filter((item) => item.departmentId === filters.departmentId)
    : disciplines;

  const availableSpecialities = selectedSpecialization
    ? specialities.filter((item) => item.id === selectedSpecialization.specialityId)
    : specialities;

  const availableSpecializations = filters.specialityId
    ? specializations.filter((item) => item.specialityId === filters.specialityId)
    : specializations;

  return {
    departments: availableDepartments,
    disciplines: availableDisciplines,
    specialities: availableSpecialities,
    specializations: availableSpecializations,
  };
}

export function sanitizeLinkedFilters<T extends StatisticsFilters>(
  filters: T,
  sources: LinkedFilterSources,
): T {
  let nextFilters = { ...filters };
  let hasChanges = true;

  while (hasChanges) {
    hasChanges = false;
    const options = getLinkedFilterOptions(nextFilters, sources);

    const allowedByField: Record<LinkedFilterField, DictionaryItem[]> = {
      departmentId: options.departments,
      disciplineId: options.disciplines,
      specialityId: options.specialities,
      specializationId: options.specializations,
    };

    linkedFilterFields.forEach((field) => {
      const value = nextFilters[field];
      if (!value) {
        return;
      }

      const isAllowed = allowedByField[field].some((item) => item.id === value);
      if (!isAllowed) {
        nextFilters = { ...nextFilters, [field]: undefined };
        hasChanges = true;
      }
    });
  }

  return nextFilters;
}
