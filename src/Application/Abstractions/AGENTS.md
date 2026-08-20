<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Abstractions

## Purpose
Cross-cutting contracts and pipeline behaviors shared by every vertical slice in `Application`. Nothing here contains business logic for a specific feature — it defines the seams the rest of the layer is built on: the CQRS marker/handler interfaces (`Messaging/`), the two decorators every handler is wrapped in (`Behaviors/`), the persistence contract implemented by `Infrastructure` (`Data/`), auth-related contracts (`Authentication/`), a generic cache contract (`Caching/`), and the report-document generation contract (`Reports/`). All interfaces are `public`; the decorator implementations are `internal`.

## Key Files
| File | Description |
|------|--------------|
| `Messaging/ICommand.cs` | `public interface ICommand;` and `public interface ICommand<TResponse>;` — empty marker interfaces distinguishing "fire-and-forget" commands from commands with a return value. |
| `Messaging/IQuery.cs` | `public interface IQuery<TResponse>;` — marker interface for queries; queries always return a value. |
| `Messaging/ICommandHandler.cs` | `ICommandHandler<in TCommand> { Task<Result> Handle(...) }` and `ICommandHandler<in TCommand, TResponse> { Task<Result<TResponse>> Handle(...) }`. |
| `Messaging/IQueryHandler.cs` | `IQueryHandler<in TQuery, TResponse> { Task<Result<TResponse>> Handle(...) }`. |
| `Behaviors/ValidationDecorator.cs` | `internal static class ValidationDecorator` containing `CommandHandler<TCommand,TResponse>` and `CommandBaseHandler<TCommand>` decorator classes. Runs every registered `IValidator<TCommand>` in parallel (`Task.WhenAll`), collects `ValidationFailure`s, and short-circuits to `Result.Failure(new ValidationError(...))` (mapping each failure's `ErrorCode`/`ErrorMessage` to `Error.Problem`) without calling the inner handler if any validator fails. No-op (calls straight through) if there are zero registered validators for that command type — queries are never wrapped by this decorator. |
| `Behaviors/LoggingDecorator.cs` | `internal static class LoggingDecorator` containing `CommandHandler<TCommand,TResponse>`, `CommandBaseHandler<TCommand>`, and `QueryHandler<TQuery,TResponse>`. Logs `Information` "Processing {Command/Query} {Name}" before calling the inner handler, `Information` "Completed ..." on `result.IsSuccess`, or `Error`-level "Completed ... with error" inside a `logger.BeginScope` carrying `{"Error": result.Error}` on failure. Uses `typeof(TCommand/TQuery).Name` as the logged name — not the specific request instance. |
| `Data/IApplicationDbContext.cs` | The persistence contract every handler in the layer depends on instead of a concrete `DbContext`. Exposes `DbSet<User> Users`, `DbSet<Form> Forms`, `DbSet<Submission> Submissions`, `DbSet<Department> Departments`, `DbSet<Discipline> Disciplines`, `DbSet<Teacher> Teachers`, `DbSet<Question> Questions`, `DbSet<Speciality> Specialities`, `DbSet<Specialization> Specializations`, `DbSet<Answer> Answers`, a generic `Set<TEntity>()`, and `Task<int> SaveChangesAsync(...)`. Implemented by the real EF Core `DbContext` in `Infrastructure`. |
| `Data/IRepository.cs` | `IRepository<T> where T : Entity` — `GetByIdAsync`, `GetAllAsync`, `Add`, `Update`, `Remove`. Defined but **not used by any handler** currently in this layer; all slices go through `IApplicationDbContext` directly. Keep this in mind before assuming a new slice should use it — check current conventions in the feature folder you're extending first. |
| `Authentication/IUserContext.cs` | `IUserContext { Guid UserId; UserRole Role }` — contract for reading the current authenticated user's id and role (implemented in `Infrastructure`/`Web.Api` from the HTTP context / claims — `Role` reads the JWT's `"role"` claim, the same one `TokenProvider` embeds). Handlers that need the acting user's id still mostly take `UserId` as an explicit command parameter instead (e.g. `CreateSubmissionCommand.UserId`); `Role` is injected directly at the endpoint level where needed (e.g. `Web.Api/Endpoints/Forms/GetList.cs` passes it into `GetFormsQuery.CallerRole`). |
| `Authentication/IPasswordHasher.cs` | `Hash(string password) : string`, `Verify(string password, string passwordHash) : bool`. Used by `Users/SignIn`, `Users/CreateGroup`, `Users/CreateStaff`, `Users/AdminSetPassword`. |
| `Authentication/ITokenProvider.cs` | `Create(User user) : string` — issues the auth token on successful login (`Users/SignIn/LoginUserCommandHandler`). |
| `Caching/ICacheService.cs` | Generic async cache contract: `GetAsync<T>`, `SetAsync<T>(key, value, expiration?)`, `RemoveAsync(key)`, `RemoveByPatternAsync(pattern)`, all `where T : class`. Not currently called from any handler found in `Application` — present for `Infrastructure` to implement and for future slices to opt into (e.g., caching `GetList`/`GetById` reads). |
| `Reports/IReportGenerator.cs` | Document-generation contract used only by `Application.Reports.Commands.Export*` handlers: `GeneratePeriodReportAsync(formTitle, periodStart, periodEnd, resolvedFilters, List<GetAnalyticsByPeriodQueryResponse>, ct) : Task<byte[]>`, `GeneratePeriodsComparisonReportAsync(formTitle, List<GetAnalyticsByPeriodsQueryResponse>, ct) : Task<byte[]>`, `GenerateGroupsComparisonReportAsync(formTitle, List<GetAnalyticsByGroupsQueryResponse>, ct) : Task<byte[]>`. Note the interface takes concrete response DTOs from `Application.Reports.Queries.*` as parameters — it's coupled to those shapes by design so `Infrastructure`'s document generator can render them directly. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Authentication/` | `IUserContext`, `IPasswordHasher`, `ITokenProvider` — auth-adjacent contracts implemented by `Infrastructure`. |
| `Behaviors/` | `ValidationDecorator`, `LoggingDecorator` — the two decorators wrapped around every handler in `DependencyInjection.cs`. |
| `Caching/` | `ICacheService` — generic cache contract, currently unused by handlers. |
| `Data/` | `IApplicationDbContext` (actively used by every handler) and `IRepository<T>` (defined, not used). |
| `Messaging/` | `ICommand`, `ICommand<T>`, `IQuery<T>`, `ICommandHandler<>`, `ICommandHandler<,>`, `IQueryHandler<,>` — the entire CQRS vocabulary of this codebase. |
| `Reports/` | `IReportGenerator` — document rendering contract for the analytics export commands. |

## For AI Agents
### Working In This Directory
- This folder defines **contracts only** — no implementations except the two `internal` decorators. New interfaces added here must be implemented in `Infrastructure` (check `src/Infrastructure/AGENTS.md`) before anything can resolve them at runtime; `DependencyInjection.cs` in this project only registers handlers/validators/decorators, not these interfaces themselves.
- If you add a new pipeline behavior (a third decorator), wire it in `Application/DependencyInjection.cs` using `services.Decorate(...)` — decorator order matters: whatever is registered as the *outermost* `Decorate` call runs first around the caller-facing interface, and each subsequent `Decorate` wraps the previous decorator. Currently: `ValidationDecorator` is registered before `LoggingDecorator`, meaning validation happens before logging wraps it (so a validation failure still gets logged as an error via `LoggingDecorator` on the outside).
- `ICommand`/`ICommand<TResponse>`/`IQuery<TResponse>` are intentionally empty marker interfaces (this codebase does not use MediatR — it's a hand-rolled minimal CQRS pipeline with Scrutor for registration). Don't add members to them; add behavior via a new decorator instead.
- Before adding a new cross-cutting interface, check whether one already covers the need — e.g. don't reintroduce a repository pattern per-aggregate; `IApplicationDbContext` is the established seam and `IRepository<T>` is dead code kept for reference/tests.
- `IReportGenerator`'s methods intentionally take the `Application.Reports.Queries.*` response DTOs directly rather than a generic document model — if you add a new report type, follow that pattern (add a new method taking your query's response DTO) rather than trying to generalize the interface.

## Dependencies
### Internal
- `Domain` — `Data/IApplicationDbContext.cs` references `Domain.College.*`, `Domain.Questionnaires.Forms`, `Domain.Questionnaires.Submissions`, `Domain.User`; `Authentication/ITokenProvider.cs` references `Domain.User.User`; `Data/IRepository.cs` constrains `T : SharedKernel.Entity`.
- `Application.Reports.Queries.*` — `Reports/IReportGenerator.cs` depends "upward" on sibling feature-folder response DTOs (an intentional exception to strict layering within this project).
- `Application.Abstractions.Messaging` — `Behaviors/*.cs` depend on the `ICommand`/`IQuery`/handler interfaces defined in `Messaging/`.

### External
- `SharedKernel` — `Result`, `Result<T>`, `Error`, `ValidationError`, `Entity`.
- FluentValidation — `Behaviors/ValidationDecorator.cs` (`IValidator<TCommand>`, `ValidationContext<TCommand>`, `ValidationResult`, `ValidationFailure`).
- Microsoft.Extensions.Logging(.Abstractions) — `Behaviors/LoggingDecorator.cs` (`ILogger<T>`, `BeginScope`).
- Microsoft.EntityFrameworkCore — `Data/IApplicationDbContext.cs` (`DbSet<T>`).

<!-- MANUAL: -->
