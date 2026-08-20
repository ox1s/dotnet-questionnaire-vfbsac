<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Application

## Purpose
This is the active application layer of the questionnaire/evaluation platform, implementing CQRS with vertical-slice organization: every use case is a folder containing a `Command`/`Query` record, its `Handler`, and (where input needs rules beyond domain invariants) a FluentValidation `Validator`. The layer depends only on `Domain` and `SharedKernel`, defines the abstraction contracts that `Infrastructure` implements (`IApplicationDbContext`, `IPasswordHasher`, `ITokenProvider`, `ICacheService`, `IReportGenerator`, etc.), and is consumed by `Web.Api` endpoints, which resolve `ICommandHandler<>`/`IQueryHandler<,>` from DI and call `Handle`. Handlers talk to the database directly through `IApplicationDbContext` (EF Core) rather than a repository-per-aggregate pattern (`IRepository<T>` exists in `Abstractions/Data` but is not used by any handler found in this layer). All handlers return `SharedKernel.Result`/`Result<T>` — no exceptions for expected failure paths.

## Key Files
| File | Description |
|------|--------------|
| `Application.csproj` | Class library targeting the layer; references `Domain` and `SharedKernel`; packages: Dapper, FluentValidation.DependencyInjectionExtensions, Microsoft.EntityFrameworkCore, Microsoft.Extensions.Configuration, Microsoft.Extensions.Logging.Abstractions, Scrutor. `InternalsVisibleTo Application.UnitTests`. `NeutralLanguage` is `ru` (validation/error messages are frequently in Russian). |
| `DependencyInjection.cs` | `AddApplication()` extension: uses **Scrutor** (`services.Scan`) to auto-register every `IQueryHandler<,>`, `ICommandHandler<>`, and `ICommandHandler<,>` implementation as scoped, then wraps them with decorators (see below), and registers all `FluentValidation` validators in the assembly. |

## Pipeline Behaviors (registered in `DependencyInjection.cs`)
Every command/query handler is wrapped, in this order (validation runs first, logging wraps the validated call):
1. **`ValidationDecorator`** (`Abstractions/Behaviors/ValidationDecorator.cs`) — runs all `IValidator<TCommand>` for the command; on failure returns `Result.Failure` with a `ValidationError` built from the FluentValidation failures (never calls the inner handler). Only applies to commands, not queries.
2. **`LoggingDecorator`** (`Abstractions/Behaviors/LoggingDecorator.cs`) — logs `"Processing {Command/Query} {Name}"` before, `"Completed ... {Name}"` on success, or an error-level log with the `Result.Error` in a log scope on failure. Applies to commands and queries.

See `Abstractions/AGENTS.md` for full detail on these and the other cross-cutting contracts (`Authentication/`, `Caching/`, `Data/`, `Messaging/`, `Reports/`).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Abstractions/` | Cross-cutting interfaces and pipeline behaviors (`IApplicationDbContext`, `IUserContext`, `IPasswordHasher`, `ITokenProvider`, `ICacheService`, `IReportGenerator`, `ICommand(Handler)`/`IQuery(Handler)`, `ValidationDecorator`, `LoggingDecorator`). See its own `AGENTS.md`. |
| `Departments/`, `Disciplines/`, `Specialities/`, `Specializations/`, `Teachers/` | Near-identical CRUD vertical slices for college reference data. Summarized in the table below rather than documented per-folder. |
| `Forms/` | Questionnaire form definitions (title, required context filters, ordered questions). `Create/Delete/GetById/GetList`. |
| `Submissions/` | Student/staff answers to a form, carrying a denormalized `SubmissionContext` (department/discipline/teacher/speciality/specialization/org/education-form/employee-category/position) used for analytics filtering. `Create/Delete/GetList`. `GetStatistics/` exists as an empty placeholder folder (no files). |
| `Reports/` | Analytics queries (per-period, multi-period comparison, per-group comparison, free-text answer extraction) and export commands that render those analytics to a document via `IReportGenerator`. Has real complexity — see its own `AGENTS.md`. |
| `Users/` | Accounts: student-group logins, staff/admin accounts, sign-in, password administration, semester open/close, group listing. |
| `Resources/` | `ApplicationErrors.resx` / `.Designer.cs` — localized validation message strings (`NotEmpty`, `WithReference`), consumed by validators via `Resources.ApplicationErrors.NotEmpty` etc. |

## CRUD Reference-Data Slices (Departments / Disciplines / Specialities / Specializations / Teachers)
All five follow the identical shape: `Create/`, `Delete/`, `GetList/`, `Restore/`, `Update/`, each with a `{Verb}{Entity}Command(Query)` + `{Verb}{Entity}CommandHandler`, injecting only `IApplicationDbContext`. Soft delete is via an `IsDeleted` bool with EF Core global query filters; handlers that need deleted rows use `.IgnoreQueryFilters()`. `GetList` always uses `IgnoreQueryFilters()` + `AsNoTracking()` and returns both active and deleted rows (ordered `IsDeleted` then name) so the UI can show/restore soft-deleted items. `Update`/`Create` check for duplicate names via a manual query (not a DB unique constraint) before calling the domain method. Errors come from static `{Entity}Errors` classes in `Domain`.

| Feature | Create | Delete | Update | Notable differences |
|---------|--------|--------|--------|----------------------|
| `Departments/` | `CreateDepartmentCommand(Name)` — duplicate-name check | Blocks if it has `Disciplines` or `Users`; if only referenced by `Submissions`, soft-deletes instead of hard-deleting | `UpdateDepartmentCommand(DepartmentId, Name)` — duplicate check excludes self (marked `// TODO: Implement this update with same name for others`) | No validator class (validation is inline duplicate/not-found checks only) |
| `Disciplines/` | `CreateDisciplineCommand(Name, DepartmentId)` — validates `DepartmentId` exists, duplicate name | Hard-deletes unless referenced by a `Submission.Context.DisciplineId`, then soft-deletes | `UpdateDisciplineCommand(DisciplineId, Name, DepartmentId)` | Has `CreateDisciplineCommandValidator` (Name: NotEmpty/MaxLength 255; DepartmentId: NotEmpty) |
| `Specialities/` | `CreateSpecialityCommand(Name)` | Blocks if it has `Specializations`; else hard/soft-delete based on `Submissions` usage | `UpdateSpecialityCommand(SpecialityId, Name)` | No validator class |
| `Specializations/` | `CreateSpecializationCommand(Name, SpecialityId)` | Hard/soft-delete based on `Submissions` usage (`Context.SpecializationId`) | `UpdateSpecializationCommand(SpecializationId, Name, SpecialityId)` — duplicate-name check, then re-validates `SpecialityId` exists only if it changed, calls `specialization.ChangeSpeciality(...)` | No validator class |
| `Teachers/` | `CreateTeacherCommand(FullName, DepartmentId?)` — `DepartmentId` is optional; validated only if present | Blocks if it has `Users`; else hard/soft-delete based on `Submissions` usage | `UpdateTeacherCommand(TeacherId, FullName, DepartmentId?)` — re-validates department only if it changed, calls `teacher.SetDepartment(...)` | Has `CreateTeacherCommandValidator` (FullName: NotEmpty/MaxLength 255, custom Russian message) |

`Restore/` is identical across all five: load with `IgnoreQueryFilters()`, 404 via `{Entity}Errors.NotFound`, set `IsDeleted = false`, save.

## Forms
| Slice | Behavior |
|-------|----------|
| `Create/CreateFormCommand(Title, RequiredFilters?, Questions?)` | Builds the `Form` aggregate via `Form.Create`, then adds each `QuestionRequest(Text, Type, Order)` through `form.AddQuestion(...)`. Validator: `Title` NotEmpty/MaxLength 500; each question validated by `QuestionRequestValidator` (Text NotEmpty/MaxLength 2000, Order >= 0). |
| `Delete/DeleteFormCommand(FormId)` | Soft-delete only (`form.IsDeleted = true`); no hard-delete path. |
| `GetById/GetFormByIdQuery(FormId)` | Projects `Form` + ordered `Questions` into `GetFormByIdQueryResponse`/`QuestionResponse`. 404 via `FormErrors.NotFound`. |
| `GetList/GetFormsQuery(IsActive?)` | Optional `IsActive` filter, ordered by `Title`. No pagination. |

Forms are toggled active/inactive in bulk by `Users/OpenSemester` and `Users/CloseSemester` (see below), not by a per-form command in this folder.

## Submissions
| Slice | Behavior |
|-------|----------|
| `Create/CreateSubmissionCommand` | Fields: `FormId, DeviceId, UserId, Answers, DisciplineId?, TeacherId?, DepartmentId?, SpecialityId?, SpecializationId?, OrganizationName?, EducationForm?, EmployeeCategory?, Position?`. Handler: loads `Form` with `Questions`, rejects if missing/inactive/deleted; rejects duplicate submission (same `FormId`+`UserId`+`DeviceId`, further narrowed by `TeacherId`/`DisciplineId` if given) via `SubmissionErrors.AlreadySubmitted()`; builds `Submission` via `Submission.Create(...)` (injects `IDateTimeProvider.UtcNow` for `SubmittedAt`), then overlays `EducationForm/EmployeeCategory/Position` onto `submission.Context` with a `with` expression; validates and adds each `AnswerRequest` per `QuestionType` (`Text` needs `Value` only, `Number` needs `NumericValue` only, `WeightedRating` needs both `NumericValue` and `Weight`, `MultipleChoice`/`SingleChoice` need `Value` only) via a private `ValidateAnswerForQuestionType` switch before calling `submission.AddAnswer(...)`. Validator (`CreateSubmissionCommandValidator`) only checks `FormId`/`UserId` NotEmpty and `Answers` NotEmpty+each validated by `AnswerRequestValidator` (QuestionId NotEmpty; must have `Value` or `NumericValue`; `NumericValue`/`Weight` InclusiveBetween 1–10 when present). |
| `Delete/DeleteSubmissionCommand(SubmissionId)` | Soft-delete only. |
| `GetList/GetSubmissionsQuery` | Every field optional filter (`FormId, DeviceId, UserId, DisciplineId, TeacherId, DepartmentId, SpecialityId, SpecializationId, OrganizationName` (Contains), `SubmittedFrom/To`); includes `Answers`, ordered by `SubmittedAt` descending. |
| `GetStatistics/` | Empty directory — no implementation yet. |

## Users
| Slice | Behavior |
|-------|----------|
| `SignIn/LoginUserCommand(Login, Password)` | Looks up by `Login` value object, verifies via `IPasswordHasher.Verify`, issues a JWT-like token string via `ITokenProvider.Create(user)`. Both "no such login" and "bad password" map to the same `UserErrors.NotFoundByLogin` (comment notes this is deliberate to avoid leaking which case failed). |
| `CreateGroup/CreateGroupUserCommand(GroupName, Password)` | Validates `GroupName` value object, uses its normalized value as the `Login`; rejects duplicates via `UserErrors.GroupExists`; creates via `User.CreateGroupUser(...)`. |
| `CreateStaff/CreateStaffUserCommand(Login, FullName, Password, DepartmentId?, UserRole Role)` | Validates `Login`; rejects duplicates via `UserErrors.UserExist()`; creates via `User.CreateStaff(...)` with `teacherId: null`. Validator restricts `Role` to `Staff` or `DeputyHead` only. |
| `AdminSetPassword/AdminSetPasswordCommand(UserId, NewPassword)` | Admin-forced password reset, bypassing normal change-password rules; calls `user.SetPasswordByAdmin(hash)`. |
| `Update/UpdateUserCommand(UserId, Login, DisplayName)` | Re-validates `Login`, checks for a conflicting login only if it actually changed, calls `user.UpdateDetails(...)`. |
| `Delete/DeleteUserCommand(UserId)` | Soft-delete only. |
| `GetById/GetUserByIdQuery(UserId)` | Projects `Id, Login.Value, DisplayName`. |
| `GetGroups/GetGroupsQuery()` | Lists users with `Role == UserRole.StudentGroup`, ordered by login. |
| `OpenSemester/OpenSemesterCommand()` / `CloseSemester/CloseSemesterCommand()` | Bulk-flip every `Form.IsActive` (`ExecuteUpdateAsync`, no per-row loading) to `true`/`false` respectively — this is how forms get activated/deactivated for a semester, not a per-form command. |

## For AI Agents
### Working In This Directory
When adding a new use case, follow the existing slice shape exactly:
1. Create a folder under the feature area named after the operation (`Create/`, `Update/`, `GetList/`, etc., or a bespoke verb like `OpenSemester/` for non-CRUD actions).
2. Add a `sealed record {Verb}{Entity}Command(...) : ICommand<TResponse>` (or `: ICommand` for no return value) or `IQuery<TResponse>` — these are marker interfaces with no members (`Abstractions/Messaging/ICommand.cs`, `IQuery.cs`).
3. Add an `internal sealed class {Verb}{Entity}CommandHandler(IApplicationDbContext context, ...) : ICommandHandler<TCommand, TResponse>` (or `ICommandHandler<TCommand>` / `IQueryHandler<TQuery, TResponse>`). Constructor-inject only what's needed (`IApplicationDbContext`, `IPasswordHasher`, `ITokenProvider`, `IDateTimeProvider` from `SharedKernel`, `IReportGenerator`, or another `IQueryHandler<,>`/`ICommandHandler<,>` if composing — see `Reports/Commands/*` which call an analytics query handler directly). **Do not register anything manually** — Scrutor's assembly scan in `DependencyInjection.cs` picks up any class implementing the handler interfaces automatically, including `internal` ones (`publicOnly: false`).
4. Return `Result`/`Result<T>` from `SharedKernel`; never throw for expected/business failures. Use `Result.Failure(...)`/`Result.Failure<T>(...)` with an `Error` from the aggregate's static `{Entity}Errors` class (defined in `Domain`, not here).
5. If the command carries input worth rejecting before it reaches the domain (empty strings, length limits, "must supply A or B", numeric ranges), add a `{Verb}{Entity}CommandValidator : AbstractValidator<TCommand>` in the same folder — FluentValidation validators are auto-registered by `services.AddValidatorsFromAssembly(...)` and automatically run by `ValidationDecorator` before the handler executes. Not every slice needs one (e.g., `Departments` has none); simple `NotFound`/duplicate checks that depend on DB state belong in the handler, not the validator, since validators only see the command's own data.
6. For entities with soft delete (`IsDeleted` bool + EF global query filter): default queries silently exclude soft-deleted rows; use `.IgnoreQueryFilters()` in `Delete`/`Restore` handlers (and in `GetList` when the UI needs to show deleted items) to reach them. Prefer soft-delete-if-referenced, hard-delete-if-orphan (see the CRUD table above) when the entity is referenced by `Submissions` or another aggregate.
7. Keep query handlers read-only: `AsNoTracking()` + `.Select(...)` projections into a dedicated `{Query}Response` record/class in the same folder, never returning domain entities directly across the boundary.
8. Reuse `Application.Reports.Queries.Shared` (`AnalyticsFilterSet`, `SubmissionFilterHelper.ApplyFilters`, `EntityNameResolver`, `StatisticsCalculator`, `QuestionStatistics`) instead of duplicating filter/statistics logic if adding another analytics slice.

## Dependencies
### Internal
- `Domain` — aggregates (`User`, `Form`, `Submission`, `Department`, `Discipline`, `Teacher`, `Speciality`, `Specialization`, `Question`, `Answer`), their `{Entity}.Create(...)`/mutation methods, and static `{Entity}Errors` classes.
- `SharedKernel` — `Result`/`Result<T>`, `Error`, `Entity` base class, `IDateTimeProvider`.

### External
- FluentValidation (+ `FluentValidation.DependencyInjectionExtensions`) — command/query input validation, auto-registered and auto-run.
- Scrutor — assembly scanning (`services.Scan`) for handler registration and decorator wrapping (`services.Decorate`).
- Microsoft.EntityFrameworkCore — all data access is via `IApplicationDbContext` (`DbSet<T>`, `IgnoreQueryFilters`, `ExecuteUpdateAsync`, LINQ projections); no repository pattern in active use despite `IRepository<T>` existing in `Abstractions/Data`.
- Dapper, Microsoft.Extensions.Configuration, Microsoft.Extensions.Logging.Abstractions — referenced at the project level; logging abstractions are used directly by `LoggingDecorator` and the `Reports/Commands/*` handlers (`[LoggerMessage]` source-generated logging).

<!-- MANUAL: -->
