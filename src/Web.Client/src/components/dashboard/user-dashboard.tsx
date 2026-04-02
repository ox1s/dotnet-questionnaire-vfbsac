import { type Form } from "../../api";
import { LogOut, FileText, User } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { DashboardContent } from "./dashboard-content";

export const UserDashboard = ({
  forms,
  logout,
}: {
  forms: Form[];
  logout: () => void;
}) => {
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

        <DashboardContent forms={forms} isAdmin={false} />
      </main>
    </div>
  );
};

