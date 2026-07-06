import { useEffect, type DependencyList, type ReactNode } from "react";
import { useAdminPage } from "@/contexts/admin-page-context";

type AdminPageConfig = {
  title?: string;
  subtitle?: string;
  actions?: ReactNode;
};

export const useAdminPageConfig = (
  config: AdminPageConfig,
  deps: DependencyList = [],
) => {
  const { setConfig } = useAdminPage();

  useEffect(() => {
    setConfig(config);

    return () => setConfig({});
  }, [setConfig, ...deps]);
};
