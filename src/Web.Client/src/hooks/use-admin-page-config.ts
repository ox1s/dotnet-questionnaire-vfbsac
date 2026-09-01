import {
  useLayoutEffect,
  useRef,
  type DependencyList,
  type ReactNode,
} from "react";
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

  // Layout effect, not a passive one: the header is rendered from this config,
  // so committing it after paint shows one frame of an empty (on mount) or
  // previous-page (on navigation) breadcrumb before it corrects itself.
  // React still flushes every cleanup before any setup within a commit, so
  // the outgoing page's reset can't clobber the incoming page's config.
  useLayoutEffect(() => {
    setConfig(configRef.current);

    return () => setConfig({});
    // Caller-supplied `deps` array is spread by design (public API lets
    // callers control re-run timing); eslint can't statically verify a
    // spread, so that specific check is disabled below.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [setConfig, ...deps]);
};
