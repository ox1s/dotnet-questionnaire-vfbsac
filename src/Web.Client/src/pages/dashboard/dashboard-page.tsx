import { useEffect, useState } from "react";
import api, { formsApi, type Form } from "../../api";
import { toast } from "sonner";
import { AdminDashboard } from "@/components/dashboard/admin-dashboard";
import { UserDashboard } from "@/components/dashboard/user-dashboard";
import { useAdminPageConfig } from "@/hooks/use-admin-page-config";
import { isAdmin, logout } from "@/utils/auth";

// Admins see this page inside the shared <AppShell/> sidebar layout and get
// the management view; everyone else gets the standalone user dashboard, which
// brings its own chrome. Two components rather than one branchy component, so
// each holds only the hooks its own variant needs.
export const DashboardPage = () =>
  isAdmin() ? <AdminDashboardPage /> : <UserDashboardPage />;

// Takes a plain discriminator rather than a loader function: as an effect
// dependency, a string can't accidentally be passed as a fresh reference on
// every render (which would turn this into a request loop).
const useForms = (source: "admin" | "user") => {
  const [forms, setForms] = useState<Form[]>([]);

  useEffect(() => {
    let cancelled = false;
    const request =
      source === "admin"
        ? formsApi.getAll()
        : api.get<Form[]>("/forms");

    request
      .then((res) => {
        if (!cancelled) setForms(res.data);
      })
      // A 401 here is already handled globally (api.ts's interceptor calls
      // logout(), which navigates to /login) — this catch is only for
      // non-auth failures, so it must not also force a redirect. The cancelled
      // check keeps a slow failure from toasting over an unrelated screen.
      .catch(() => {
        if (!cancelled) toast.error("Не удалось загрузить анкеты.");
      });

    return () => {
      cancelled = true;
    };
  }, [source]);

  return [forms, setForms] as const;
};

const AdminDashboardPage = () => {
  const [forms, setForms] = useForms("admin");

  useAdminPageConfig({ title: "Дашборд", subtitle: "Анкеты" });

  const deleteForm = async (id: string) => {
    try {
      await api.delete(`/forms/${id}`);
      setForms((prev) => prev.filter((f) => f.id !== id));
      toast.success("Анкета успешно удалена.");
    } catch {
      toast.error("Не удалось удалить анкету.");
    }
  };

  const toggleFormActive = async (form: Form) => {
    try {
      if (form.isActive) {
        await formsApi.deactivate(form.id);
      } else {
        await formsApi.activate(form.id);
      }
      setForms((prev) =>
        prev.map((f) =>
          f.id === form.id ? { ...f, isActive: !f.isActive } : f,
        ),
      );
      toast.success(form.isActive ? "Анкета закрыта." : "Анкета открыта.");
    } catch {
      toast.error("Не удалось изменить статус анкеты.");
    }
  };

  return (
    <AdminDashboard
      forms={forms}
      deleteForm={deleteForm}
      toggleFormActive={toggleFormActive}
    />
  );
};

const UserDashboardPage = () => {
  const [forms] = useForms("user");

  return <UserDashboard forms={forms} logout={logout} />;
};
