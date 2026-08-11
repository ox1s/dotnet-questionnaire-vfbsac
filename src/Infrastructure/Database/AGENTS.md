<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Database

## Purpose
Owns the EF Core `DbContext`, the generic repository implementation, startup migration/seeding orchestration, and the demo-data generator used for local/dev environments. This is the persistence backbone that every other part of `Infrastructure` (and, through `IApplicationDbContext`/`IRepository<T>`, all of `Application`) ultimately reads and writes through.

## Key Files
| File | Description |
|------|-------------|
| `ApplicationDbContext.cs` | `sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext>) : DbContext, IApplicationDbContext`. Exposes `DbSet<T>` for `User`, `Form`, `Answer`, `Submission`, `Department`, `Discipline`, `Teacher`, `Speciality`, `Question`, `Specialization`. `OnModelCreating` calls `ApplyConfigurationsFromAssembly`, sets default schema `public`, and applies `!IsDeleted` global query filters to every soft-deletable entity. `SaveChangesAsync` is overridden but currently just forwards to `base` (no domain-event dispatch, outbox, or auditing hook — despite an earlier `DeleteOutbox` migration suggesting an outbox pattern used to exist and was removed). |
| `DbInitializer.cs` | `DbInitializer(IServiceProvider, ILogger<DbInitializer>, DemoDataGenerator)`. `InitializeAsync()` creates a DI scope, runs `context.Database.MigrateAsync()`, and — only if `Forms` table is empty — hashes a default password (`"12345678"`) and calls `DemoDataGenerator.SeedAsync`. Called once at application startup (from `Web.Api`, outside this layer). |
| `DemoDataGenerator.cs` | ~520 lines. Builds a full realistic dataset for a Belarusian telecom college (BGAS — Белорусская государственная академия связи): 9 departments, 22 teachers, 3 specialities, 5 specializations, 18 disciplines (department-templated), ~24 users (1 admin, 22 student-group users, 1 deputy-head, up to 9 staff), 2 demo `Form`s (a discipline-satisfaction form with `WeightedRating` questions and a practice-supervisor form with `Number`/`MultipleChoice`/`Text` questions), and 230 demo `Submission`s (140 discipline-form + 90 practice-form) with per-department score baselines (`GetDepartmentScore`) and randomized realistic noise (`RandomNumberGenerator`-based, not `System.Random`). Uses reflection (`typeof(Submission).GetProperty(...).SetValue(...)`) to backdate `Submission.SubmittedAt` since the domain has no public setter for it — a workaround, not a general pattern to copy elsewhere. |
| `Repository.cs` | `internal sealed class Repository<T>(IApplicationDbContext) : IRepository<T> where T : Entity`. Thin wrapper: `GetByIdAsync`, `GetAllAsync`, `Add`, `Update`, `Remove`, all delegating to `context.Set<T>()`. No entity-specific overrides exist anywhere — if a query needs filtering/joins, callers use `IApplicationDbContext` LINQ directly instead of extending this class. |
| `Schemas.cs` | `internal static class Schemas { public const string Default = "public"; }` — single constant, used by `ApplicationDbContext` and `DependencyInjection.AddDatabase` (for the EF migrations history table schema). |

## For AI Agents
### Working In This Directory
- `ApplicationDbContext` is registered via `services.AddDbContext<ApplicationDbContext>(...)` in `../DependencyInjection.cs` with `UseNpgsql(...).UseSnakeCaseNamingConvention()`; `IApplicationDbContext` is registered separately as a scoped forwarding lambda (`sp => sp.GetRequiredService<ApplicationDbContext>()`) so `Application`-layer code depends only on the interface.
- If you add a new aggregate/entity: (1) add a `DbSet<T>` property here, (2) add an `IEntityTypeConfiguration<T>` in the matching subfolder (see `College/`, `Questionnaires/`, `Users/` for conventions), (3) if soft-deletable, add its `HasQueryFilter(x => !x.IsDeleted)` line in `OnModelCreating` — it will NOT pick this up automatically, (4) generate a migration (see root `AGENTS.md` for the `dotnet ef` command).
- `DemoDataGenerator` runs only on an empty `Forms` table (see `DbInitializer`), so re-running it after any manual data entry is a no-op by design — to force a re-seed in dev, truncate/drop the DB (or at least the `forms` table) first.
- `Repository<T>` intentionally has no LINQ-filtering surface; do not add ad-hoc query methods to it — that pattern is deliberately kept out of this generic class so query logic stays in `Application` handlers against `IApplicationDbContext`.

## Dependencies
### Internal
- `Application.Abstractions.Data` (`IApplicationDbContext`, `IRepository<T>`), `Application.Abstractions.Authentication.IPasswordHasher` (used by `DbInitializer`).
- `Domain.College.*`, `Domain.Questionnaires.*`, `Domain.User` — entities mapped/seeded.
- `SharedKernel` (`Entity` base class constraint on `Repository<T>`, `Result<T>` used by `DemoDataGenerator`).

### External
- `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Metadata` — DbContext/model APIs.
- `System.Security.Cryptography` (`RandomNumberGenerator`) — used by `DemoDataGenerator` for reproducible-looking but cryptographically sourced randomness instead of `System.Random`.

<!-- MANUAL: -->
