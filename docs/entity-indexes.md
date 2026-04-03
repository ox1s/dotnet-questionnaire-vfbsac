# Индексы сущностей

Ниже перечислены сущности, для которых в текущей модели EF Core есть индексы.

## Department

- `Name` - уникальный индекс (`ix_departments_name`)

## Discipline

- `DepartmentId` - обычный индекс (`ix_disciplines_department_id`)
- `Name` - уникальный индекс (`ix_disciplines_name`)

## Question

- `(FormId, Order)` - уникальный составной индекс (`ix_question_form_id_order`)

## Answer

- `(SubmissionId, QuestionId)` - уникальный составной индекс (`ix_answer_submission_id_question_id`)

## Submission

- `FormId` - обычный индекс (`ix_submissions_form_id`)
- `(FormId, SubmittedAt)` - составной индекс (`ix_submissions_form_id_submitted_at`)

## Submission.Context

Это owned-объект внутри `Submission`, но его поля тоже индексируются в таблице `submissions`.

- `DepartmentId` -> `context_department_id` (`ix_submissions_context_department_id`)
- `DisciplineId` -> `discipline_id` (`ix_submissions_discipline_id`)
- `SpecialityId` -> `context_speciality_id` (`ix_submissions_context_speciality_id`)
- `SpecializationId` -> `context_specialization_id` (`ix_submissions_context_specialization_id`)
- `TeacherId` -> `teacher_id` (`ix_submissions_teacher_id`)

## Сущности без индексов

В текущем snapshot модели отдельные индексы не найдены у:

- `Form`
- `Teacher`
- `Speciality`
- `Specialization`
- `User`

Источники:

- `src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`
- `src/Infrastructure/College/Department/DepartmentConfiguration.cs`
- `src/Infrastructure/College/Discipline/DisciplineConfiguration.cs`
- `src/Infrastructure/Questionnaires/Form/QuestionConfiguration.cs`
- `src/Infrastructure/Questionnaires/Submission/AnswerConfiguration.cs`
- `src/Infrastructure/Questionnaires/Submission/SubmissionConfiguration.cs`
