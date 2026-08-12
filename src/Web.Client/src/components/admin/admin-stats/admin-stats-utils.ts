import { format } from "date-fns";
import type { SatisfactionRating } from "../../../api";

export type Mode = "single" | "periods" | "groups";

export type CompareField =
  | "departmentId"
  | "specialityId"
  | "specializationId"
  | "disciplineId"
  | "teacherId";

export type RangeState = {
  id: string;
  label: string;
  dateFrom: string;
  dateTo: string;
};

export const ratingLabels: Record<SatisfactionRating, string> = {
  Excellent: "отлично",
  Good: "хорошо",
  Satisfactory: "удовлетворительно",
  Unsatisfactory: "неудовлетворительно",
};

export function asDateInput(date: Date) {
  return format(date, "yyyy-MM-dd");
}

export function createRangeState(
  label: string,
  dateFrom: string,
  dateTo: string,
): RangeState {
  return {
    id: crypto.randomUUID(),
    label,
    dateFrom,
    dateTo,
  };
}

export function getSemesterRange(): RangeState {
  const now = new Date();
  const start =
    now.getMonth() < 6
      ? new Date(now.getFullYear(), 0, 1)
      : new Date(now.getFullYear(), 6, 1);

  return createRangeState(
    "Текущий семестр",
    asDateInput(start),
    asDateInput(now),
  );
}

export function getPreviousPeriodRange(): RangeState {
  const now = new Date();

  return {
    id: crypto.randomUUID(),
    label: "Предыдущий период",
    dateFrom: asDateInput(
      new Date(now.getFullYear(), Math.max(now.getMonth() - 6, 0), 1),
    ),
    dateTo: asDateInput(now),
  };
}
