import { type Form } from "../../api";
import { DashboardContent } from "./dashboard-content";

export const AdminDashboard = ({
  forms,
  deleteForm,
}: {
  forms: Form[];
  deleteForm: (id: string) => void;
}) => {
  return <DashboardContent forms={forms} deleteForm={deleteForm} isAdmin />;
};

