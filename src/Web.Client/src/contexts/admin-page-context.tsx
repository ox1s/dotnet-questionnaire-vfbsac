import React, { createContext, useContext, useState } from "react";

type AdminPageConfig = {
  title?: string;
  subtitle?: string;
  actions?: React.ReactNode;
};

type AdminPageContextType = {
  config: AdminPageConfig;
  setConfig: (config: AdminPageConfig) => void;
};

const AdminPageContext = createContext<AdminPageContextType | null>(null);

export const useAdminPage = () => {
  const ctx = useContext(AdminPageContext);
  if (!ctx) {
    throw new Error("useAdminPage must be used inside AdminPageProvider");
  }
  return ctx;
};

export const AdminPageProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [config, setConfig] = useState<AdminPageConfig>({});

  return (
    <AdminPageContext.Provider value={{ config, setConfig }}>
      {children}
    </AdminPageContext.Provider>
  );
};
