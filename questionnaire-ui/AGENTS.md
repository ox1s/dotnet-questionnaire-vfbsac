<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# questionnaire-ui

## Purpose
LEGACY/SUPERSEDED by `src/Web.Client` — not the active frontend. This is an earlier, smaller React 19 + Vite SPA for the same questionnaire product: end users log in, take surveys (rating/text/choice questions), and admins manage questions, forms, and view summary reports (with Recharts bar charts). Evidence of its legacy status: only 4 git commits (last on 2025-12-04, log message titles in Russian — "feat: front", "feat: minimal frontend with errors", "feat: frontend base"), it is NOT registered in `src/Aspire.AppHost` (which only wires up `Web.Api` + `Web.Client`), and it uses a different, mixed UI stack — MUI (`@mui/material`, `@emotion`) for most screens plus Zustand for auth state, with react-aria-components/tailwind-variants only in the `Button` component and `LoginPage` — versus `Web.Client`'s Radix/shadcn + Tailwind throughout. Confirmed backend target: `src/api/axios.ts` hardcodes `baseURL: 'http://localhost:5202'`, which matches only `src/Questionnaire.Api/Properties/launchSettings.json` (the legacy standalone API, not `src/Web.Api` which runs on port 5000/5001). So this app was built to pair with `Questionnaire.Api`, not the active `Web.Api`/`Web.Client` stack. **Update 2026-08-11: `src/Questionnaire.Api` (and the rest of the `Questionnaire.*` backend) has been deleted from the repo entirely** — this frontend is now fully orphaned with no backend to call at all, on top of already being superseded by `Web.Client`. Treat this directory as historical reference only; do not extend it as if it were a working app.

## Key Files
| File | Description |
|------|--------------|
| `src/main.tsx` | React app entry point, mounts `App` |
| `src/App.tsx` | Router setup: `/login`, `/` (dashboard), `/surveys/:id`, and `/admin/*` (questions, forms, forms/:id, reports/:id) behind `ProtectedRoute`/`AdminRoute` |
| `src/api/axios.ts` | Axios instance; `baseURL: 'http://localhost:5202'` (legacy `Questionnaire.Api`), attaches `Bearer` token from `localStorage['authToken']` via request interceptor |
| `src/store/authStore.ts` | Zustand store: login/logout/initialize, decodes JWT with `jwt-decode` to extract ASP.NET role claim into `roles[]` |
| `src/components/ProtectedRoute.tsx` / `src/components/AdminRoute.tsx` | Route guards based on `authStore` auth state / `roles.includes('admin')` |
| `src/components/QuestionRenderer.tsx` | Renders a question by `type` (0=Rating with weight+mark selects, 1=Text, 2=Choice — Choice UI is a stub, not implemented) |
| `vite.config.ts`, `tsconfig*.json` | Vite/TS build config (uses `rolldown-vite` override) |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `public/` | Static assets served as-is (default Vite `vite.svg`) — no app-specific content |
| `src/assets/` | Bundled static assets (default Vite `react.svg`) — unused/boilerplate |
| `src/api/` | `axios.ts` (client + auth interceptor), `surveyService.ts` (GET `/surveys`, GET `/forms/:id`, POST `/surveys/submit`), `adminService.ts` (CRUD for `/admin/questions`, `/forms`, form-question links `/forms/:id/questions/:qId`, `/reports/summary/:id`) |
| `src/components/admin/` | `AdminLayout.tsx` (MUI Tabs shell + `Outlet` for admin routes), `QuestionForm.tsx` (create-question form with type/options), `QuestionsTable.tsx` (list + delete questions) |
| `src/components/ui/` | Just `Button.tsx` — a react-aria-components + tailwind-variants button (primary/secondary/destructive); the only non-MUI UI primitive in the app, used only by `LoginPage` |
| `src/pages/` | `LoginPage`, `DashboardPage` (lists surveys), `SurveyPage` (take a survey, submit answers), `AdminQuestionsPage`, `AdminFormsPage` (create/delete forms, links to edit/report), `AdminFormDetailPage` (attach/detach questions to a form), `AdminReportPage` (per-form summary report with Recharts bar chart for Choice questions) |
| `src/store/` | `authStore.ts` — sole Zustand store; holds token/isAuthenticated/isLoading/roles |
| `src/types/` | `auth.ts` (`LoginRequest`), `survey.ts` (`Question`, `Survey`, `SurveyDetail`, `AnswerDetail`, `SubmitSurveyPayload`; question `type`: 0=Rating,1=Text,2=Choice), `report.ts` (`SummaryReport`, `QuestionSummary`, rating/choice summary shapes) |

## For AI Agents
### Working In This Directory
This app is superseded by `src/Web.Client` and is not wired into `src/Aspire.AppHost`. Confirm with the user before investing significant work here — new features almost certainly belong in `src/Web.Client` instead. If asked to fix a bug here, first check whether the equivalent flow already exists (and is better implemented) in `src/Web.Client`.

## Dependencies
### Internal
- Targeted the `src/Questionnaire.Api` backend (hardcoded `http://localhost:5202` in `src/api/axios.ts`), not `src/Web.Api`. That backend was deleted from the repo on 2026-08-11 — this app currently has nothing to call.
- No shared code with `src/Web.Client`; fully independent Vite project with its own `package.json`.

### External
- React 19, react-router-dom 7, Vite (via `rolldown-vite` override)
- MUI (`@mui/material`, `@mui/icons-material`, `@emotion/*`) — primary UI kit for almost all screens
- Zustand — auth state store
- axios, jwt-decode — API client and JWT role-claim decoding
- recharts — bar chart in `AdminReportPage`
- react-aria-components, tailwind-variants, tailwind-merge, Tailwind CSS 4 — used only in `src/components/ui/Button.tsx` and `LoginPage` (a partial, unfinished migration toward the newer stack seen in `Web.Client`)

<!-- MANUAL: -->
