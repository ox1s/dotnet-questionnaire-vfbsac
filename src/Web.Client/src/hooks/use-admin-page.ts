import { createContext, useContext, type ReactNode } from "react";

export type AdminPageConfig = {
  title?: string;
  subtitle?: string;
  actions?: ReactNode;
};

export type AdminPageContextType = {
  config: AdminPageConfig;
  setConfig: (config: AdminPageConfig) => void;
};

export const AdminPageContext = createContext<AdminPageContextType | null>(
  null,
);

export const useAdminPage = () => {
  const ctx = useContext(AdminPageContext);
  if (!ctx) {
    throw new Error("useAdminPage must be used inside AdminPageProvider");
  }
  return ctx;
};
