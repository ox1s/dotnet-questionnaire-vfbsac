import { format, parseISO } from "date-fns";
import { ru } from "date-fns/locale";

import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import type { TextAnswerItem } from "../../../api";

export function TextAnswersSection({ answers }: { answers: TextAnswerItem[] }) {
  return (
    <div className="mb-8 overflow-hidden border bg-card shadow-sm">
      <div className="border-b px-4 py-4">
        <h3 className="text-base font-bold text-foreground md:text-lg">
          Текстовые ответы
        </h3>
      </div>

      {answers.length > 0 ? (
        <div className="space-y-4 p-4">
          {answers.map((answer, index) => (
            <Card key={`${answer.questionId}-${index}`} className="gap-3 py-5">
              <CardContent className="space-y-3">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge variant="outline">{answer.questionText}</Badge>
                  {answer.teacherName ? (
                    <Badge variant="secondary">{answer.teacherName}</Badge>
                  ) : null}
                  {answer.departmentName ? (
                    <Badge variant="secondary">{answer.departmentName}</Badge>
                  ) : null}
                  <span className="ml-auto text-xs text-muted-foreground">
                    {format(parseISO(answer.submittedAt), "PPP", {
                      locale: ru,
                    })}
                  </span>
                </div>
                <p className="text-sm leading-6 text-foreground whitespace-pre-wrap">
                  {answer.value}
                </p>
              </CardContent>
            </Card>
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
