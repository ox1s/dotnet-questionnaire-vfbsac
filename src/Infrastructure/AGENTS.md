<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Infrastructure

## Purpose
`Infrastructure` is the outer layer of the Clean Architecture solution: it implements the abstraction interfaces declared in `src/Application` (`Application.Abstractions.*`) using concrete technology — EF Core over PostgreSQL, ASP.NET Core JWT authentication, a custom claims-based permission/authorization system, PBKDF2 password hashing, ClosedXML/OpenXml report generation, and system time. It is referenced only by `src/Web.Api`, which calls the single `AddInfrastructure(configuration)` extension method exposed from `DependencyInjection.cs` to wire everything into the DI container. Nothing in this layer should be referenced by `Domain` or `Application` — dependencies flow inward only.

## Key Files
| File | Description |
|------|-------------|
| `DependencyInjection.cs` | Single composition root for the layer. `AddInfrastructure` chains `AddServices`, `AddDatabase`, `AddHealthChecksInternal`, `AddAuthenticationInternal`, `AddAuthorizationInternal` (uses C# 14 `extension` members on `IServiceCollection`). Registers `ApplicationDbContext` (Npgsql + snake_case naming convention), `IRepository<>` → `Repository<>`, JWT bearer auth, permission-based authorization, `IDateTimeProvider`, `IReportGenerator` (Excel active, Word commented out), `DemoDataGenerator`, `DbInitializer`. |
| `Infrastructure.csproj` | References `Application` project only. Package set includes Npgsql.EntityFrameworkCore.PostgreSQL, EFCore.NamingConventions, AspNetCore.HealthChecks.NpgSql, Microsoft.AspNetCore.Authentication.JwtBearer, ClosedXML, DocumentFormat.OpenXml, and (currently unused, see below) Quartz, Quartz.Extensions.Hosting, StackExchange.Redis, Microsoft.Extensions.Caching.StackExchangeRedis, Newtonsoft.Json. `InternalsVisibleTo ArchitectureTests`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Authentication/` | JWT token issuance (`TokenProvider`), PBKDF2 password hashing (`PasswordHasher`), current-user access (`UserContext`, `ClaimsPrincipalExtensions`). See details below. |
| `Authorization/` | Custom claims-based permission authorization (`HasPermissionAttribute`, `PermissionAuthorizationHandler`, `PermissionAuthorizationPolicyProvider`, `PermissionRequirement`, `PermissionProvider`). See details below. |
| `College/` | EF Core `IEntityTypeConfiguration<T>` classes for the five reference/dictionary entities: Department, Discipline, Speciality, Specialization, Teacher. See `College/AGENTS.md`. |
| `Database/` | `ApplicationDbContext`, generic `Repository<T>`, `DbInitializer` (migrate + seed on startup), `DemoDataGenerator` (Russian-language fake data for BGAS/telecom-college domain), `Schemas`. See `Database/AGENTS.md`. |
| `Migrations/` | EF Core migrations for `ApplicationDbContext`, generated via `dotnet ef migrations add`. Squashed to a single `InitialMigration` (2026-08-31) covering the full schema, plus `ApplicationDbContextModelSnapshot.cs` — the prior incremental migration history (previously 12 files) was collapsed since only mock/demo data existed in any deployed environment. Do not hand-edit `*.Designer.cs` or the snapshot — regenerate via the CLI (see "Working In This Directory" below). |
| `Questionnaires/` | EF Core configurations for `Form`/`Question` (`Form/`) and `Submission`/`Answer` (`Submission/`). See `Questionnaires/AGENTS.md`. |
| `Reports/` | `IReportGenerator` implementations: `ExcelReportGenerator` (ClosedXML, active) and `WordReportGenerator` (OpenXml, implemented but not registered in DI). See `Reports/AGENTS.md`. |
| `Time/` | `DateTimeProvider` — one-line `SharedKernel.IDateTimeProvider` implementation returning `DateTime.UtcNow`. Registered as singleton. |
| `Users/` | `UserConfiguration` — EF Core configuration for the `User` aggregate (owns `Login` value object, converts `Role` enum to string). |

## For AI Agents
### Working In This Directory
- **DbContext**: `Database/ApplicationDbContext.cs` implements `Application.Abstractions.Data.IApplicationDbContext`. It applies all `IEntityTypeConfiguration<T>` classes in the assembly via `ApplyConfigurationsFromAssembly`, sets `HasDefaultSchema("public")` (see `Database/Schemas.cs`), and adds soft-delete query filters (`!x.IsDeleted`) for `Discipline`, `Department`, `Speciality`, `Specialization`, `Form`, `Answer`, `Submission`, `Teacher`, `User`. When adding a new soft-deletable entity, add its query filter here too, or reads will leak deleted rows.
- **Naming convention**: `UseSnakeCaseNamingConvention()` (EFCore.NamingConventions) is applied in `DependencyInjection.AddDatabase`, so all table/column names in the actual Postgres schema are snake_case even though C# properties are PascalCase. `UserConfiguration` explicitly overrides this for the owned `Login.Value` property with `.HasColumnName("Login")` (note: NOT snake_case — an inconsistency to be aware of if querying raw SQL).
- **Migrations workflow**: add a migration from the repo root (or `src/Web.Api`, which is the startup project) with `dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Web.Api`. Apply with `dotnet ef database update` or let `DbInitializer.InitializeAsync()` call `context.Database.MigrateAsync()` at startup (invoked from `Web.Api` on boot). Never edit generated `*.Designer.cs` / `ModelSnapshot.cs` by hand.
- **Repository pattern**: `IRepository<T>` is a thin, generic, single-implementation wrapper (`Database/Repository.cs`) over `context.Set<T>()` (GetById, GetAll, Add, Update, Remove). It is registered as an open generic (`typeof(IRepository<>), typeof(Repository<>)`) — no per-entity repository classes exist or are needed; most query logic actually lives in `Application` using `IApplicationDbContext` directly via LINQ/EF, not through `IRepository<T>`.
- **Unused-but-referenced packages**: `Quartz`, `Quartz.Extensions.Hosting`, `StackExchange.Redis`, `Microsoft.Extensions.Caching.StackExchangeRedis`, and `Newtonsoft.Json` are all present in `Infrastructure.csproj` and some are `using`-imported in `DependencyInjection.cs`, but **none of them are actually invoked anywhere in this layer** (no `AddQuartz`, no `ConnectionMultiplexer`, no Quartz jobs, no Newtonsoft serialization calls exist as of this writing). Treat them as scaffolding for planned features (background jobs, distributed caching) rather than active infrastructure — don't assume caching or scheduled jobs exist just because the packages are referenced.
- **Report generation**: two `IReportGenerator` implementations exist (`Reports/ExcelReportGenerator.cs`, `Reports/WordReportGenerator.cs`), but only `ExcelReportGenerator` is registered in DI (`services.AddScoped<IReportGenerator, ExcelReportGenerator>();`); the Word registration line is commented out in `DependencyInjection.cs`. If you need Word export active, uncomment/swap that line — DI only allows one active `IReportGenerator` registration at a time (last-registered wins for a scoped interface, so don't register both without also changing to a factory/keyed pattern).
- **Auth model**: JWT is validated with a symmetric key from config (`Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationInMinutes`); `TokenProvider` embeds a `role` claim and one or more `permission` claims per token (permissions are derived from `UserRole` in a switch statement, duplicated between `TokenProvider.GetPermissionsForRole` and `Authorization/PermissionProvider.GetForUserIdAsync` — if you change role→permission mapping, update both). Authorization checks (`PermissionAuthorizationHandler`) check the `permission` claim directly on `ClaimsPrincipal`; `PermissionProvider` (DB-backed permission lookup) is registered in DI but has no other callers in this layer — verify in `Web.Api`/`Application` before assuming it's dead code.
- **Demo/seed data**: `DbInitializer.InitializeAsync()` runs migrations then seeds only if `Forms` table is empty, delegating to `DemoDataGenerator` which builds a full fake dataset (Belarusian State Academy of Communications telecom college domain, Russian text) — departments, teachers, specialities/specializations, disciplines, users (including `ADMIN`/`HEAD_ICT`/`STAFF##` logins), two demo forms, and 230 demo submissions with plausible score distributions. Default seeded password for all demo accounts is `"12345678"` (hashed via `IPasswordHasher`).

## Dependencies
### Internal
- `src/Application` — implements `Application.Abstractions.Data.{IApplicationDbContext, IRepository<T>}`, `Application.Abstractions.Authentication.{IPasswordHasher, ITokenProvider, IUserContext}`, `Application.Abstractions.Reports.IReportGenerator`; consumes `Application.Reports.Queries.*` response DTOs in the report generators.
- `src/Domain` — EF Core configurations map `Domain.College.*`, `Domain.Questionnaires.*`, `Domain.User` aggregates/value objects.
- `src/SharedKernel` — implements `IDateTimeProvider`; consumes `Entity` (generic repository constraint), `Result<T>`, `Permissions` constants.
- Consumed by `src/Web.Api` via `AddInfrastructure(configuration)`; `Web.Api` is the EF Core startup/migrations project.

### External
- `Npgsql.EntityFrameworkCore.PostgreSQL`, `EFCore.NamingConventions`, `AspNetCore.HealthChecks.NpgSql` — PostgreSQL persistence, snake_case schema, health checks.
- `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.IdentityModel.Tokens` / `.JsonWebTokens` — JWT issuance and validation.
- `ClosedXML` — active Excel (`.xlsx`) report generation.
- `DocumentFormat.OpenXml` — Word (`.docx`) report generation (implemented, not DI-registered).
- `Quartz`, `Quartz.Extensions.Hosting`, `StackExchange.Redis`, `Microsoft.Extensions.Caching.StackExchangeRedis`, `Newtonsoft.Json` — referenced, currently unused (see gotcha above).

<!-- MANUAL: -->
