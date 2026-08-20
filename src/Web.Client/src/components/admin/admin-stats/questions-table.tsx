import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { PeriodAnalyticsResponse } from "../../../api";
import { ratingLabels } from "./admin-stats-utils";

export function QuestionsTable({
  questions,
  periods,
}: {
  questions: PeriodAnalyticsResponse["questionStatistics"];
  periods: PeriodAnalyticsResponse[];
}) {
  return (
    <div className="mb-8 overflow-hidden border bg-card shadow-sm">
      <div className="border-b px-4 py-4">
        <h3 className="text-base font-bold text-foreground md:text-lg">
          Детализация
        </h3>
      </div>
      <div className="overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="text-center">№</TableHead>
              <TableHead className="text-left">Вопрос</TableHead>
              {periods.length === 1 ? (
                <>
                  <TableHead className="text-right">Удовл. потреб., %</TableHead>
                  <TableHead className="text-right">Средний балл</TableHead>
                  <TableHead className="text-right">Отклонение</TableHead>
                  <TableHead className="text-right">Оценка</TableHead>
                  <TableHead className="text-right">Ответов</TableHead>
                </>
              ) : (
                periods.map((period, index) => (
                  <TableHead
                    key={`${period.label}-${index}`}
                    className="text-right"
                  >
                    {period.label}
                  </TableHead>
                ))
              )}
            </TableRow>
          </TableHeader>
          <TableBody>
            {questions.map((question, index) => (
              <TableRow key={question.questionId}>
                <TableCell className="text-center">{index + 1}</TableCell>
                <TableCell>{question.questionText}</TableCell>
                {periods.length === 1 ? (
                  <>
                    <TableCell className="text-right">
                      {question.satisfactionPercentage.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-right">
                      {question.averageScore.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-right">
                      {question.standardDeviation.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-right">
                      {ratingLabels[question.rating]}
                    </TableCell>
                    <TableCell className="text-right">
                      {question.responseCount}
                    </TableCell>
                  </>
                ) : (
                  periods.map((period, periodIndex) => {
                    const metric = period.questionStatistics.find(
                      (q) => q.questionId === question.questionId,
                    );
                    return (
                      <TableCell
                        key={`${question.questionId}-${periodIndex}`}
                        className="text-right"
                      >
                        {metric?.satisfactionPercentage.toFixed(2) ?? "-"}
                      </TableCell>
                    );
                  })
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
