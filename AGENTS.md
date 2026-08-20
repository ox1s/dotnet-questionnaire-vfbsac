<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# dotnet-questionnaire-vfbsac (Система Анкетирования)

## Purpose

A .NET 10 platform for anonymous questionnaires used to evaluate teaching/education quality at a college — students, staff, and (planned) employers fill out forms; admins manage groups, dictionaries (teachers, specialities, specializations, departments, disciplines), and view analytics/reports. It replaces paper questionnaires while preserving the existing scoring methodology (see `college-specs/`).

## History: a legacy prototype backend/frontend used to live here

As of 2026-08-11, this repo used to contain a second, unrelated, abandoned prototype implementation of the questionnaire backend (`src/Questionnaire.Domain`, `src/Questionnaire.Application`, `src/Questionnaire.Infrastructure`, `src/Questionnaire.Api`, `src/Questionnaire.Contracts`, `src/Questionnaire.SharedKernel` — an AI-assisted scaffolding experiment, ~8 commits, last touched 2025-12-05, never wired into `src/Aspire.AppHost`). **It has been deleted** — the stack described below is the only backend now. Two remnants of that era still exist and are now fully non-functional/orphaned, kept only as historical reference until someone decides to remove them too:
- `questionnaire-ui/` — a legacy React frontend that called the now-deleted `Questionnaire.Api` (see `questionnaire-ui/AGENTS.md`).
- `requests/*.http` — manual request files that target the now-deleted `Questionnaire.Api` (see `requests/AGENTS.md`).

The root `Questionnaire.sln` (which only ever registered the deleted prototype's projects) has also been deleted. The real, current, correctly-scoped solution file for the active stack is **`src/Questionnaire.slnx`** (the newer XML-based solution format) — use that in your IDE, not a root-level `.sln`.

## Key Files

| File | Description |
|------|-------------|
| `aspire.config.json` | Points the Aspire CLI/tooling at `src/Aspire.AppHost/Aspire.AppHost.csproj`, the real entry point for running the app |
| `Directory.Packages.props` | Central NuGet package version management (`ManagePackageVersionsCentrally`) for all C# projects |
| `Directory.Build.props` | Shared MSBuild settings applied to every C# project under `src/`/`tests/` |
| `.editorconfig` | C# formatting/analyzer rules — sizeable, check before assuming a style |
| `README.md` | Project description (Russian) + feature checklist + Jira link |
| `TODO.md` | Outstanding product tasks (Russian): open/close forms per-item, employer login, split admin stats page, move generated text to resource files |

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `src/` | All backend C# projects and both frontend apps (see `src/AGENTS.md`) |
| `tests/` | Unit tests + architecture/layering tests (see `tests/AGENTS.md`) |
| `questionnaire-ui/` | Orphaned legacy React frontend, superseded by `src/Web.Client`; its backend (`Questionnaire.Api`) has been deleted (see `questionnaire-ui/AGENTS.md`) |
| `requests/` | `.http` manual request files — target the now-deleted `Questionnaire.Api` (see `requests/AGENTS.md`) |
| `college-specs/` | Source scoring-methodology documents the questionnaire logic must reproduce (see `college-specs/AGENTS.md`) |
| `.github/` | CI workflows |
| `.cursor/rules/` | Cursor IDE rule file with ASP.NET Core 10 coding conventions (C# 12, primary constructors, `is null` checks, `internal sealed` by default, etc.) — apply these conventions to all new C# code regardless of editor |
| `.aspire/`, `.idea/`, `.vs/`, `.claude/`, `.omc/` | Tooling/IDE state — not project source, generally skip |

## For AI Agents

### Working In This Directory
- `questionnaire-ui/` and `requests/` are historical leftovers from the deleted prototype and have no working backend — don't treat them as real feature targets without checking with the user first.
- Follow `.cursor/rules/dotnet-rules.mdc` conventions for any C# you write: C# 12 features, primary constructors for DI, records for immutable data, controller endpoints preferred over minimal APIs (minimal APIs only for simple cases), explicit typing (avoid `var` unless the type is obvious), `internal sealed` by default, `Guid` identifiers by default, `is null`/`is not null` instead of `== null`/`!= null`.
- The product spec/README is in Russian; feel free to respond in the language the user uses, but keep code, identifiers, and comments in English to match the existing codebase.

### Testing Requirements
- `tests/Application.UnitTests` (business logic — currently an empty scaffold, no test classes yet) and `tests/ArchitectureTests` (layering rules — verify you haven't violated Clean Architecture boundaries between `Domain`/`Application`/`Infrastructure`/`Web.Api`).
- No automated tests exist for either frontend.

## Dependencies

### External
- .NET 10 / ASP.NET Core, EF Core + Npgsql (PostgreSQL), .NET Aspire (orchestration), Redis (caching), Quartz (background jobs), Serilog, Swashbuckle/OpenAPI.
- Frontends: React 19, Vite, TypeScript, Tailwind CSS.

<!-- MANUAL: Any manually added notes below this line are preserved on regeneration -->
