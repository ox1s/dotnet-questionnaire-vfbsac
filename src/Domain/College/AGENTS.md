<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# College

## Purpose
Reference/master data for the college's organizational structure that other aggregates (`User`, `Questionnaires`) hang off of: which department a teacher or discipline belongs to, which speciality a specialization belongs to, and the pool of teachers users can be linked to. All five entities here are small, soft-deletable, name-bearing aggregates with no children of their own except the parent/child relationships noted below.

## Key Files
| Subfolder | Entity | Key Properties | Invariants / Notable Behavior | Errors (`{Entity}Errors`) |
|-----------|--------|-----------------|-------------------------------|----------------------------|
| `Departments/` | `Department` | `Name` | `Create`/`UpdateName` reject blank names (`Error.NullValue`); name is trimmed. | `NotFound`, `Duplicate`, `HasTeachers()`, `HasDisciplines()`, `HasUsers()` — conflict guards used before allowing delete of a department that still has dependents. |
| `Disciplines/` | `Discipline` | `Name`, `DepartmentId` | Belongs to exactly one `Department` (non-nullable FK). `ChangeDepartment(Guid)` uses `Throw`'s `ThrowIfNull()` — passing `Guid.Empty` throws instead of returning a `Result`. | `NotFound`, `DepartmentDeleted(departmentId)`, `Duplicate(name)`. |
| `Specialities/` | `Speciality` | `Name` | Same shape as `Department` (no FK). | `NotFound`, `HasSpecializations()`, `Duplicate(name)`. |
| `Specializations/` | `Specialization` | `Name`, `SpecialityId` | Belongs to exactly one `Speciality`. `Create` does **not** validate blank `name` (unlike its siblings — calls `name.Trim()` directly with no null/whitespace check, so an empty string will throw `NullReferenceException`/`ArgumentException` rather than a `Result` failure; worth hardening if touched). `ChangeSpeciality(Guid)` also uses `ThrowIfNull()`. | `NotFound`, `SpecialityDeleted(specialityId)`, `Duplicate(name)`. |
| `Teachers/` | `Teacher` | `FullName`, `DepartmentId` (nullable) | Department assignment is optional (`Guid? departmentId = null` in `Create`); `SetDepartment(Guid?)` can clear it back to `null`. | `NotFound`, `HasUsers()`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Departments/` | `Department` aggregate + `DepartmentErrors`. |
| `Disciplines/` | `Discipline` aggregate (child of `Department`) + `DisciplineErrors`. |
| `Specialities/` | `Speciality` aggregate + `SpecialityErrors`. |
| `Specializations/` | `Specialization` aggregate (child of `Speciality`) + `SpecializationErrors`. |
| `Teachers/` | `Teacher` aggregate (optionally linked to `Department`) + `TeacherErrors`. |

Each of these five folders is a two-file pair (`{Entity}.cs`, `{Entity}Errors.cs`) and is intentionally kept flat — do not add a per-folder AGENTS.md unless one of them grows materially beyond this shape.

## For AI Agents
### Working In This Directory
- All five entities are `sealed class {Name} : Entity, ISoftDeletable` with a private parameterless ctor (`// EF Core`), a private id+fields ctor, and `public static Result<{Name}> Create(...)`. Follow this exact shape for any new college-reference entity.
- The two hierarchical relationships are one-directional FKs stored as plain `Guid`/`Guid?` on the child (`Discipline.DepartmentId`, `Specialization.SpecialityId`, `Teacher.DepartmentId?`) — there is no navigation collection back on the parent (`Department` doesn't expose `Disciplines`/`Teachers`). Cross-aggregate consistency (e.g. "does this department have disciplines?") is enforced at the `Application`/`Infrastructure` layer using the `Has*()` conflict errors defined here, not by the domain entities themselves.
- `Specialization.Create` is the one inconsistency in this folder: it skips the blank-name guard that every other `Create` in `College/` has. If you touch `Specialization.cs`, consider whether to align it with `Speciality.Create`'s validation — but confirm with the team/tests before changing behavior, since `Application`-layer validators may already be covering this gap.
- Error codes follow `"{PluralEntity}.{Reason}"` (e.g. `"Departments.HasTeachers"`, `"Disciplines.DepartmentDeleted"`) and every message text is sourced from `Domain.Resources.DomainErrors` (see `../AGENTS.md`) — add resource strings before adding error factory methods.

## Dependencies
### Internal
`SharedKernel` (`Entity`, `Result`/`Result<T>`, `Error`, `ISoftDeletable`).

### External
`Throw` — used by `Discipline.ChangeDepartment` and `Specialization.ChangeSpeciality` for `Guid.ThrowIfNull()` guard clauses.

<!-- MANUAL: -->
