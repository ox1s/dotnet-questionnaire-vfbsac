import { type Form } from "../../api";
import { DashboardContent } from "./dashboard-content";

export const AdminDashboard = ({
  forms,
  deleteForm,
  toggleFormActive,
}: {
  forms: Form[];
  deleteForm: (id: string) => void;
  toggleFormActive: (form: Form) => void;
}) => {
  return (
    <DashboardContent
      forms={forms}
      deleteForm={deleteForm}
      toggleFormActive={toggleFormActive}
      isAdmin
    />
  );
};

