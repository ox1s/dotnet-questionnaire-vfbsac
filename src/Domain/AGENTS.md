<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Domain

## Purpose
The active domain layer of the questionnaire/evaluation platform. Contains all entities, value objects, enums, and error catalogs for the college structure (departments, disciplines, specialities, specializations, teachers), the questionnaire lifecycle (forms/questions and submissions/answers), and user accounts. It has zero framework dependencies (no EF Core, no ASP.NET, no MediatR) — every rule here is pure C#, referenced by `Application` (see `../Application/AGENTS.md`) and, transitively, by `Infrastructure` and `Web.Api`. It also carries the assembly's localization resources.

## Key Files
| File | Description |
|------|-------------|
| `Domain.csproj` | Project file. References `SharedKernel` only and the `Throw` NuGet package (guard-clause helper used by a few mutation methods). Wires `Resources/DomainErrors.resx` to its generated `.Designer.cs`. |
| `Properties/AssemblyInfo.cs` | Declares `[assembly: NeutralResourcesLanguage("ru")]` — the neutral/fallback culture for `Resources.DomainErrors` is Russian, not English. |
| `Resources/DomainErrors.resx` + `Resources/DomainErrors.Designer.cs` | Auto-generated resource class `Resources.DomainErrors`. Every `*Errors` static class across the layer pulls its human-readable message text from this resource (e.g. `Resources.DomainErrors.Departments_NotFound`), then wraps it in a `SharedKernel.Error` with a machine-readable code (e.g. `"Departments.NotFound"`). Edit the `.resx`, not the `.Designer.cs`, when changing message text. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `College/` | Reference/master data for the college org structure: Departments, Disciplines, Specialities, Specializations, Teachers. See `College/AGENTS.md`. |
| `Questionnaires/` | The core domain: `Forms/` (Form + Question templates) and `Submissions/` (Submission + Answer instances). See `Questionnaires/AGENTS.md`. |
| `User/` | `User` entity (staff/admin/group accounts) plus `Login` and `GroupName` value objects, `UserRole` enum, and `UserErrors`. Small enough to not warrant its own AGENTS.md — see notes below. |
| `Resources/` | Localization resource (`DomainErrors.resx`) backing every error catalog in the layer. No standalone AGENTS.md — see Key Files above. |
| `Properties/` | Just `AssemblyInfo.cs` (neutral resource language attribute). No standalone AGENTS.md. |

### `User/` at a glance
- `User.cs` — `sealed class User : Entity, ISoftDeletable`. No public constructor; created only via three static factories: `CreateGroupUser(GroupName, Guid groupId, string passwordHash)` (role `StudentGroup`, sets `GroupId`), `CreateStaff(Login, fullName, teacherId?, departmentId?, passwordHash, role = Staff)` (validates non-blank `fullName`), and `CreateAdmin(Login, passwordHash)` (role `Admin`). Mutators: `UpdateDetails`, `SetPasswordByAdmin`, `ChangePassword`, `SetDepartment` (uses `Throw`'s `ThrowIfNull()` on the `Guid`, so passing `Guid.Empty` throws rather than returning a `Result`).
- `Login.cs` / `GroupName.cs` — `sealed record` value objects with a `static Result<T> Create(string)` factory. `GroupName` additionally enforces an exact length of 5 characters (`UserErrors.GroupNameInvalid()` otherwise).
- `UserRole.cs` — plain enum: `Admin = 1, StudentGroup = 3, Staff = 4, Employer = 5`.
- `UserErrors.cs` — `NotFound`, `NotFoundByLogin`, `Unauthorized`, `InvalidResetToken`, `ExpiredResetToken`, `UserExist`, `GroupExists`, `GroupNameInvalid`.

## For AI Agents
### Working In This Directory
- **Repeated entity shape**: every mutable entity (`Department`, `Discipline`, `Speciality`, `Specialization`, `Teacher`, `Question`, `Form`, `Answer`, `Submission`, `User`) follows the same pattern: `private` parameterless ctor annotated `// EF Core`, a `private` ctor taking `Guid id` + fields (calls `base(id)`), and a `public static Result<T> Create(...)` factory that validates input (usually `string.IsNullOrWhiteSpace` → `Error.NullValue`) before constructing. Mutation methods are named `UpdateX`/`ChangeX`/`SetX` and mostly return `Result` (validated) rather than `void` — `SetDepartment`/`ChangeDepartment`/`ChangeSpeciality` are the exception, using `Throw`'s `.ThrowIfNull()` on a `Guid` instead of a `Result`, so a bad `Guid.Empty` throws an exception there rather than failing gracefully. Match whichever pattern the sibling methods on that entity already use.
- **Errors**: every aggregate has a matching `static class {Name}Errors` (e.g. `DepartmentErrors`, `FormErrors`) whose methods build a `SharedKernel.Error` via `Error.NotFound` / `Error.Conflict` / `Error.Failure` / `Error.Validation`, with the message pulled from `Resources.DomainErrors`. When adding a new failure case, add the resource string to `Resources/DomainErrors.resx` first, then add the wrapping method to the errors class — don't inline literal strings for anything user-facing.
- **Soft delete**: most entities implement `SharedKernel.ISoftDeletable` (`bool IsDeleted { get; set; }`). `Submission` is the one exception in this layer — it has its own `IsDeleted` property but does **not** implement `ISoftDeletable` on the class declaration; check before assuming polymorphic soft-delete handling applies to it.
- **No domain events**: `SharedKernel.IDomainEvent` exists but nothing in `Domain/` raises or references it currently — entities don't queue events on state changes. Don't assume an event will fire on `Create`/`Update`/etc. unless you add that plumbing yourself.
- **String normalization**: any `Create`/`Update` that takes a display string trims it (`name.Trim()`) and typically upper-invariants identifiers (`Login`/`GroupName` in some paths). Keep this convention when adding similar value objects.
- **Aggregate boundaries**: `Form` owns its `Question`s (`_questions` private list, exposed as `IReadOnlyList<Question>`, added only via `Form.AddQuestion`, guarding duplicate `Order`). `Submission` owns its `Answer`s the same way via `AddAnswer`, guarding one answer per `QuestionId`. Don't construct `Question`/`Answer` directly from outside code expecting them to attach themselves to the parent — always go through the aggregate's method.

## Dependencies
### Internal
`SharedKernel` (see `../SharedKernel/AGENTS.md`) supplies `Entity` (Guid `Id`, protected empty ctor + `SetIdForSeeding`), `Result`/`Result<T>`, `Error`/`ErrorType`, and `ISoftDeletable`. This is the only in-repo project reference.

### External
`Throw` (NuGet) — used for a handful of guard clauses (`Guid.ThrowIfNull()`) in `Discipline.ChangeDepartment`, `Specialization.ChangeSpeciality`, `User.SetDepartment`.

<!-- MANUAL: -->
