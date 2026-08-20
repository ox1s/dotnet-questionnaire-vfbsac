<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Questionnaires

## Purpose
EF Core `IEntityTypeConfiguration<T>` mappings for the core questionnaire aggregate (`Form/`: `Form` + `Question`) and the response aggregate (`Submission/`: `Submission` + `Answer`). Together these model a dynamic-form system: a `Form` has an ordered list of `Question`s of varying `QuestionType`, and each `Submission` (one per respondent per form) has an `Answer` per question plus a `Context` value object recording which teacher/discipline/department/speciality/specialization/organization the submission relates to.

## Key Files
| File | Description |
|------|-------------|
| `Form/FormConfiguration.cs` | Maps `Form`. `Title` required, max length 500. `RequiredFilters` (a `List<FilterField>`) is stored as a JSON string via `HasConversion` + `JsonSerializer.Serialize/Deserialize`, with a custom `ValueComparer` (sequence-equality + hash aggregation) so EF's change tracker diffs the list correctly. `Questions` is a one-to-many owned-by-FK relationship (`HasForeignKey(q => q.FormId)`, `DeleteBehavior.Cascade` — deleting a `Form` deletes its `Question`s). |
| `Form/QuestionConfiguration.cs` | Maps `Question`. `Text` required, max length 2000. `Type` (`QuestionType` enum) stored as `int`. **Unique composite index** on `(FormId, Order)` — two questions in the same form cannot share an order/position. |
| `Submission/SubmissionConfiguration.cs` | Maps `Submission`. `FormId`/`UserId` required. `SubmittedAt` forced to `DateTimeKind.Utc` on write via `HasConversion` (read side is a no-op) — guards against Postgres `timestamp without time zone` round-trips silently becoming `Unspecified`. Indexes on `FormId` and composite `(FormId, SubmittedAt)` for analytics/reporting queries. `Context` is an **owned entity** (`OwnsOne`) mapped to explicit snake_case-ish column names (`teacher_id`, `discipline_id`, `context_department_id`, `context_speciality_id`, `context_specialization_id`, `context_organization_name`, `context_education_form`, `context_employee_category`, `context_position`), each individually indexed for analytics filtering. `Answers` is one-to-many, FK `SubmissionId`, `DeleteBehavior.Cascade`. |
| `Submission/AnswerConfiguration.cs` | Maps `Answer`. `SubmissionId`/`QuestionId` required. `Value` (free text) max length 5000. `NumericValue` and `Weight` both `decimal(18,2)`. **Unique composite index** on `(SubmissionId, QuestionId)` — one answer per question per submission. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Form/` | `Form` and `Question` EF configurations. |
| `Submission/` | `Submission` and `Answer` EF configurations. |

## For AI Agents
### Working In This Directory
- All four classes are `internal sealed`, auto-discovered via `ApplyConfigurationsFromAssembly` in `Database/ApplicationDbContext.cs` — no manual registration.
- `FormConfiguration`'s JSON-column pattern for `RequiredFilters` is the only value-conversion + custom-comparer example in the codebase; copy it verbatim (including the `ValueComparer`) if you add another list/array-typed property to an entity, otherwise EF will either throw on change-tracking or silently fail to detect mutations to the list.
- The `Submission.Context` owned-entity column names are **not** consistently prefixed: `teacher_id`/`discipline_id` have no `context_` prefix while every other `Context` property does (`context_department_id`, etc.) — this is existing behavior baked into shipped migrations, not a bug to "fix" casually; changing it means writing a migration that renames live columns.
- Soft-delete query filters for `Form`, `Answer`, `Submission` (`!x.IsDeleted`) live centrally in `ApplicationDbContext.OnModelCreating`, not in these files.
- Cascade deletes are intentional here (`Form → Question`, `Submission → Answer`) — unlike the `Restrict` FKs used throughout `College/`. If you add a new child collection to either aggregate, match the existing cascade behavior for consistency with the aggregate-root delete semantics.
- `DemoDataGenerator` (`Database/DemoDataGenerator.cs`) is the best reference for how `Form`/`Question`/`Submission`/`Answer` are constructed and populated end-to-end (including `AddQuestion`, `AddAnswer`, and the reflection hack used to backdate `Submission.SubmittedAt` for seed realism).

## Dependencies
### Internal
- `Domain.Questionnaires.Forms` (`Form`, `Question`, `FilterField`, `QuestionType`), `Domain.Questionnaires.Submissions` (`Submission`, `Answer`) — entities being configured.
- Consumed by `Database/ApplicationDbContext.cs` and read/written extensively by `Application` query/command handlers (e.g. `Application.Reports.Queries.*`, which the `Reports/` generators consume) and by `Reports/ExcelReportGenerator.cs` / `Reports/WordReportGenerator.cs` indirectly through those DTOs.

### External
- `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.ChangeTracking` (`ValueComparer`), `Microsoft.EntityFrameworkCore.Metadata.Builders` — fluent configuration.
- `System.Text.Json` — JSON serialization of `Form.RequiredFilters`.

<!-- MANUAL: -->
