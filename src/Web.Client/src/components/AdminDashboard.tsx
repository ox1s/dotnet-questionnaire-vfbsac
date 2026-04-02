import { type Form } from "../api";
import { DashboardContent } from "./DashboardContent";

export const AdminDashboard = ({
  forms,
  deleteForm,
}: {
  forms: Form[];
  deleteForm: (id: string) => void;
}) => {
  return <DashboardContent forms={forms} deleteForm={deleteForm} isAdmin />;
};
