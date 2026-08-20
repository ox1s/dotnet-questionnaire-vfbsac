<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Web.Client

## Purpose
`Web.Client` is the active React frontend of the college questionnaire/evaluation platform ("ВФБГАС" per the sidebar logo). It is a React 19 + TypeScript SPA built with Vite, styled with Tailwind CSS v4 and shadcn/Radix UI primitives, and talks exclusively to the `Web.Api` backend through a single axios instance proxied under `/api`. The app has two audiences: students/groups who fill out survey forms, and Admins/DeputyHeads who manage dictionaries (departments, teachers, disciplines, specialities, specializations), build forms, and view analytics/reports.

## Key Files
| File | Description |
|------|-------------|
| `package.json` | Scripts (`dev`, `build`, `lint`, `preview`) and dependencies: React 19, react-router-dom 7, axios, radix-ui, recharts, date-fns, sonner/react-hot-toast, next-themes, class-variance-authority, tailwind-merge, Tailwind CSS v4 |
| `vite.config.ts` | Vite config: React plugin, Tailwind Vite plugin, `@` alias → `./src`, dev-server proxy `/api` → `https://localhost:5001` (strips the `/api` prefix, `secure: false` for the local dev cert) |
| `index.html` | SPA shell; loads Google Fonts "Manrope" (unused — actual body font is self-hosted Geist Variable via `@fontsource-variable/geist`) and mounts `#root` |
| `components.json` | shadcn CLI config: style `radix-lyra`, base color `mist`, icon library `lucide`, path aliases (`@/components`, `@/lib`, `@/hooks`, `@/components/ui`) |
| `eslint.config.js` | Flat ESLint config: `typescript-eslint` recommended + `react-hooks` + `react-refresh` (Vite) rules over `**/*.{ts,tsx}` |
| `tsconfig.json` / `tsconfig.app.json` / `tsconfig.node.json` | TS project references; app config targets the browser, node config covers `vite.config.ts` |
| `public/logo.jpg`, `public/logo.png` | Static institution logo used in the admin sidebar (`AppSidebar`) |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | All application source — see `src/AGENTS.md` for `api.ts`, contexts, hooks, lib, utils, and links to `components/` and `pages/` docs |

## For AI Agents
### Working In This Directory
- **How it runs**: In normal local development this app is **not** started with a bare `npm run dev`. It is booted by `src/Aspire.AppHost/Program.cs` as a Vite resource: `builder.AddViteApp("frontend", "../Web.Client")`, alongside the `Web.Api` backend project and Postgres. Aspire manages the process, port, and env wiring, and both `backend` and `frontend` are exposed together through the `public-api` dev tunnel. Running `npm run dev` directly in this folder also works for isolated frontend-only work, but the backend must be reachable at `https://localhost:5001` (see the Vite proxy) for API calls to succeed — Aspire orchestration is the "real" way the full stack runs.
- **Build**: `npm run build` runs `tsc -b` (project-reference type check) then `vite build`. A type error anywhere in `src/` fails the build.
- **Auth model**: JWT stored in `localStorage["token"]`; role and user id are decoded client-side from the JWT payload (see `src/utils/auth.ts`). There is no refresh-token flow — a 401 anywhere clears the token and hard-redirects to `/login` (see `src/api.ts` response interceptor).
- **Styling**: Tailwind v4 utility classes directly in JSX; no CSS Modules. Design system is shadcn "new-york"-style components under `src/components/ui/` built on Radix primitives + `class-variance-authority` (`cva`) for variants, composed with the `cn()` helper (`clsx` + `tailwind-merge`) from `src/lib/utils.ts`. UI text is Russian throughout (admin/user-facing strings, toasts, validation messages).
- **State management**: No global state library (no Redux/Zustand/React Query). Pages own their own `useState`/`useEffect` data-fetching and pass callbacks down. The one exception is `AdminPageContext` (`src/contexts/admin-page-context.tsx` + `src/hooks/use-admin-page-config.ts`), which lets any admin page push a `{title, subtitle, actions}` config up into the shared `AdminLayout` header/breadcrumb without prop-drilling through the router.
- **API-calling pattern**: All HTTP calls go through the single `api` axios instance exported from `src/api.ts` (baseURL `/api`, Bearer token injected via request interceptor, 401 → logout via response interceptor). Endpoints are grouped into named objects (`usersApi`, `settingsApi`, `dictionariesApi`, `submissionsApi`, `reportsApi`) rather than one-off calls scattered through components, though pages occasionally call `api.get/post/delete` directly for `/forms` and `/submissions`. Errors are surfaced with `getApiErrorMessage()` (reads ASP.NET `ProblemDetails`-shaped `detail`/`errors`/`title`) and shown via `sonner`'s `toast`.
- **Routing**: `react-router-dom` v7 with `BrowserRouter`; all route definitions live in `src/App.tsx`. Route guarding uses `<ProtectedRoute allowedRoles={[...]} />` as a layout route wrapping `<Outlet />`; unauthenticated users are redirected to `/login`, and users lacking a required role are redirected to `/dashboard`. Admin-only routes are additionally wrapped in `<AdminLayout />` (sidebar + breadcrumb chrome).

## Dependencies
### Internal
- Consumes `Web.Api` HTTP endpoints only (Departments/Disciplines/Teachers/Specialities/Specializations/Forms/Submissions/Reports/Users) — no shared TS types are generated from the backend; DTOs are hand-written interfaces in `src/api.ts` and must be kept in sync manually with the API contract.
- Booted by `src/Aspire.AppHost` (`AddViteApp("frontend", "../Web.Client")`) as part of the local Aspire app graph.

### External
- react, react-dom, react-router-dom, axios, radix-ui, lucide-react, recharts, date-fns, next-themes, react-hot-toast, sonner, class-variance-authority, clsx, tailwind-merge, tw-animate-css, tailwindcss (v4), @fontsource-variable/geist, shadcn (CLI, not a runtime dep of note), vite, typescript

<!-- MANUAL: -->
