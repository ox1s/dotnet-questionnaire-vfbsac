<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-08-11 | Updated: 2026-08-11 -->

# Reports

## Purpose
Implements `Application.Abstractions.Reports.IReportGenerator` — turns pre-computed analytics DTOs from `Application.Reports.Queries.*` (period reports, period-comparison reports, group-comparison reports) into downloadable binary documents. Two implementations exist side by side: an Excel generator (ClosedXML) and a Word generator (OpenXml SDK); each implements the exact same three-method interface so they're interchangeable at the DI registration point.

## Key Files
| File | Description |
|------|-------------|
| `ExcelReportGenerator.cs` | `ExcelReportGenerator(ILogger<ExcelReportGenerator>) : IReportGenerator`, **the currently DI-registered implementation** (see `../DependencyInjection.cs`). Uses `ClosedXML.Excel.XLWorkbook`. Implements `GeneratePeriodReportAsync` (single sheet: title, period, resolved filters, then a bordered/bold-header stats table with columns № / Вопрос / Удовл. потреб., % / Средний балл / Ст. откл. / Оценка / Кол-во ответов), `GeneratePeriodsComparisonReportAsync` and `GenerateGroupsComparisonReportAsync` (both build a "5 stat rows per question" vertical layout — stat rows are Удовл. потреб., %/Средний балл/Ст. откл./Оценка/Кол-во — with one column-group per period/group, merged № and Вопрос cells spanning the 5 stat rows, frozen header rows/columns via `SheetView.FreezeColumns(3)`/`FreezeRows(5)`, and `AdjustToContents()` auto-fit). All three catch and rethrow as `InvalidOperationException` with a message naming the form, after logging via injected `ILogger`. |
| `WordReportGenerator.cs` | `WordReportGenerator(ILogger<WordReportGenerator>) : IReportGenerator`, **implemented but not registered** — `../DependencyInjection.cs` has `// services.AddScoped<IReportGenerator, WordReportGenerator>();` commented out in favor of Excel. Uses `DocumentFormat.OpenXml.Wordprocessing` (`WordprocessingDocument`, `Body`, `Table`/`TableRow`/`TableCell`, `Run`/`Text`). Same three methods, same data shape, rendered as Word tables instead of worksheet ranges. Has a `[SuppressMessage("SonarLint", "S3220", ...)]` at the type level justifying the OpenXml `Append(params ...)` overload usage. Carefully disposes `WordprocessingDocument`/`MemoryStream` in `finally` blocks (with `#pragma warning disable CA1849` around the sync `Dispose()` calls, since there's no async-dispose path being used). |

## For AI Agents
### Working In This Directory
- Only one `IReportGenerator` can be active at a time under the current DI setup (a single scoped registration, last-wins) — if a caller needs both formats simultaneously (e.g. a "download as .xlsx or .docx" toggle in the UI), this needs a DI change (e.g. keyed services or an explicit factory keyed by format), not just uncommenting the Word line.
- Both generators duplicate the same statistics-table-building logic (unique-questions collection, 5-stat-row layout, `FormatNumber` = `value.ToString("F2", CultureInfo.InvariantCulture)`, `FormatRating` = maps `SatisfactionRating` to a Russian label) independently — if you change the report shape (e.g. add a stat column) or the rating labels, update **both** files or they will silently diverge.
- All Russian-language literal strings (column headers, "Нет данных для отображения", stat names) are hardcoded inline, not resource/localization-based — consistent with the rest of the codebase (see `Database/DemoDataGenerator.cs`), so don't be surprised there's no `.resx`.
- `IXLWorksheet`/OpenXml objects are heavy — both generators build the whole document in memory and return the completed `byte[]`; there's no streaming. For very large statistic sets this is the likely first place to optimize if performance becomes an issue.
- Empty-data branches exist in every method (`if (statistics.Count == 0)` / `if (periodsData.Count == 0)` / `if (groupsData.Count == 0)`) and short-circuit to a "no data" placeholder — note `WordReportGenerator.GeneratePeriodReportAsync`'s empty-table branch has the `noDataRun.Append(new Text(...))` line commented out (line 62), so that specific empty-state produces a blank paragraph with no visible text, unlike its two sibling methods in the same file which do append the text. This looks like an oversight, not an intentional inconsistency — worth fixing if touching that method.

## Dependencies
### Internal
- `Application.Abstractions.Reports.IReportGenerator` — interface implemented by both classes.
- `Application.Reports.Queries.GetAnalyticsByGroups`, `.GetAnalyticsByPeriod`, `.GetAnalyticsByPeriods`, `.Shared` (`QuestionStatistics`, `SatisfactionRating`) — input DTOs (satisfaction-%/average-score/standard-deviation/rating/response-count per question, computed per "Методика оценки удовлетворенности потребителей" already by `Application`; this layer only formats/lays them out).

### External
- `ClosedXML` (`ClosedXML.Excel`) — Excel generation in `ExcelReportGenerator`.
- `DocumentFormat.OpenXml`, `DocumentFormat.OpenXml.Packaging`, `DocumentFormat.OpenXml.Wordprocessing` — Word generation in `WordReportGenerator`.
- `Microsoft.Extensions.Logging` — both classes take an `ILogger<T>` for error logging before rethrow.

<!-- MANUAL: -->
