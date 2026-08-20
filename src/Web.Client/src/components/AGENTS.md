<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# components

## Purpose
All reusable React components, split into seven feature/role folders. `ui/` is the shadcn/Radix design-system layer (generic, no business logic); `admin/`, `auth/`, `dashboard/`, `layout/`, `shared/`, and `survey/` build on top of `ui/` with app-specific behavior — admin CRUD tables/modals, route protection, the two dashboard variants (admin vs. student), the admin sidebar chrome, small cross-page widgets, and the survey-taking form controls.

## Key Files
| File | Description |
|------|-------------|
| **admin/** | |
| `admin/admin-shared.tsx` | The admin "kit": `AdminLayout`/`AdminLayoutContent` (sidebar + breadcrumb header shell, reads title/subtitle/actions from `AdminPageProvider`/`useAdminPage` unless overridden by props), `AdminTable<T>` (generic searchable table with empty state), `AdminTableRow`, `AdminTableIconCell` (icon-or-initial + title + "Удалено" badge), `AdminTableActions` (edit/delete-with-confirm/restore icon buttons), `AdminTableTextBadge`, and `AdminModal` (simple centered form-modal, not Radix Dialog — hand-rolled overlay + `<Card>`). Every `pages/admin/*` page is built from these primitives |
| **auth/** | |
| `auth/protected-route.tsx` | `ProtectedRoute({ allowedRoles? })` — layout route rendering `<Outlet />`; redirects to `/login` if `getUserInfo()` is null, or to `/dashboard` if the user's role isn't in `allowedRoles` |
| **dashboard/** | |
| `dashboard/dashboard-content.tsx` | `DashboardContent({ forms, deleteForm?, isAdmin })` — the shared card grid of forms; admin cards get delete (with `AlertDialog` confirm) + stats-link buttons, student cards get a "Пройти опрос" (take survey) button linking to `/form/:id` |
| `dashboard/admin-dashboard.tsx` | Thin wrapper: `<DashboardContent isAdmin />` |
| `dashboard/user-dashboard.tsx` | Full standalone page-like shell (own nav bar with logout + `ModeToggle`) wrapping `<DashboardContent isAdmin={false} />` — used for non-admin users since they don't get the `AdminLayout` sidebar |
| **layout/** | |
| `layout/app-sidebar.tsx` | `AppSidebar` — the admin nav: hardcoded `data.navMain` tree (Дашборд / Справочники / Настройки sections with hrefs to the `/admin/*` routes), logo linking to `/dashboard`, footer with `ModeToggle` + logout button (`window.location.href = "/login"`, not React Router) |
| `layout/nav-main.tsx` | `NavMain` — renders the collapsible sidebar sections from `AppSidebar`'s `data.navMain`; highlights the active route via `useLocation()` |
| `layout/nav-projects.tsx` | `NavProjects` — generic shadcn-template "Projects" sidebar group; **not wired into `AppSidebar`'s actual nav data, effectively unused boilerplate from the shadcn dashboard template** |
| `layout/team-switcher.tsx` | `TeamSwitcher` — generic shadcn-template team/org switcher dropdown; **also not referenced by `AppSidebar`, unused boilerplate** |
| `layout/theme-provider.tsx` | `ThemeProvider` — light/dark/system theme context, persists to `localStorage["vite-ui-theme"]`, toggles the `light`/`dark` class on `<html>`. Wraps the whole app in `App.tsx`. The paired `useTheme()` hook lives in `hooks/use-theme.ts` (moved out to keep this file component-only for Vite fast-refresh) |
| **shared/** | |
| `shared/filter-select.tsx` | `FilterSelect` — a `<Select>` wrapper that maps an internal `""` (all/no filter) sentinel to `"all"` for Radix (which can't have empty-string item values); used for the report filter dropdowns in `AdminStatsPage` |
| `shared/mode-toggle.tsx` | `ModeToggle` — sun/moon icon button that flips `useTheme()` between light and dark (resolves `"system"` via `matchMedia` first) |
| **survey/** | |
| `survey/context-selector.tsx` | `ContextSelector({ requiredFilters, onChange })` — renders the "education form / department / discipline / teacher" pickers a student fills in before/while taking a survey, conditionally shown per `form.requiredFilters`; loads dictionaries on demand and cross-filters department↔discipline via `getLinkedFilterOptions` |
| `survey/weighted-rating-input.tsx` | `WeightedRatingInput({ value, weight, onChange })` — the "importance / actual score" (both 1–10) paired number inputs used for `WeightedRating`-type questions, with inline validation (range checks + "score can't exceed weight") |
| **ui/** | shadcn-generated Radix-based primitives (see below) — `alert-dialog.tsx`, `alert.tsx`, `avatar.tsx`, `badge.tsx`, `breadcrumb.tsx`, `button.tsx`, `calendar.tsx`, `card.tsx`, `chart.tsx`, `collapsible.tsx`, `dropdown-menu.tsx`, `field.tsx`, `input-group.tsx`, `input.tsx`, `label.tsx`, `popover.tsx`, `select.tsx`, `separator.tsx`, `sheet.tsx`, `sidebar.tsx`, `skeleton.tsx`, `sonner.tsx`, `table.tsx`, `textarea.tsx`, `tooltip.tsx` |

### Notable `ui/` internals
- `button.tsx` — `cva`-driven variants (`default|outline|secondary|ghost|destructive|link`) × sizes (`default|xs|sm|lg|icon|icon-xs|icon-sm|icon-lg`); square corners (`rounded-none`) is the deliberate house style (`style: "radix-lyra"` in `components.json`), not a bug — don't "fix" it by adding rounded corners.
- `chart.tsx` — thin Recharts wrapper (`ChartContainer`, `ChartTooltip`, `ChartTooltipContent`, `type ChartConfig`) that maps a config object to CSS custom properties for series colors; used only by `AdminStatsPage`'s `BarChart`.
- `sidebar.tsx` — the full shadcn collapsible-sidebar system (`SidebarProvider`, `Sidebar`, `SidebarInset`, `SidebarTrigger`, `SidebarMenu*`, mobile sheet fallback via `use-mobile.ts`) that `AdminLayout` and `AppSidebar` are built on. The paired `useSidebar()` hook lives in `hooks/use-sidebar.ts` (moved out to keep this file component-only for Vite fast-refresh).
- `sonner.tsx` — theatrically thin wrapper binding the `sonner` `Toaster` to `next-themes`'s theme (note: the app's own `ThemeProvider` in `layout/theme-provider.tsx` is what's actually mounted in `App.tsx`, not `next-themes`'s provider — the two theme systems coexist but only the custom one drives `<html>`'s class).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `admin/` | Admin CRUD-page building blocks (layout shell, table, modal) |
| `auth/` | Route guarding |
| `dashboard/` | Form-list dashboard, admin and student variants |
| `layout/` | App chrome: admin sidebar, nav, theme provider |
| `shared/` | Small cross-cutting widgets (filter select, theme toggle) |
| `survey/` | Survey-taking form controls |
| `ui/` | shadcn/Radix design-system primitives |

## For AI Agents
### Working In This Directory
- **Component pattern**: functional components, typed via inline `interface Props` or `React.ComponentProps<typeof X>`; most are arrow-function `const X = (...) => {...}` exports, `ui/` ones are `function X(...) {...}` per shadcn convention. No class components.
- **Styling**: Tailwind utility classes inline; use `cn()` from `@/lib/utils` when a class list needs conditional merging (variant/state classes) rather than string concatenation. Square/flat corners (`rounded-none`) and border-heavy cards are the house look — match it in new components.
- **Icons**: `lucide-react` exclusively, sized via the `size={n}` prop (not Tailwind `size-*` classes) in feature components, though `ui/button.tsx` targets `svg` sizing via `[&_svg:not([class*='size-'])]:size-4`.
- **Adding a new admin CRUD page**: reuse `AdminTable` + `AdminModal` + `AdminTableActions` from `admin/admin-shared.tsx`, plus the shared `useDictionaryCrud` hook from `hooks/use-dictionary-crud.ts` for list loading, search filtering, and modal/save/delete/restore state (see any `pages/admin/admin-*-page.tsx` for the pattern) instead of hand-rolling a table or re-implementing that state.
- **Before adding sidebar nav items**: `layout/nav-projects.tsx` and `layout/team-switcher.tsx` are unused shadcn template leftovers — don't assume they're wired up; the real nav data lives inline in `layout/app-sidebar.tsx`'s `data.navMain`.
- **Adding shadcn primitives**: this project uses the `shadcn` CLI (`components.json`, style `radix-lyra`, base color `mist`) — new `ui/` components should be added via the CLI rather than hand-written to keep variant conventions (`data-slot`, `cva`) consistent with the existing set.

## Dependencies
### Internal
- `@/api` (DTOs), `@/utils/linked-filters`, `@/utils/auth`, `@/contexts/admin-page-context`, `@/hooks/use-admin-page-config`, `@/lib/utils` (`cn`)
- Nearly every non-`ui/` component imports from `components/ui/*`

### External
- radix-ui (primitives underlying every `ui/` component), lucide-react (icons), class-variance-authority (`cva` variants), react-router-dom (`Link`, `Outlet`, `useLocation`), recharts (via `ui/chart.tsx`), sonner (toasts), react-day-picker (via `ui/calendar.tsx`), date-fns

<!-- MANUAL: -->
