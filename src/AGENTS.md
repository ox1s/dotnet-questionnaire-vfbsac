<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# src

## Purpose

Contains every C# project and both frontend apps. See the root `AGENTS.md` for the critical distinction between the **active** stack and the **legacy/prototype** stack before working in here — the two are unrelated implementations that happen to live side by side.

## Subdirectories

### Active stack (Clean Architecture, wired into `Aspire.AppHost`)

| Directory | Purpose |
|-----------|---------|
| `Domain/` | Entities, value objects, domain events — no framework dependencies (see `Domain/AGENTS.md`) |
| `Application/` | CQRS use cases (commands/queries + handlers), validation, application-level abstractions (see `Application/AGENTS.md`) |
| `Infrastructure/` | EF Core persistence, external service implementations, auth, caching, background jobs (see `Infrastructure/AGENTS.md`) |
| `Web.Api/` | ASP.NET Core host — controllers/endpoints, middleware, composition root (see `Web.Api/AGENTS.md`) |
| `SharedKernel/` | Base types shared across all four layers above (see `SharedKernel/AGENTS.md`) |
| `Aspire.AppHost/` | .NET Aspire orchestrator — boots `Web.Api`, Postgres, and the `Web.Client` Vite app together (see `Aspire.AppHost/AGENTS.md`) |
| `Aspire.ServiceDefaults/` | Shared Aspire service defaults (health checks, telemetry, resilience) referenced by `Web.Api` (see `Aspire.ServiceDefaults/AGENTS.md`) |
| `Web.Client/` | Active React frontend, run by Aspire as a Vite app (see `Web.Client/AGENTS.md`) |

### Legacy/prototype stack (not wired into Aspire; treat as reference-only unless told otherwise)

| Directory | Purpose |
|-----------|---------|
| `Questionnaire.Domain/` | Prototype domain layer (see `Questionnaire.Domain/AGENTS.md`) |
| `Questionnaire.Application/` | Prototype CQRS layer: Authentication, Forms, Questions, Reports, Surveys (see `Questionnaire.Application/AGENTS.md`) |
| `Questionnaire.Infrastructure/` | Prototype persistence/services (see `Questionnaire.Infrastructure/AGENTS.md`) |
| `Questionnaire.Api/` | Prototype ASP.NET Core host (see `Questionnaire.Api/AGENTS.md`) |
| `Questionnaire.Contracts/` | Prototype API request/response contracts (see `Questionnaire.Contracts/AGENTS.md`) |
| `Questionnaire.SharedKernel/` | Prototype shared base types (see `Questionnaire.SharedKernel/AGENTS.md`) |

## For AI Agents

### Working In This Directory
- Don't cross-wire the two stacks — e.g. never add a `ProjectReference` from an active-stack project to a `Questionnaire.*` project or vice versa; they're intentionally isolated.
- When adding a new active-stack feature, mirror the existing vertical-slice pattern already used by `Departments`/`Disciplines`/`Teachers`/etc.: a folder per feature under `Application/`, `Infrastructure/`, and `Web.Api/Endpoints/`, each containing `Create`/`Update`/`Delete`/`GetList`/`Restore` sub-slices as needed.

## Dependencies

### Internal
Layering (active stack): `Web.Api` → `Infrastructure` → `Application` → `Domain`, with `SharedKernel` referenced by all four. `tests/ArchitectureTests` enforces this — run it after structural changes.

<!-- MANUAL: -->
