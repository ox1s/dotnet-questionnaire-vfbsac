import { type Form } from "../../api";
import { LogOut, FileText, User, Building2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ModeToggle } from "@/components/shared/mode-toggle";
import { DashboardContent } from "./dashboard-content";
import { getUserInfo } from "@/utils/auth";
import { OWN_ROLE_LABELS } from "@/utils/roles";

export const UserDashboard = ({
  forms,
  logout,
}: {
  forms: Form[];
  logout: () => void;
}) => {
  const role = getUserInfo()?.role;
  const roleLabel = (role && OWN_ROLE_LABELS[role]) ?? "Пользователь";
  const RoleIcon = role === "Employer" ? Building2 : User;

  return (
    <div className="min-h-screen bg-muted/30 font-sans text-foreground">
      <nav className="bg-background border-b border-border px-6 py-4">
        <div className="max-w-5xl mx-auto flex justify-between items-center">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 bg-primary flex items-center justify-center text-primary-foreground">
              <FileText size={18} />
            </div>
            <h1 className="text-lg font-bold">Опросы</h1>
          </div>
          <div className="flex items-center gap-4">
            <Badge
              variant="secondary"
              className="hidden sm:flex items-center gap-1"
            >
              <RoleIcon size={14} /> {roleLabel}
            </Badge>
            <ModeToggle />
            <Button
              variant="ghost"
              onClick={logout}
              aria-label="Выйти"
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
