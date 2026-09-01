<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# src

## Purpose
This is the application source root for the Web.Client SPA. It contains the app bootstrap (`main.tsx`, `App.tsx`), the single HTTP client and API surface (`api.ts`), cross-cutting primitives (`contexts/`, `hooks/`, `lib/`, `utils/`), and the two larger trees documented separately: `components/` (reusable UI, one level per feature area) and `pages/` (route-level screens). There is currently no `src/assets/` or `src/api/` directory — API types/calls live in the single top-level `src/api.ts` file instead of an `api/` folder.

## Key Files
| File | Description |
|------|-------------|
| `main.tsx` | React 19 entry point; mounts `<App />` into `#root` under `<StrictMode>` |
| `App.tsx` | Top-level router: `BrowserRouter` → `ThemeProvider` (default light) → `TooltipProvider` → `AuthSessionListener` (`components/auth/auth-session-listener.tsx`; navigates to `/login` on the `auth:logout` window event dispatched by `logout()`) → `Toaster` (sonner, top-center) → `<Routes>`. Defines every route in the app (see table below), plus a trailing `path="*"` catch-all redirecting to `/dashboard` |
| `App.css` | Legacy Vite starter styles (mostly unused now that Tailwind + `index.css` drive styling) |
| `index.css` | Tailwind v4 entrypoint: `@import "tailwindcss"`, shadcn theme (`shadcn/tailwind.css`), `tw-animate-css`, Geist Variable font; defines the OKLCH color tokens (`--background`, `--primary`, `--chart-1..5`, `--sidebar-*`, etc.) consumed via `@theme inline` and used by every `ui/` component and `dark:` variants |
| `api.ts` | The entire backend API surface: axios instance (`baseURL: import.meta.env.VITE_API_URL \|\| "/api"`, Bearer-token request interceptor, 401→`logout()` response interceptor via the exported `handleUnauthorizedResponse`, skipped for the `/users/login` request itself), every DTO interface (`Form`, `FormDetail`, `Question`, `DictionaryItem`, `TeacherItem`, `SubmissionListItem`, `StatisticsFilters`, analytics/report request-response types), grouped endpoint objects (`usersApi`, `settingsApi`, `dictionariesApi`, `submissionsApi`, `reportsApi`), and `getApiErrorMessage()` for turning ASP.NET `ProblemDetails` errors into user-facing Russian strings |

### Routes defined in `App.tsx`
| Path | Element | Guard |
|------|---------|-------|
| `/login` | `LoginPage` | none |
| `/` | redirect → `/dashboard` | none |
| `/dashboard` | `DashboardPage` | `ProtectedRoute` (any authenticated user) + `AppShell` |
| `/form/:id` | `SurveyPage` | `ProtectedRoute` (standalone, no sidebar) |
| `/admin/stats/:id` | `AdminStatsPage` | Admin + `AppShell` |
| `/admin/preview/:id` | `AdminFormPreviewPage` | Admin + `AppShell` |
| `/admin/create-form` | `CreateFormPage` | Admin + `AppShell` |
| `/admin/teachers` | `AdminTeachersPage` | Admin + `AppShell` |
| `/admin/disciplines` | `AdminDisciplinesPage` | Admin + `AppShell` |
| `/admin/departments` | `AdminDepartmentsPage` | Admin + `AppShell` |
| `/admin/specialities` | `AdminSpecialitiesPage` | Admin + `AppShell` |
| `/admin/specializations` | `AdminSpecializationsPage` | Admin + `AppShell` |
| `/admin/groups` | `AdminGroupsPage` | Admin + `AppShell` |
| `/admin/employers` | `AdminEmployersPage` | Admin + `AppShell` |
| `/admin/settings` | `AdminSettingsPage` | Admin + `AppShell` |

"Admin" above is `ProtectedRoute allowedRoles={["Admin"]}`, nested inside the
`AppShell` layout route. `AppShell` (`components/layout/app-shell.tsx`) is the
**only** place that mounts `AdminLayout`: React Router keeps a layout route's
element mounted while you navigate between its children, so this is what stops
the sidebar from being rebuilt (and losing its expanded groups) on every
navigation. A page must never wrap itself in its own `AdminLayout` —
`components/layout/app-shell.test.tsx` fails the build if one does. For a
non-admin, `AppShell` renders the page directly, since every sidebar link
points at an admin screen — but the `AdminPageProvider` sits *above* that
branch, so `useAdminPageConfig` is safe to call on any page under the shell
regardless of role.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `contexts/` | `admin-page-context.tsx` — React context (`AdminPageProvider`) holding a mutable `{title, subtitle, actions}` config object that `AdminLayout`'s header reads, so any admin page can push its own breadcrumb/title/action button without prop-drilling |
| `hooks/` | `use-admin-page.ts` (`useAdminPage()` — reads/writes the `admin-page-context`); `use-admin-page-config.ts` (wraps `useAdminPage` in a `useEffect` that sets config on mount and clears it on unmount — call once per admin page); `use-theme.ts` (`useTheme()` for the app's own `ThemeProvider`, not `next-themes`); `use-sidebar.ts` (`useSidebar()` for the shadcn sidebar system); `use-dictionary-crud.ts` (shared list/search/modal/CRUD state for the admin dictionary pages — see `pages/AGENTS.md`); `use-mobile.ts` (`useIsMobile()`, a 768px `matchMedia` breakpoint hook used by the shadcn sidebar) |
| `lib/` | `utils.ts` — single `cn()` helper (`clsx` + `tailwind-merge`) used by every component in `components/ui/` for conditional class composition |
| `utils/` | `auth.ts` (`logout()` clears `localStorage["token"]` and dispatches the `AUTH_LOGOUT_EVENT` window event — see `components/auth/auth-session-listener.tsx`; `getUserInfo()` decodes the JWT payload from `localStorage["token"]`; `isAdmin()` checks `role === "Admin"`), `device.ts` (`getDeviceId()` — persists a random UUID in `localStorage["deviceId"]`, used to dedupe/identify anonymous survey submissions), `linked-filters.ts` (`getLinkedFilterOptions()` / `sanitizeLinkedFilters()` — pure functions that cross-filter department/discipline/speciality/specialization dictionaries so dependent `<Select>`s only show valid combinations; shared by `ContextSelector` and `AdminStatsPage`) |
| `components/` | All reusable/feature components — see `components/AGENTS.md` |
| `pages/` | All route-level page components — see `pages/AGENTS.md` |

## For AI Agents
### Working In This Directory
- `api.ts` is the single source of truth for backend calls and DTO shapes — when the backend contract changes, this is the one file to update; there is no codegen from OpenAPI/Swagger.
- Any page rendered inside `AppShell` must call `useAdminPageConfig({ title, subtitle, actions }, deps)` once, near the top of the component, rather than hardcoding a header or wrapping itself in a layout — see any file in `pages/admin/` for the pattern. Memoize `actions` (e.g. `useMemo`) if it depends on a callback, to avoid re-render loops through the `useEffect` dependency array; if the handler itself is recreated every render, keep it in a ref and let the memo depend only on what the header actually displays (`admin-stats-page.tsx` does this for its export button).
- Cross-filtering UI (any place that has Department/Discipline/Speciality/Specialization selects that should narrow each other) should reuse `getLinkedFilterOptions` / `sanitizeLinkedFilters` from `utils/linked-filters.ts` rather than re-implementing filter logic — it is already used by both the survey-taking flow (`ContextSelector`) and the stats/report filters (`AdminStatsPage`).
- `getDeviceId()` is the anonymous-submission identity mechanism: it is sent with every form submission and with `submissionsApi.getMyList()` (alongside the JWT `sub` claim) so a student's own past submissions can be looked up without a dedicated "my submissions" backend concept beyond `userId` + `deviceId` query params.

## Dependencies
### Internal
- `api.ts` types (`DictionaryItem`, `TeacherItem`, `Form`, `FormDetail`, `StatisticsFilters`, etc.) are imported throughout `components/` and `pages/` — treat them as the canonical frontend model layer.
- `contexts/admin-page-context.tsx` is only meaningful inside `components/admin/admin-shared.tsx`'s `AdminLayout`.

### External
- axios (HTTP), clsx + tailwind-merge (class composition), react (context/hooks), Tailwind CSS v4 + `@fontsource-variable/geist` (styling/fonts, via `index.css`)

<!-- MANUAL: -->
