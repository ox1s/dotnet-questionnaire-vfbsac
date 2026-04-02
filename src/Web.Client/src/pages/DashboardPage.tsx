import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api, { type Form } from "../api";
import {
  LogOut,
  FileText,
  BarChart3,
  User,
  ArrowRight,
  Trash2,
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
import { isAdmin } from "../utils/auth";
import { AdminLayout } from "@/components/AdminShared";
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
import { toast } from "sonner";

export const DashboardPage = () => {
  const [forms, setForms] = useState<Form[]>([]);
  const navigate = useNavigate();
  const userIsAdmin = isAdmin();

  useEffect(() => {
    loadForms().catch(() => navigate("/login"));
  }, [navigate]);

  const loadForms = async () => {
    const res = await api.get<Form[]>("/forms");
    setForms(res.data);
  };

  const logout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  const deleteForm = async (id: string) => {
    try {
      await api.delete(`/forms/${id}`);
      setForms((prev) => prev.filter((form) => form.id !== id));
      toast.success("Анкета успешно удалена.");
    } catch {
      toast.error("Не удалось удалить анкету.");
    }
  };

  const Content = () => (
    <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
      {forms.map((form) => (
        <Card
          key={form.id}
          className="group flex flex-col hover:shadow-md transition-all duration-200"
        >
          <CardHeader className="flex flex-row items-start justify-between pb-2 space-y-0">
            <div className="p-3 bg-primary/15 text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
              <FileText size={24} />
            </div>
            {userIsAdmin && (
              <div className="flex items-center gap-1">
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
                        onClick={() => deleteForm(form.id)}
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
          </CardContent>
          <CardFooter className="mt-auto pt-4 border-t flex-col items-start gap-4">
            <div className="flex flex-wrap gap-2">
              {form.requiredFilters && form.requiredFilters.length > 0 ? (
                form.requiredFilters.map((filter, i) => (
                  <Badge key={i} variant="secondary">
                    {filter}
                  </Badge>
                ))
              ) : (
                <Badge variant="outline">Без фильтров</Badge>
              )}
            </div>
            {!userIsAdmin && (
              <Button asChild className="w-full" variant="secondary">
                <Link to={`/form/${form.id}`}>
                  Пройти опрос <ArrowRight size={16} className="ml-2" />
                </Link>
              </Button>
            )}
          </CardFooter>
        </Card>
      ))}
      {forms.length === 0 && (
        <div className="col-span-full p-12 text-center border-2 border-dashed border-border rounded-xl">
          <p className="text-muted-foreground font-medium">
            Нет доступных анкет
          </p>
        </div>
      )}
    </div>
  );

  if (userIsAdmin) {
    return (
      <AdminLayout
        title="Анкеты"
        subtitle="Обзор всех активных опросов и анкет."
      >
        <Content />
      </AdminLayout>
    );
  }

  return (
    <div className="min-h-screen bg-muted/30 font-sans text-foreground">
      <nav className="bg-background border-b border-border px-6 py-4">
        <div className="max-w-5xl mx-auto flex justify-between items-center">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 bg-primary flex items-center justify-center text-primary-foreground">
              <FileText size={18} />
            </div>
            <h1 className="text-lg font-bold">Опросы Студентов</h1>
          </div>
          <div className="flex items-center gap-4">
            <Badge
              variant="secondary"
              className="hidden sm:flex items-center gap-1"
            >
              <User size={14} /> Студент
            </Badge>
            <Button
              variant="ghost"
              onClick={logout}
              className="text-muted-foreground hover:text-destructive"
            >
              <LogOut size={16} className="mr-2" />
              <span className="hidden sm:inline">Выйти</span>
            </Button>
          </div>
        </div>
      </nav>
      <main className="max-w-5xl mx-auto px-6 py-10">
        <div className="mb-8">
          <h2 className="text-2xl font-bold">Доступные анкеты</h2>
          <p className="text-muted-foreground">
            Выберите анкету из списка ниже, чтобы начать.
          </p>
        </div>
        <Content />
      </main>
    </div>
  );
};
