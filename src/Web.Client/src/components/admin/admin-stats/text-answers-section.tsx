import { useMemo, useState } from "react";
import { format, parseISO } from "date-fns";
import { ru } from "date-fns/locale";

import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { TextAnswerItem } from "../../../api";

type GroupBy = "none" | "teacher" | "department" | "discipline";

const GROUP_OPTIONS: { value: GroupBy; label: string }[] = [
  { value: "none", label: "Без группировки" },
  { value: "teacher", label: "По преподавателю" },
  { value: "department", label: "По кафедре" },
  { value: "discipline", label: "По дисциплине" },
];

const UNSPECIFIED = "Не указано";

function getGroupName(answer: TextAnswerItem, groupBy: GroupBy): string {
  switch (groupBy) {
    case "teacher":
      return answer.teacherName ?? UNSPECIFIED;
    case "department":
      return answer.departmentName ?? UNSPECIFIED;
    case "discipline":
      return answer.disciplineName ?? UNSPECIFIED;
    default:
      return "";
  }
}

function AnswerCard({ answer }: { answer: TextAnswerItem }) {
  return (
    <Card className="gap-3 py-5">
      <CardContent className="space-y-3">
        <div className="flex flex-wrap items-center gap-2">
          <Badge variant="outline">{answer.questionText}</Badge>
          {answer.teacherName ? (
            <Badge variant="secondary">{answer.teacherName}</Badge>
          ) : null}
          {answer.departmentName ? (
            <Badge variant="secondary">{answer.departmentName}</Badge>
          ) : null}
          {answer.disciplineName ? (
            <Badge variant="secondary">{answer.disciplineName}</Badge>
          ) : null}
          <span className="ml-auto text-xs text-muted-foreground">
            {format(parseISO(answer.submittedAt), "PPP", { locale: ru })}
          </span>
        </div>
        <p className="text-sm leading-6 text-foreground whitespace-pre-wrap">
          {answer.value}
        </p>
      </CardContent>
    </Card>
  );
}

export function TextAnswersSection({ answers }: { answers: TextAnswerItem[] }) {
  const [groupBy, setGroupBy] = useState<GroupBy>("none");

  const groups = useMemo(() => {
    if (groupBy === "none") {
      return [{ name: "", items: answers }];
    }

    const buckets = new Map<string, TextAnswerItem[]>();

    for (const answer of answers) {
      const name = getGroupName(answer, groupBy);
      const bucket = buckets.get(name);

      if (bucket) {
        bucket.push(answer);
      } else {
        buckets.set(name, [answer]);
      }
    }

    return [...buckets.entries()]
      .map(([name, items]) => ({ name, items }))
      .sort((a, b) => {
        // "Не указано" всегда в конце списка групп.
        if (a.name === UNSPECIFIED) return 1;
        if (b.name === UNSPECIFIED) return -1;
        return a.name.localeCompare(b.name, "ru");
      });
  }, [answers, groupBy]);

  return (
    <div className="mb-8 overflow-hidden border bg-card shadow-sm">
      <div className="flex flex-wrap items-center gap-3 border-b px-4 py-4">
        <h3 className="text-base font-bold text-foreground md:text-lg">
          Текстовые ответы
        </h3>
        {answers.length > 0 ? (
          <Select
            value={groupBy}
            onValueChange={(value) => setGroupBy(value as GroupBy)}
          >
            <SelectTrigger className="ml-auto w-56" size="sm">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {GROUP_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        ) : null}
      </div>

      {answers.length > 0 ? (
        <div className="space-y-6 p-4">
          {groups.map((group) => (
            <div key={group.name} className="space-y-4">
              {group.name ? (
                <div className="flex items-center gap-2">
                  <h4 className="text-sm font-medium text-foreground">
                    {group.name}
                  </h4>
                  <Badge variant="outline">{group.items.length}</Badge>
                </div>
              ) : null}
              {group.items.map((answer, index) => (
                <AnswerCard
                  key={`${answer.questionId}-${answer.submittedAt}-${index}`}
                  answer={answer}
                />
              ))}
            </div>
          ))}
        </div>
      ) : (
        <div className="p-10 text-center text-muted-foreground">
          Текстовые ответы по выбранным фильтрам не найдены.
        </div>
      )}
    </div>
  );
}
