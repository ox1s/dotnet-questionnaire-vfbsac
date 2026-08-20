import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts";
import {
  ChartTooltip,
  ChartTooltipContent,
  ChartContainer,
  type ChartConfig,
} from "@/components/ui/chart";
import type { PeriodAnalyticsResponse } from "../../../api";

const colors = [
  "var(--chart-5)",
  "var(--chart-4)",
  "var(--chart-3)",
  "var(--chart-2)",
  "var(--chart-1)",
];

const chartConfig = {
  desktop: {
    label: "Desktop",
    color: "#2563eb",
  },
  mobile: {
    label: "Mobile",
    color: "#60a5fa",
  },
} satisfies ChartConfig;

export function AnalyticsChart({
  report,
  questions,
}: {
  report: PeriodAnalyticsResponse[];
  questions: PeriodAnalyticsResponse["questionStatistics"];
}) {
  const chartData = questions.map((question, index) => {
    const row: Record<string, string | number> = {
      name: `В${index + 1}`,
      fullName: question.questionText,
    };
    report.forEach((period, periodIndex) => {
      const metric = period.questionStatistics.find(
        (q) => q.questionId === question.questionId,
      );
      row[`slice_${periodIndex}`] = metric?.satisfactionPercentage ?? 0;
    });
    return row;
  });

  if (chartData.length === 0) return null;

  return (
    <div>
      <ChartContainer config={chartConfig} className="h-50 w-full">
        <BarChart
          data={chartData}
          margin={{ top: 10, right: 10, left: -25, bottom: 0 }}
        >
          <CartesianGrid
            strokeDasharray="3 3"
            vertical={false}
            stroke="var(--border)"
          />
          <XAxis
            dataKey="name"
            axisLine={false}
            tickLine={false}
            tick={{ fill: "var(--muted-foreground)", fontSize: 10 }}
            dy={10}
          />
          <YAxis
            axisLine={false}
            tickLine={false}
            tick={{ fill: "var(--muted-foreground)", fontSize: 10 }}
            domain={[0, 100]}
            ticks={[0, 20, 40, 60, 80, 100]}
          />
          <ChartTooltip
            cursor={true}
            content={
              <ChartTooltipContent
                labelKey="fullName"
                formatter={(value, name) => [
                  <b>{name}</b>,
                  " ",
                  Number(value ?? 0).toFixed(2),
                ]}
              />
            }
          />
          {report.map((period, index) => (
            <Bar
              key={`${period.label}-${index}`}
              dataKey={`slice_${index}`}
              name={period.label}
              fill={colors[index % colors.length]}
              radius={[4, 4, 0, 0]}
              maxBarSize={40}
            />
          ))}
        </BarChart>
      </ChartContainer>
    </div>
  );
}
