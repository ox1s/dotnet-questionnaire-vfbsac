import { useEffect, useRef, type DependencyList, type ReactNode } from "react";
import { useAdminPage } from "@/hooks/use-admin-page";

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
  // Always read the latest config from a ref so the effect below doesn't
  // need `config` in its dependency array (re-run timing is controlled by
  // the caller-supplied `deps`).
  const configRef = useRef(config);
  configRef.current = config;

  useEffect(() => {
    setConfig(configRef.current);

    return () => setConfig({});
    // Caller-supplied `deps` array is spread by design (public API lets
    // callers control re-run timing); eslint can't statically verify a
    // spread, so that specific check is disabled below.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [setConfig, ...deps]);
};
