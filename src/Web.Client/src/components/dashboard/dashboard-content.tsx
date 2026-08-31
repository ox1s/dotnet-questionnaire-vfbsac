import { Link } from "react-router-dom";
import { type Form } from "../../api";
import { ROLE_LABELS } from "@/utils/roles";
import { FILTER_FIELD_LABELS } from "@/utils/filter-fields";
import {
  FileText,
  BarChart3,
  ArrowRight,
  Trash2,
  Power,
  PowerOff,
  Eye,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";

export const DashboardContent = ({
  forms,
  deleteForm,
  toggleFormActive,
  isAdmin,
}: {
  forms: Form[];
  deleteForm?: (id: string) => void;
  toggleFormActive?: (form: Form) => void;
  isAdmin: boolean;
}) => {
  return (
    <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
      {forms.map((form) => (
        <Card
          key={form.id}
          className={`group flex flex-col hover:shadow-md transition-all duration-200 ${
            isAdmin && !form.isActive ? "opacity-60" : ""
          }`}
        >
          <CardHeader className="flex flex-row items-start justify-between pb-2 space-y-0">
            <div className="p-3 bg-primary/15 text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
              <FileText size={24} />
            </div>
            {isAdmin && (
              <div className="flex items-center gap-1">
                <Button
                  variant="ghost"
                  size="icon"
                  title={form.isActive ? "Закрыть анкету" : "Открыть анкету"}
                  className={
                    form.isActive
                      ? "text-muted-foreground hover:text-destructive"
                      : "text-muted-foreground hover:text-primary"
                  }
                  onClick={() => toggleFormActive?.(form)}
                >
                  {form.isActive ? (
                    <Power size={18} />
                  ) : (
                    <PowerOff size={18} />
                  )}
                </Button>
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="text-muted-foreground hover:text-destructive"
                    >
                      <Trash2 size={18} />
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>Удалить анкету?</AlertDialogTitle>
                      <AlertDialogDescription>
                        Анкета "{form.title}" будет удалена без возможности
                        восстановления.
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Отмена</AlertDialogCancel>
                      <AlertDialogAction
                        className="bg-destructive text-destructive-foreground"
                        onClick={() => deleteForm?.(form.id)}
                      >
                        Удалить
                      </AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
                <Button
                  variant="ghost"
                  size="icon"
                  asChild
                  className="text-muted-foreground hover:text-foreground"
                >
                  <Link to={`/admin/preview/${form.id}`} title="Просмотреть анкету">
                    <Eye size={18} />
                  </Link>
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  asChild
                  className="text-muted-foreground hover:text-foreground"
                >
                  <Link to={`/admin/stats/${form.id}`}>
                    <BarChart3 size={18} />
                  </Link>
                </Button>
              </div>
            )}
          </CardHeader>
          <CardContent>
            <CardTitle className="text-base leading-snug">
              {form.title}
            </CardTitle>
            {isAdmin && !form.isActive && (
              <Badge variant="destructive" className="mt-2">
                Неактивна
              </Badge>
            )}
            {isAdmin && form.targetRole && (
              <Badge variant="secondary" className="mt-2">
                Аудитория: {ROLE_LABELS[form.targetRole] ?? form.targetRole}
              </Badge>
            )}
          </CardContent>
          <CardFooter className="mt-auto pt-4 border-t flex-col items-start gap-4">
            <div className="flex flex-wrap gap-2">
              {form.requiredFilters && form.requiredFilters.length > 0 ? (
                form.requiredFilters.map((filter, i) => (
                  <Badge key={i} variant="secondary">
                    {FILTER_FIELD_LABELS[filter] ?? filter}
                  </Badge>
                ))
              ) : (
                <Badge variant="outline">Без фильтров</Badge>
              )}
            </div>
            {!isAdmin && (
              <Button asChild className="w-full" variant="outline">
                <Link to={`/form/${form.id}`}>
                  Пройти опрос <ArrowRight size={16} className="ml-2" />
                </Link>
              </Button>
            )}
          </CardFooter>
        </Card>
      ))}
      {forms.length === 0 && (
        <div className="col-span-full border-2 border-dashed border-border p-12 text-center">
          <p className="text-muted-foreground font-medium">
            Нет доступных анкет
          </p>
        </div>
      )}
    </div>
  );
};
