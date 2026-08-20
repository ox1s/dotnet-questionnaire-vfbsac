<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# SharedKernel

## Purpose
Tiny, dependency-free class library of base types shared by `Domain`, `Application`, `Infrastructure`, and `Web.Api` in the active stack. It has no framework dependencies (plain `Microsoft.NET.Sdk`, no package references) and defines only cross-cutting primitives: the `Result`/`Error` pattern used instead of exceptions for expected failures, a base `Entity`, a couple of small marker interfaces, and the app's permission constants.

## Key Files
| File | Description |
|------|-------------|
| `Entity.cs` | Abstract base class for domain entities. Holds `Guid Id`; protected parameterless ctor exists only for EF Core, plus `SetIdForSeeding(Guid)` for deterministic seed data. |
| `Result.cs` | `Result`/`Result<TValue>` — the success/failure wrapper used pervasively instead of throwing. `Result<TValue>` has an implicit operator from `TValue` via `Create`, which maps `null` to `Error.NullValue`. Ctor throws `ArgumentException` if success/error state is inconsistent (success with a real error, or failure with `Error.None`). |
| `Error.cs` | `record Error(Code, Description, Type)` with static factories per `ErrorType` (`Failure`, `NotFound`, `Problem`, `Validation`, `Conflict`) plus sentinel `Error.None` / `Error.NullValue`. |
| `ErrorType.cs` | Enum backing `Error.Type`: `Failure`, `Validation`, `Problem`, `NotFound`, `Conflict`. Likely mapped to HTTP status codes somewhere in `Web.Api`. |
| `ValidationError.cs` | `sealed record ValidationError : Error` — aggregates multiple `Error`s (e.g. from FluentValidation or multiple failed `Result`s) into one `Error` via `FromResults(IEnumerable<Result>)`. |
| `IDomainEvent.cs` | Empty marker interface (`public interface IDomainEvent;`) for domain events raised by entities. |
| `IDateTimeProvider.cs` | Abstraction over `DateTime.UtcNow` for testable time-dependent logic; implemented in `Infrastructure`. |
| `ISoftDeletable.cs` | Marker interface (`bool IsDeleted { get; set; }`) for entities supporting soft delete, consumed by EF Core query filters in `Infrastructure`. |
| `Permissions.cs` | `static class Permissions` — string constants for policy-based authorization (`admin:access`, `users:access`, `dictionaries:write`, `reports:view`, `forms:submit`), referenced by `[Authorize(Policy = ...)]` in `Web.Api` and permission-seeding code. |

## For AI Agents
### Working In This Directory
- Keep this project dependency-free. It must not reference `Domain`, `Application`, `Infrastructure`, `Web.Api`, EF Core, ASP.NET Core, or any other package — it sits below every layer and is referenced by all of them. Adding a dependency here risks a circular reference or forces an unwanted package onto every layer.
- `Result`/`Error` is the established error-handling convention in this codebase — prefer returning `Result`/`Result<T>` over throwing for expected/business failures; reserve exceptions for truly exceptional cases.
- When adding a new permission, add the constant to `Permissions.cs` rather than hardcoding the string elsewhere (policy names, seed data, and `[Authorize]` attributes should all reference this class).
- `Entity`'s parameterless protected constructor exists for EF Core materialization — don't remove it even though nothing in this project calls it directly.
- `obj/` and `bin/` contain build-generated files (`SharedKernel.GlobalUsings.g.cs`, `SharedKernel.AssemblyInfo.cs`, etc.) — never hand-edit them, they regenerate on every build.

## Dependencies
### Internal
None — this is the lowest-level project in the active stack; everything else depends on it, it depends on nothing internal.

### External
None (plain `Microsoft.NET.Sdk`, no `PackageReference`s) — deliberately kept framework-free.

<!-- MANUAL: -->
