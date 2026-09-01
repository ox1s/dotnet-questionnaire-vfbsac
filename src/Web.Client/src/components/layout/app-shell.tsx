import { Outlet } from "react-router-dom";
import { AdminLayout } from "@/components/admin/admin-shared";
import { AdminPageProvider } from "@/contexts/admin-page-context";
import { isAdmin } from "@/utils/auth";

// The single layout route for every screen that shows the sidebar. Pages must
// not render their own <AdminLayout/>: React Router keeps a layout route's
// element mounted while you navigate between its children, so hoisting the
// shell here is what stops the sidebar from being torn down and rebuilt on
// every navigation (and losing its expanded groups with it).
//
// /dashboard is the one shell route a non-admin can reach, and every sidebar
// link points at an admin screen, so regular users get the page without the
// chrome. The provider sits above that branch on purpose: it means any page
// under this route can call `useAdminPageConfig` safely, whoever is looking.
export const AppShell = () => (
  <AdminPageProvider>{isAdmin() ? <AdminLayout /> : <Outlet />}</AdminPageProvider>
);
