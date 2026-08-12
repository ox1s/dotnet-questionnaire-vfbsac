import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api, { formsApi, type Form } from "../../api";
import { toast } from "sonner";
import { AdminDashboard } from "@/components/dashboard/admin-dashboard";
import { UserDashboard } from "@/components/dashboard/user-dashboard";
import { AdminLayout } from "@/components/admin/admin-shared";
import { isAdmin } from "@/utils/auth";

export const DashboardPage = () => {
  const [forms, setForms] = useState<Form[]>([]);
  const navigate = useNavigate();
  const userIsAdmin = isAdmin();

  useEffect(() => {
    loadForms().catch(() => navigate("/login"));
  }, []);

  const loadForms = async () => {
    const res = userIsAdmin
      ? await formsApi.getAll()
      : await api.get<Form[]>("/forms");
    setForms(res.data);
  };

  const logout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

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
