import { useMemo, useState, type ReactNode } from "react";
import { AdminPageContext, type AdminPageConfig } from "@/hooks/use-admin-page";

export const AdminPageProvider = ({ children }: { children: ReactNode }) => {
  const [config, setConfig] = useState<AdminPageConfig>({});

  // The provider is long-lived (it sits on the AppShell layout route), and
  // every navigation pushes a new config through it. Memoizing keeps that from
  // re-rendering the sidebar and the whole page subtree twice over.
  // `setConfig` is a useState setter, so it is already stable.
  const value = useMemo(() => ({ config, setConfig }), [config]);

  return (
    <AdminPageContext.Provider value={value}>
      {children}
    </AdminPageContext.Provider>
  );
};
