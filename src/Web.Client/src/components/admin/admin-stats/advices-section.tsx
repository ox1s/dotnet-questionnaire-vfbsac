import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import type { AdviceItem } from "../../../api";

export function AdvicesSection({
  advices,
  teacherFilterId,
  departmentFilterId,
  getTeacherName,
  getDepartmentName,
}: {
  advices: AdviceItem[];
  teacherFilterId?: string;
  departmentFilterId?: string;
  getTeacherName: (teacherId?: string) => string | undefined;
  getDepartmentName: (departmentId?: string) => string | undefined;
}) {
  const showTeacher = !departmentFilterId || Boolean(teacherFilterId);
  const showDepartment = !teacherFilterId || Boolean(departmentFilterId);

  return (
    <div className="mb-8 overflow-hidden border bg-card shadow-sm">
      <div className="border-b px-4 py-4">
        <h3 className="text-base font-bold text-foreground md:text-lg">
          Рекомендации
        </h3>
      </div>

      {advices.length > 0 ? (
        <div className="space-y-4 p-4">
          {advices.map((advice, index) => {
            const teacherName = getTeacherName(advice.teacherId);
            const departmentName = getDepartmentName(advice.departmentId);

            return (
              <Card key={`${advice.text}-${index}`} className="gap-3 py-5">
                <CardContent className="space-y-3">
                  <div className="flex flex-wrap gap-2">
                    {showTeacher && teacherName ? (
                      <Badge variant="secondary">{teacherName}</Badge>
                    ) : null}
                    {showDepartment && departmentName ? (
                      <Badge variant="outline">{departmentName}</Badge>
                    ) : null}
                  </div>
                  <p className="text-sm leading-6 text-foreground whitespace-pre-wrap">
                    {advice.text}
                  </p>
                </CardContent>
              </Card>
            );
          })}
        </div>
      ) : (
        <div className="p-10 text-center text-muted-foreground">
          Текстовые ответы не найдены.
        </div>
      )}
    </div>
  );
}
