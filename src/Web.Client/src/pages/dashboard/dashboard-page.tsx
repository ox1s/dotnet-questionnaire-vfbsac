import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api, { formsApi, type Form } from "../../api";
import { toast } from "sonner";
import { AdminDashboard } from "@/components/dashboard/admin-dashboard";
import { UserDashboard } from "@/components/dashboard/user-dashboard";
import { AdminLayout } from "@/components/admin/admin-shared";
import { isAdmin, logout } from "@/utils/auth";

export const DashboardPage = () => {
  const [forms, setForms] = useState<Form[]>([]);
  const navigate = useNavigate();
  const userIsAdmin = isAdmin();

  const loadForms = useCallback(async () => {
    const res = userIsAdmin
      ? await formsApi.getAll()
      : await api.get<Form[]>("/forms");
    setForms(res.data);
  }, [userIsAdmin]);

  useEffect(() => {
    // `setForms` runs after the awaited fetch resolves, not synchronously
    // during the effect/commit, so this isn't the synchronous-setState
    // pattern react-hooks/set-state-in-effect warns about.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadForms().catch(() => navigate("/login"));
  }, [loadForms, navigate]);

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
      toast.success(
        form.isActive ? "Анкета закрыта." : "Анкета открыта.",
      );
    } catch {
      toast.error("Не удалось изменить статус анкеты.");
    }
  };

  return userIsAdmin ? (
    <AdminLayout title="Дашборд" subtitle="Анкеты">
      <AdminDashboard
        forms={forms}
        deleteForm={deleteForm}
        toggleFormActive={toggleFormActive}
      />
    </AdminLayout>
  ) : (
    <UserDashboard forms={forms} logout={logout} />
  );
};
