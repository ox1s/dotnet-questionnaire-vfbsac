<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# pages

## Purpose
Route-level screens, one component per route registered in `src/App.tsx`, grouped by area: `auth/` (login), `dashboard/` (the post-login landing page, which branches by role), `forms/` (form builder + survey-taking), and `admin/` (dictionary CRUD screens, group/user management, semester settings, and the analytics/report screen). Admin pages are rendered inside the shared `<AppShell>` layout route (`components/layout/app-shell.tsx`), which is the only thing that mounts `<AdminLayout>`. A page never wraps itself in a layout — it returns just its content and pushes its header through `useAdminPageConfig`.

## Key Files
| File | Route | Description |
|------|-------|-------------|
| **auth/** | | |
| `auth/login-page.tsx` | `/login` | `LoginPage` — login form (`login`/`password` fields, Russian "Группа" hint), `POST /users/login`, stores the returned JWT string directly in `localStorage["token"]`, navigates to `/dashboard`. No refresh-token/remember-me logic |
| **dashboard/** | | |
| `dashboard/dashboard-page.tsx` | `/dashboard` | `DashboardPage` — a one-line role switch between two sibling components sharing a `useForms` hook: `AdminDashboardPage` (loads `formsApi.getAll()`, sets its header via `useAdminPageConfig`, owns `deleteForm`/`toggleFormActive`) and `UserDashboardPage` (loads `GET /forms`, renders the standalone `UserDashboard`). Two components rather than one branchy one, so each holds only the hooks its own variant needs |
| **forms/** | | |
| `forms/create-form-page.tsx` | `/admin/create-form` | `CreateFormPage` — the form builder: title textarea, toggleable required-filter buttons (`Teacher`/`Discipline`/`Department`/`Speciality`/`EmployeeCategory`), add/reorder (buttons + native HTML5 drag-and-drop)/remove questions (`Text`/`Number`/`WeightedRating`, numeric `QuestionType` enum matching the backend), `POST /forms` on save (save button disabled while in flight; errors surfaced via `getApiErrorMessage`). Uses `useAdminPageConfig` to push its title/save-button into the shared `AdminLayout` header |
| `forms/survey-page.tsx` | `/form/:id` | `SurveyPage` — loads `GET /forms/:id`, renders `<ContextSelector>` then one `<Card>` per question keyed by `q.type` (`WeightedRating`→`WeightedRatingInput`, `Text`→`Textarea`, `Number`→`Input type=number`), validates required filters client-side before submit, `POST /submissions` with `deviceId` (from `getDeviceId()`) + context + answers; shows a Russian toast and redirects to `/dashboard` on success, handles HTTP 409 (already voted for this teacher/discipline) with a specific message |
| **admin/** | | |
| `admin/admin-departments-page.tsx` | `/admin/departments` | CRUD for department/"кафедра" dictionary items via `dictionariesApi` (`getDepartments`/`create`/`update`/`delete`/`restore`Department). Soft-delete pattern: deleted rows stay listed (grayed, "Удалено" badge) with a restore action instead of disappearing |
| `admin/admin-disciplines-page.tsx` | `/admin/disciplines` | CRUD for disciplines; each discipline belongs to a department (`<Select>` of non-deleted departments), same soft-delete/restore pattern |
| `admin/admin-specialities-page.tsx` | `/admin/specialities` | CRUD for specialities; simplest of the dictionary pages (name only) |
| `admin/admin-specializations-page.tsx` | `/admin/specializations` | CRUD for specializations; each belongs to a speciality (`<Select>`); note the `SpecializationItem` type locally widens `DictionaryItem` and the code falls back to `departmentId` if `specialityId` is absent — a defensive compatibility shim, not the primary field |
| `admin/admin-teachers-page.tsx` | `/admin/teachers` | CRUD for teachers; each optionally belongs to a department; label formatting collapses long names (`truncateFirstWord`) in the table |
| `admin/admin-groups-page.tsx` | `/admin/groups` | User-management for **student group accounts** (login = group name) via `usersApi` (`getGroups`/`createGroup`/`updateUser`/`setPassword`/`deleteUser`); auto-generates an 8-digit numeric password, shows a one-time "Группа создана — Логин/Пароль" banner after creation since the password isn't re-displayable later |
| `admin/admin-settings-page.tsx` | `/admin/settings` | Admin's own password change (`usersApi.setPassword` using `getUserInfo().sub`) plus the semester lifecycle controls: `settingsApi.closeSemester()`/`openSemester()`, both gated behind a native `window.confirm()` since closing deactivates all forms for students |
| `admin/admin-stats-page.tsx` | `/admin/stats/:id` | `AdminStatsPage` — the analytics/report screen, by far the largest page. Three modes (`single` period / `periods` compare / `groups` compare by Department·Discipline·Speciality·Specialization·Teacher), date-range pickers (`date-fns` + shadcn `Calendar`/`Popover`), cross-filterable dictionary selects (via `utils/linked-filters`), a Recharts `BarChart` of per-question means, a `QuestionsTable` detail table, a `TextAnswersSection` for free-text answers (with client-side grouping by teacher/department/discipline), and Word-document export (`reportsApi.exportAnalyticsBy*`, blob download via an in-memory `<a download>` link). Calls `reportsApi.getAnalyticsByPeriod/getAnalyticsByPeriods/getAnalyticsByGroups` depending on mode and normalizes all three response shapes into a common `PeriodAnalyticsResponse[]` for rendering |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `admin/` | Dictionary CRUD screens, group/user management, semester settings, analytics/report screen — all Admin-only |
| `auth/` | Login screen |
| `dashboard/` | Post-login landing page (role-branching) |
| `forms/` | Form builder (admin) and survey-taking flow (student) |

## For AI Agents
### Working In This Directory
- **Page shape**: every page is a single default-exportable-but-actually-named-exported (`export const XPage = () => {...}`) function component that owns its own data fetching (`useEffect` + `async loadData()`), no route loaders/actions and no data-fetching library — this is the consistent pattern to follow for new pages.
- **Admin page boilerplate**: every `admin/*` page follows the same skeleton — `useState` for the list + form fields, a `loadData()` async function called from `useEffect(() => { loadData() }, [])`, `useAdminPageConfig({ title, subtitle, actions })` near the top to set the `AdminLayout` header/breadcrumb, then `<AdminTable>` + `<AdminModal>` from `@/components/admin/admin-shared`. Copy the smallest one (`admin-specialities-page.tsx`) as a template for a new simple dictionary CRUD page, or `admin-disciplines-page.tsx`/`admin-teachers-page.tsx` for one with a parent-dictionary `<Select>`.
- **Soft delete convention**: dictionary items carry `isDeleted?: boolean`; delete buttons call a `delete*` endpoint but the row stays visible (grayed + badge) with a `restore*` action — don't switch this to actually removing the row from local state on delete.
- **Error handling convention**: wrap API calls in try/catch, surface failures via `sonner`'s `toast.error(getApiErrorMessage(e, "<Russian fallback>"))`; success via `toast.success("...")`. A few older pages (`admin-disciplines-page.tsx`'s `handleSubmit`) just do `toast.error("Ошибка")` without `getApiErrorMessage` — prefer the `getApiErrorMessage` form for new/edited code.
- **`AdminStatsPage` is the most complex file in the whole app** (~1200 lines) — before extending it, read the `Mode`/`CompareField`/`buildRequest`/`loadReport` flow fully; it silently normalizes three different backend response shapes (`AnalyticsByPeriodResponse[]`, `PeriodAnalyticsResponse[]`, `GroupAnalyticsResponse[]`) into one `PeriodAnalyticsResponse[]` shape client-side with derived `totalSubmissions`/`overallAverage`/`overallStandardDeviation` — don't duplicate that normalization elsewhere; extend it in place if a fourth mode is needed.

## Dependencies
### Internal
- `@/api` (axios instance, DTOs, `usersApi`/`settingsApi`/`dictionariesApi`/`submissionsApi`/`reportsApi`, `getApiErrorMessage`)
- `@/components/admin/admin-shared` (`AdminTable`, `AdminModal`, `AdminTableActions`, `AdminTableIconCell`, `AdminTableRow`, `AdminTableTextBadge`) — note `AdminLayout` is deliberately absent: only `components/layout/app-shell.tsx` may import it
- `@/components/dashboard/*`, `@/components/survey/*`, `@/components/shared/filter-select`
- `@/hooks/use-admin-page-config`, `@/utils/auth`, `@/utils/device`, `@/utils/linked-filters`, `@/lib/utils`

### External
- react-router-dom (`useNavigate`, `useParams`), sonner (`toast`), lucide-react (icons), recharts (`AdminStatsPage` chart), date-fns (+ `date-fns/locale/ru`) for date formatting/parsing

<!-- MANUAL: -->
