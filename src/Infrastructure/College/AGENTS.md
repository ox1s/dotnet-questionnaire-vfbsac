<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# College

## Purpose
Holds EF Core `IEntityTypeConfiguration<T>` classes for the five "college dictionary" entities from `Domain.College.*`: `Department`, `Discipline`, `Speciality`, `Specialization`, `Teacher`. Each is a small reference/lookup table used to tag questionnaire submissions with organizational context (which department, discipline, teacher, speciality, specialization a submission relates to). There is no business logic here — every file is purely fluent-API mapping consumed by `ApplicationDbContext.OnModelCreating` via `ApplyConfigurationsFromAssembly`.

## Key Files
| File | Description |
|------|-------------|
| `Department/DepartmentConfiguration.cs` | Maps `Domain.College.Departments.Department`. `Name` required, max length 100, **unique index**. |
| `Discipline/DisciplineConfiguration.cs` | Maps `Domain.College.Disciplines.Discipline`. `Name` required, max length 255, unique index. Required FK `DepartmentId` → `Department`, `DeleteBehavior.Restrict`. |
| `Speciality/SpecialityConfiguration.cs` | Maps `Domain.College.Specialities.Speciality`. `Name` required, max length 255. No FK, no unique index (only config here without a uniqueness constraint on `Name`). |
| `Specialization/SpecializationConfiguration.cs` | Maps `Domain.College.Specializations.Specialization`. `Name` max length 255 (not marked required — differs from siblings). `SpecialityId` configured but not marked `.IsRequired()` explicitly. |
| `Teacher/TeacherConfiguration.cs` | Maps `Domain.College.Teachers.Teacher`. `FullName` required, max length 255. `DepartmentId` is **optional** (`.IsRequired(false)`) with a non-unique index and FK `DeleteBehavior.Restrict` to `Department` — teachers can exist without a department assignment (see the `RemoveTeacherDepartment`/`AddTeacherDepartmentMarker` migrations, which changed this relationship over time). |

## Subdirectories
All five subdirectories (`Department/`, `Discipline/`, `Speciality/`, `Specialization/`, `Teacher/`) follow the identical one-class-per-folder pattern described above; none contain anything beyond a single `*Configuration.cs` file.

## For AI Agents
### Working In This Directory
- Every class here is `internal sealed`, named `<Entity>Configuration`, implements `IEntityTypeConfiguration<Domain.College.<Plural>.<Entity>>`, and is auto-discovered by `ApplyConfigurationsFromAssembly` in `Database/ApplicationDbContext.cs` — no manual registration needed when adding a new one, as long as the namespace/folder matches the existing convention (`Infrastructure.College.<Entity>`).
- These five entities are **not** identical in strictness: `Department`/`Discipline` enforce unique `Name`; `Speciality`/`Specialization`/`Teacher` do not. `Discipline` requires its `DepartmentId` FK; `Teacher`'s is optional. Don't assume uniformity when adding new fields — check the specific file.
- Soft-delete: `Department`, `Discipline`, `Speciality`, `Specialization`, `Teacher` all have `IsDeleted`-based global query filters applied centrally in `ApplicationDbContext.OnModelCreating` (not in these configuration files) — so a `.Remove()` via `IRepository<T>`/`DbContext` here should generally go through the domain's soft-delete method rather than a hard `DELETE`, or the query filters will hide the "deleted" row on all subsequent reads while it still occupies the unique index (relevant for `Department.Name`/`Discipline.Name` uniqueness).
- When adding a new college dictionary entity, follow the existing folder-per-entity convention and put the FK `DeleteBehavior` decision deliberately — `Restrict` is used everywhere here to avoid silent cascading deletes of departments/teachers.

## Dependencies
### Internal
- `Domain.College.Departments`, `Domain.College.Disciplines`, `Domain.College.Specialities`, `Domain.College.Specializations`, `Domain.College.Teachers` — the entities being configured.
- Consumed by `Infrastructure/Database/ApplicationDbContext.cs` (via assembly scan) and indirectly by `Infrastructure/Database/DemoDataGenerator.cs`, which creates instances of all five entities for seeding.

### External
- `Microsoft.EntityFrameworkCore` / `Microsoft.EntityFrameworkCore.Metadata.Builders` — fluent configuration API only.

<!-- MANUAL: -->
