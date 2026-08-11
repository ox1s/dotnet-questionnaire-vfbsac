using System.Globalization;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Application.Reports.Queries.GetAnalyticsByPeriods;
using Application.Reports.Queries.Shared;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reports;

public sealed class ExcelReportGenerator(ILogger<ExcelReportGenerator> logger) : IReportGenerator
{
    public Task<byte[]> GeneratePeriodReportAsync(
        string formTitle,
        DateTime periodStart,
        DateTime periodEnd,
        Dictionary<string, string> resolvedFilters,
        List<GetAnalyticsByPeriodQueryResponse> statistics,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("Отчет");

            int currentRow = 1;

            // Title
            worksheet.Cell(currentRow, 1).Value = formTitle;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
            currentRow += 2;

            // Period
            worksheet.Cell(currentRow, 1).Value = "Период:";
            worksheet.Cell(currentRow, 2).Value = $"{periodStart:dd.MM.yyyy} - {periodEnd:dd.MM.yyyy}";
            currentRow++;

            // Filters
            foreach (KeyValuePair<string, string> filter in resolvedFilters)
            {
                worksheet.Cell(currentRow, 1).Value = $"{filter.Key}:";
                worksheet.Cell(currentRow, 2).Value = filter.Value;
                currentRow++;
            }

            currentRow++;

            if (statistics.Count == 0)
            {
                worksheet.Cell(currentRow, 1).Value = "Нет данных для отображения";
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return Task.FromResult(stream.ToArray());
            }

            // Header row
            worksheet.Cell(currentRow, 1).Value = "№";
            worksheet.Cell(currentRow, 2).Value = "Вопрос";
            worksheet.Cell(currentRow, 3).Value = "Удовл. потреб., %";
            worksheet.Cell(currentRow, 4).Value = "Средний балл";
            worksheet.Cell(currentRow, 5).Value = "Ст. откл.";
            worksheet.Cell(currentRow, 6).Value = "Оценка";
            worksheet.Cell(currentRow, 7).Value = "Кол-во ответов";

            IXLRange headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            currentRow++;

            // Data rows
            int rowNumber = 1;
            foreach (GetAnalyticsByPeriodQueryResponse stat in statistics)
            {
                worksheet.Cell(currentRow, 1).Value = rowNumber;
                worksheet.Cell(currentRow, 2).Value = stat.QuestionText;
                worksheet.Cell(currentRow, 3).Value = FormatNumber(stat.SatisfactionPercentage);
                worksheet.Cell(currentRow, 4).Value = FormatNumber(stat.AverageScore);
                worksheet.Cell(currentRow, 5).Value = FormatNumber(stat.StandardDeviation);
                worksheet.Cell(currentRow, 6).Value = FormatRating(stat.Rating);
                worksheet.Cell(currentRow, 7).Value = stat.ResponseCount;

                IXLRange dataRange = worksheet.Range(currentRow, 1, currentRow, 7);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                currentRow++;
                rowNumber++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return Task.FromResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Excel document for form {FormTitle}", formTitle);
            throw new InvalidOperationException($"Failed to generate Excel report for form '{formTitle}'", ex);
        }
    }

    public Task<byte[]> GeneratePeriodsComparisonReportAsync(
        string formTitle,
        List<GetAnalyticsByPeriodsQueryResponse> periodsData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("Сравнение периодов");

            int currentRow = 1;

            // Title
            worksheet.Cell(currentRow, 1).Value = "Сравнительный отчет по периодам";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
            currentRow += 2;

            // Form name
            worksheet.Cell(currentRow, 1).Value = "Форма:";
            worksheet.Cell(currentRow, 2).Value = formTitle;
            currentRow += 2;

            if (periodsData.Count == 0)
            {
                worksheet.Cell(currentRow, 1).Value = "Нет данных для отображения";
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return Task.FromResult(stream.ToArray());
            }

            // Header row - vertical layout
            int col = 1;
            worksheet.Cell(currentRow, col++).Value = "№";
            worksheet.Cell(currentRow, col++).Value = "Вопрос";
            worksheet.Cell(currentRow, col++).Value = "Статистика";

            foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
            {
                string periodLabel =
                    $"{period.Label}\n({period.PeriodStart:dd.MM.yyyy} - {period.PeriodEnd:dd.MM.yyyy})";
                worksheet.Cell(currentRow, col++).Value = periodLabel;
            }

            IXLRange headerRange = worksheet.Range(currentRow, 1, currentRow, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.WrapText = true;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            currentRow++;

            // Get all unique questions
            var allQuestions = new Dictionary<Guid, string>();
            foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
            {
                foreach (QuestionStatistics stat in period.QuestionStatistics)
                {
                    if (!allQuestions.ContainsKey(stat.QuestionId))
                    {
                        allQuestions[stat.QuestionId] = stat.QuestionText;
                    }
                }
            }

            // Data rows - 5 rows per question (one per stat)
            int rowNumber = 1;
            string[] statNames = ["Удовл. потреб., %", "Средний балл", "Ст. откл.", "Оценка", "Кол-во"];

            foreach (KeyValuePair<Guid, string> question in allQuestions)
            {
                int questionStartRow = currentRow;

                for (int statIndex = 0; statIndex < statNames.Length; statIndex++)
                {
                    col = 1;

                    // Row number (merged across 5 stat rows)
                    if (statIndex == 0)
                    {
                        worksheet.Cell(currentRow, col).Value = rowNumber;
                    }

                    col++;

                    // Question text (merged across 5 stat rows)
                    if (statIndex == 0)
                    {
                        worksheet.Cell(currentRow, col).Value = question.Value;
                    }

                    col++;

                    // Stat name
                    worksheet.Cell(currentRow, col++).Value = statNames[statIndex];

                    // Values for each period
                    foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
                    {
                        QuestionStatistics? stat =
                            period.QuestionStatistics.FirstOrDefault(s => s.QuestionId == question.Key);

                        if (stat is not null)
                        {
                            string value = statIndex switch
                            {
                                0 => FormatNumber(stat.SatisfactionPercentage),
                                1 => FormatNumber(stat.AverageScore),
                                2 => FormatNumber(stat.StandardDeviation),
                                3 => FormatRating(stat.Rating),
                                4 => stat.ResponseCount.ToString(CultureInfo.InvariantCulture),
                                _ => "-"
                            };
                            worksheet.Cell(currentRow, col++).Value = value;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, col++).Value = "-";
                        }
                    }

                    IXLRange dataRange = worksheet.Range(currentRow, 1, currentRow, col - 1);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    currentRow++;
                }

                // Merge row number and question cells
                if (statNames.Length > 1)
                {
                    worksheet.Range(questionStartRow, 1, questionStartRow + statNames.Length - 1, 1).Merge();
                    worksheet.Range(questionStartRow, 2, questionStartRow + statNames.Length - 1, 2).Merge();

                    worksheet.Cell(questionStartRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(questionStartRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                rowNumber++;
            }

            // Freeze first three columns (№, Вопрос, Статистика)
            worksheet.SheetView.FreezeColumns(3);
            worksheet.SheetView.FreezeRows(5); // Title + form + header

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return Task.FromResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating periods comparison Excel document for form {FormTitle}", formTitle);
            throw new InvalidOperationException(
                $"Failed to generate periods comparison Excel report for form '{formTitle}'", ex);
        }
    }

    public Task<byte[]> GenerateGroupsComparisonReportAsync(
        string formTitle,
        List<GetAnalyticsByGroupsQueryResponse> groupsData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("Сравнение групп");

            int currentRow = 1;

            // Title
            worksheet.Cell(currentRow, 1).Value = "Сравнительный отчет по группам";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
            currentRow += 2;

            // Form name
            worksheet.Cell(currentRow, 1).Value = "Форма:";
            worksheet.Cell(currentRow, 2).Value = formTitle;
            currentRow += 2;

            if (groupsData.Count == 0)
            {
                worksheet.Cell(currentRow, 1).Value = "Нет данных для отображения";
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return Task.FromResult(stream.ToArray());
            }

            // Header row - vertical layout
            int col = 1;
            worksheet.Cell(currentRow, col++).Value = "№";
            worksheet.Cell(currentRow, col++).Value = "Вопрос";
            worksheet.Cell(currentRow, col++).Value = "Статистика";

            foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
            {
                worksheet.Cell(currentRow, col++).Value = group.GroupName;
            }

            IXLRange headerRange = worksheet.Range(currentRow, 1, currentRow, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.WrapText = true;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            currentRow++;

            // Get all unique questions
            var allQuestions = new Dictionary<Guid, string>();
            foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
            {
                foreach (QuestionStatistics stat in group.QuestionStatistics)
                {
                    if (!allQuestions.ContainsKey(stat.QuestionId))
                    {
                        allQuestions[stat.QuestionId] = stat.QuestionText;
                    }
                }
            }

            // Data rows - 5 rows per question (one per stat)
            int rowNumber = 1;
            string[] statNames = ["Удовл. потреб., %", "Средний балл", "Ст. откл.", "Оценка", "Кол-во"];

            foreach (KeyValuePair<Guid, string> question in allQuestions)
            {
                int questionStartRow = currentRow;

                for (int statIndex = 0; statIndex < statNames.Length; statIndex++)
                {
                    col = 1;

                    // Row number (merged across 5 stat rows)
                    if (statIndex == 0)
                    {
                        worksheet.Cell(currentRow, col).Value = rowNumber;
                    }

                    col++;

                    // Question text (merged across 5 stat rows)
                    if (statIndex == 0)
                    {
                        worksheet.Cell(currentRow, col).Value = question.Value;
                    }

                    col++;

                    // Stat name
                    worksheet.Cell(currentRow, col++).Value = statNames[statIndex];

                    // Values for each group
                    foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
                    {
                        QuestionStatistics? stat =
                            group.QuestionStatistics.FirstOrDefault(s => s.QuestionId == question.Key);

                        if (stat is not null)
                        {
                            string value = statIndex switch
                            {
                                0 => FormatNumber(stat.SatisfactionPercentage),
                                1 => FormatNumber(stat.AverageScore),
                                2 => FormatNumber(stat.StandardDeviation),
                                3 => FormatRating(stat.Rating),
                                4 => stat.ResponseCount.ToString(CultureInfo.InvariantCulture),
                                _ => "-"
                            };
                            worksheet.Cell(currentRow, col++).Value = value;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, col++).Value = "-";
                        }
                    }

                    IXLRange dataRange = worksheet.Range(currentRow, 1, currentRow, col - 1);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    currentRow++;
                }

                // Merge row number and question cells
                if (statNames.Length > 1)
                {
                    worksheet.Range(questionStartRow, 1, questionStartRow + statNames.Length - 1, 1).Merge();
                    worksheet.Range(questionStartRow, 2, questionStartRow + statNames.Length - 1, 2).Merge();

                    worksheet.Cell(questionStartRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(questionStartRow, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                rowNumber++;
            }

            // Freeze first three columns (№, Вопрос, Статистика)
            worksheet.SheetView.FreezeColumns(3);
            worksheet.SheetView.FreezeRows(5); // Title + form + header

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return Task.FromResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating groups comparison Excel document for form {FormTitle}", formTitle);
            throw new InvalidOperationException(
                $"Failed to generate groups comparison Excel report for form '{formTitle}'", ex);
        }
    }

    private static string FormatNumber(decimal value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }

    private static string FormatRating(SatisfactionRating rating)
    {
        return rating switch
        {
            SatisfactionRating.Excellent => "отлично",
            SatisfactionRating.Good => "хорошо",
            SatisfactionRating.Satisfactory => "удовлетворительно",
            _ => "неудовлетворительно"
        };
    }
}
