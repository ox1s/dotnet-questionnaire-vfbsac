<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# requests/

## Purpose
Manual `.http` request collections (REST Client / VS Code "Rest Client" extension / Rider HTTP client format) for exercising the API by hand during development. These files target the LEGACY `Questionnaire.Api` project, not the active `Web.Api` stack. Confirmed from the decoded JWT payloads embedded in these files: `"iss":"Questionnaire.Api","aud":"Questionnaire.Api"`. The route shapes (`/auth/login`, `/auth/register`, `/admin/questions`, `/surveys`, `/surveys/submit`, `/forms`, `/forms/{id}/questions/{qid}`, `/reports/summary/{id}`, `/reports/export/{id}`) and payload field names match `src/Questionnaire.Application`'s feature folders (Authentication, Questions, Surveys, Forms, Reports) — not the active stack's Departments/Disciplines/Teachers/Users/SignIn naming used by `src/Application`/`src/Web.Api`. All files point at `@host = http://localhost:5202`, the legacy `Questionnaire.Api`'s local dev port.

## Key Files
| File | Description |
|------|--------------|
| `Login.http` | `POST /auth/login` with `{Login, Password}` JSON body. Example uses `admin`/`admin`. |
| `Register.http` | `POST /auth/register` with `{Login, Password, Role}` JSON body. Example registers a `student` role user. |
| `GetAdmin.http` | `GET /admin/data` with a hardcoded `Bearer` JWT (admin role) — likely a smoke-test / role-gated endpoint check. |
| `Questions.http` | Admin-only question management: `GET /admin/questions` (list), `POST /admin/questions` to create a question. Demonstrates three question `Type` values by example: `0` = Rating (no `Options`), `2` = Choice (with `Options` array of strings), and a deliberately invalid case (`Type: 1` with `Options` present) expected to return `400 Bad Request` — implying `Type: 1` must NOT have options (likely a free-text/open type). |
| `Surveys.http` | Student-facing: `GET /surveys` (surveys available to the caller's role — a comment notes a manual DB step is required first, linking a form to the `student` role via a `FormRoles` table) and `POST /surveys/submit` with `{formId, details:[{questionId, weight, mark, textResponse}]}`. Includes both a valid submission and an intentionally invalid one (`mark` exceeding `weight`) expected to return `400`. |
| `Forms.http` | Admin flow: `GET /forms/{id}`, `GET /forms/999` (expected `404`), `GET /forms` (list all), `POST /forms` to create `{Name}`, `POST /forms/{formId}/questions/{questionId}` with `{Order}` to attach a question to a form, plus negative cases for duplicate attach (`409 Conflict`) and attaching a non-existent question (`404`). |
| `Reports.http` | Admin reporting: `GET /reports/summary/{formId}` and `GET /reports/export/{formId}` (comment says the export produces `.docx`). Comment notes the form must already have submitted answers for the summary to be meaningful. |

## For AI Agents
### Working In This Directory
- Do not use these files as a reference for the active `Web.Api` route surface. If asked to add/update `.http` requests for the current backend (`src/Web.Api`), these are the wrong template to copy from — check `src/Web.Api`'s controllers/endpoints for the real route names (Departments, Disciplines, Teachers, Users, SignIn, etc.) instead.
- Every file embeds a real-looking (expired, local-dev-only) JWT as a literal `@jwt_token` variable. These are throwaway tokens signed with the legacy stack's dev secret, not credentials that need protecting — but don't paste real production tokens into this style of file.
- Several requests are deliberately negative-path examples annotated with Russian-language `###` comments stating the expected HTTP status (e.g. "Должен вернуться статус 400 Bad Request"). When adding new requests, keep the convention of a `###` separator comment above each request block, in Russian to match existing style, describing intent and (for negative cases) the expected outcome.
- Route base URL is set once per file via `@host = http://localhost:5202` — if the legacy API's dev port changes, all seven files need updating individually (there is no shared/global `.http` config in this directory).
- `Questions.http`'s comments reveal a validation rule worth knowing if working on the legacy `Questionnaire.Application.Questions` feature: question `Type` values map to enum-like ints where only certain types may carry `Options` (`Type: 2` = Choice requires options; `Type: 0` = Rating and `Type: 1` must have `Options: null`, otherwise `400`).
- `Surveys.http`'s comment reveals a manual data-setup dependency: a survey/form is only visible to a role via a `FormRoles` join table that must be populated by hand (no seed data or admin UI shown here) before `GET /surveys` returns anything for that role.

## Dependencies
### Internal
- Targets `src/Questionnaire.Api` (legacy presentation layer) exclusively, which in turn depends on `src/Questionnaire.Application`, `src/Questionnaire.Infrastructure`, `src/Questionnaire.Domain`, `src/Questionnaire.Contracts`, `src/Questionnaire.SharedKernel`.
- No relationship to `src/Web.Api`, `src/Application`, `src/Infrastructure`, `src/Domain`, or `src/SharedKernel` (the active stack).

### External
- Requires a REST-client tool capable of parsing `.http` files with `@variable` substitution (VS Code REST Client extension, JetBrains Rider/VS HTTP client, etc.) — not runnable via `dotnet` or a test runner.
- Requires the legacy `Questionnaire.Api` project running locally on port 5202 and a populated dev database (including manual `FormRoles` rows for `Surveys.http` to return data).

<!-- MANUAL: -->
