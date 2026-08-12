import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import type { PeriodAnalyticsResponse } from "../../../api";
import { ratingLabels } from "./admin-stats-utils";

export function AnalyticsSummaryCards({
  report,
}: {
  report: PeriodAnalyticsResponse[] | null;
}) {
  return report?.length === 1 ? (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
      <Card>
        <CardHeader>
          <CardTitle>Всего анкет</CardTitle>
        </CardHeader>
        <CardContent className="text-2xl">
          {String(report[0].submissionCount)}
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Удовлетворенность потребителей, %</CardTitle>
        </CardHeader>
        <CardContent className="text-2xl">
          {report[0].overall.hasData ? (
            <>
              {report[0].overall.meanPercentage.toFixed(2)}
              <span className="ml-2 text-sm font-normal text-muted-foreground">
                {ratingLabels[report[0].overall.rating]}
              </span>
            </>
          ) : (
            <span className="text-sm font-normal text-muted-foreground">
              Нет данных
            </span>
          )}
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Отклонение</CardTitle>
        </CardHeader>
        <CardContent className="text-2xl">
          {report[0].overall.hasData
            ? `± ${report[0].overall.averageStandardDeviation.toFixed(2)}`
            : (
              <span className="text-sm font-normal text-muted-foreground">
                Нет данных
              </span>
            )}
        </CardContent>
      </Card>
    </div>
  ) : (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-8">
      {report?.map((period, index) => (
        <Card key={`${period.label}-${index}`}>
          <CardHeader>
            <CardTitle>{period.label}</CardTitle>
          </CardHeader>
          <CardContent className="text-2xl">
            {period.overall.hasData
              ? `${period.overall.meanPercentage.toFixed(2)}% / ${period.submissionCount}`
              : `— / ${period.submissionCount}`}
          </CardContent>
          <CardFooter className="text-xs">
            {`${period.periodStart.slice(0, 10)} - ${period.periodEnd.slice(0, 10)}`}
          </CardFooter>
        </Card>
      ))}
    </div>
  );
}
