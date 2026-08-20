import { useState, type ReactNode } from "react";
import { AdminPageContext, type AdminPageConfig } from "@/hooks/use-admin-page";

export const AdminPageProvider = ({
  children,
}: {
  children: ReactNode;
}) => {
  const [config, setConfig] = useState<AdminPageConfig>({});

  return (
    <AdminPageContext.Provider value={{ config, setConfig }}>
      {children}
    </AdminPageContext.Provider>
  );
};
