<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# college-specs/

## Purpose
Source-of-truth domain documents from the college/institution that this questionnaire system is built for. These are the real-world scoring methodology and sample-data files that the satisfaction-survey analytics/reporting logic (`src/Application/Reports`, `src/Infrastructure/Reports`, and their legacy counterparts under `src/Questionnaire.Application/Reports`) is meant to reproduce in code. They are binary Microsoft Office formats (`.doc`, `.xlsx`) and were not parsed as part of producing this file — descriptions below are based on filename/role only, not verified document content.

## Key Files
| File | Description |
|------|--------------|
| `Методика_оценки_удовлетворенности_потребителей.doc` (Russian: "Methodology for evaluating consumer/customer satisfaction") | Legacy MS Word `.doc` binary. By name, this is the formal methodology document defining how student ("consumer") satisfaction should be scored/aggregated — the specification the Reports feature's calculation logic (weights, marks, summary scores) is presumably derived from. Content unread — could not be parsed with text tools. |
| `обработка_удовл_обучающихся_преподаванием_дисциплин_18_19_июнь.xlsx` (Russian: "processing of student satisfaction with subject/discipline teaching, June 18-19") | Legacy MS Excel `.xlsx` binary. By name, this is a worked example / sample dataset showing the methodology above applied to real survey responses about how disciplines were taught, dated around June 18-19 (year unspecified). Likely useful as a reference for expected output shape (e.g. per-discipline aggregate scores) of the Reports export. Content unread — could not be parsed with text tools. |

## For AI Agents
### Working In This Directory
- **These are opaque binaries to this agent.** `.doc` (old binary Word format, not `.docx`) and `.xlsx` cannot be read with the available text-reading tools. Do not fabricate or assume specific formulas, weightings, or score thresholds from these files — any claim about "the methodology says X" must be sourced from a human/domain expert who has actually opened these files, or from equivalent logic already implemented and comment-documented in `src/Application/Reports` / `src/Infrastructure/Reports` / `src/Questionnaire.Application/Reports`.
- If a task requires actually reading these documents (e.g. "verify the report calculation matches the methodology"), flag to the user/maintainer that the `.doc`/`.xlsx` content needs to be manually extracted or converted (e.g. to `.docx`/`.csv`) first, or ask a domain expert to summarize the scoring rules in a plain-text form that can be committed alongside the code.
- Do not attempt to overwrite, rename, or "clean up" these files — they are reference source documents from the institution, not generated artifacts.
- The `18_19_июнь` in the xlsx filename most plausibly reads as a date range (June 18-19) for when the underlying survey data was collected, not a version number — worth confirming with a domain expert if this file is ever used to validate report output for a specific term/period.

## Dependencies
### Internal
- Conceptually related to (but not code-linked to) `src/Application/Reports`, `src/Infrastructure/Reports` (active stack), and `src/Questionnaire.Application/Reports` (legacy stack) — the report-generation logic these directories aim to model. No build system or code in the repo directly references these files by path.

### External
- Requires Microsoft Word (or a compatible viewer) to open the `.doc` file, and Microsoft Excel (or a compatible viewer) to open the `.xlsx` file. No automated tooling in this repo currently parses either format for report validation.

<!-- MANUAL: -->
