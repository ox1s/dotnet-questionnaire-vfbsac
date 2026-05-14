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
            worksheet.Cell(currentRow, 3).Value = "Медиана";
            worksheet.Cell(currentRow, 4).Value = "Среднее";
            worksheet.Cell(currentRow, 5).Value = "Мода";
            worksheet.Cell(currentRow, 6).Value = "Ст. откл.";
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
                worksheet.Cell(currentRow, 3).Value = FormatNumber(stat.Median);
                worksheet.Cell(currentRow, 4).Value = FormatNumber(stat.Mean);
                worksheet.Cell(currentRow, 5).Value = FormatNumber(stat.Mode);
                worksheet.Cell(currentRow, 6).Value = FormatNumber(stat.StandardDeviation);
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

            // Header row
            int col = 1;
            worksheet.Cell(currentRow, col++).Value = "№";
            worksheet.Cell(currentRow, col++).Value = "Вопрос";

            foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
            {
                string periodLabel = $"{period.Label} ({period.PeriodStart:dd.MM.yyyy} - {period.PeriodEnd:dd.MM.yyyy})";
                worksheet.Cell(currentRow, col++).Value = $"{periodLabel} - Медиана";
                worksheet.Cell(currentRow, col++).Value = $"{periodLabel} - Среднее";
                worksheet.Cell(currentRow, col++).Value = $"{periodLabel} - Мода";
                worksheet.Cell(currentRow, col++).Value = $"{periodLabel} - Ст. откл.";
                worksheet.Cell(currentRow, col++).Value = $"{periodLabel} - Кол-во";
            }

            IXLRange headerRange = worksheet.Range(currentRow, 1, currentRow, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.WrapText = true;

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

            // Data rows
            int rowNumber = 1;
            foreach (KeyValuePair<Guid, string> question in allQuestions)
            {
                col = 1;
                worksheet.Cell(currentRow, col++).Value = rowNumber;
                worksheet.Cell(currentRow, col++).Value = question.Value;

                foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
                {
                    QuestionStatistics? stat = period.QuestionStatistics.FirstOrDefault(s => s.QuestionId == question.Key);

                    if (stat is not null)
                    {
                        worksheet.Cell(currentRow, col++).Value = FormatNumber(stat.Median);
                        worksheet.Cell(currentRow, col++).Value = FormatNumber(stat.Mean);
                        worksheet.Cell(currentRow, col++).Value = FormatNumber(stat.Mode);
                        worksheet.Cell(currentRow, col++).Value = FormatNumber(stat.StandardDeviation);
                        worksheet.Cell(currentRow, col++).Value = stat.ResponseCount;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, col++).Value = "-";
                        worksheet.Cell(currentRow, col++).Value = "-";
                        worksheet.Cell(currentRow, col++).Value = "-";
                        worksheet.Cell(currentRow, col++).Value = "-";
                        worksheet.Cell(currentRow, col++).Value = "-";
                    }
                }

                IXLRange dataRange = worksheet.Range(currentRow, 1, currentRow, col - 1);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                currentRow++;
                rowNumber++;
            }

            // Freeze first two columns (№ and Вопрос)
            worksheet.SheetView.FreezeColumns(2);
            worksheet.SheetView.FreezeRows(currentRow - rowNumber);

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return Task.FromResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating periods comparison Excel document for form {FormTitle}", formTitle);
            throw new InvalidOperationException($"Failed to generate periods comparison Excel report for form '{formTitle}'", ex);
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

            // Header row
            int col = 1;
            worksheet.Cell(currentRow, col++).Value = "№";
            worksheet.Cell(currentRow, col++).Value = "Вопрос";

            foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
            {
                worksheet.Cell(currentRow, col++).Value = $"{group.GroupName} - Медиана";
                worksheet.Cell(currentRow, col++).Value = $"{group.GroupName} - Среднее";
                worksheet.Cell(currentRow, col++).Value = $"{group.GroupName} - Мода";
                worksheet.Cell(currentRow, col++).Value = $"{group.GroupName} - Ст. откл.";
                worksheet.Cell(currentRow, col++).Value = $"{group.GroupName} - Кол-во";
            }

            IXLRange headerRange = worksheet.Range(currentRow, 1, currentRow, col - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.WrapText = true;

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

            // Data rows
            int rowNumber = 1;
            foreach (KeyValuePair<Guid, string> question in allQuestions)
            {
                col = 1;
                worksheet.Cell(currentRow, col++).Value = rowNumber;
                worksheet.Cell(currentRow, col++).Value = question.Value;

                foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
                {
                    QuestionStatistics? stat = group.QuestionStatistics.FirstOrDefault(s => s.QuestionId == question.Key);

                    if (stat is not null)
                    {
                        worksheet.Cell(currentRow, col++).Value = FormatNumber(stat.Median);
                        worksheet.Cell(currentRow, col++).Value = FormatNumber(stat.Mean);
                        worksheet.Cell(currentRow, col++).Value = FormatNumber(stat.Mode);
                        worksheet.Cell(currentRow, col++).Value = FormatNumber(stat.StandardDeviation);
                        worksheet.Cell(currentRow, col++).Value = stat.ResponseCount;
                    }
                    else
                    {
                        worksheet.Cell(currentRow, col++).Value = "-";
                        worksheet.Cell(currentRow, col++).Value = "-";
                        worksheet.Cell(currentRow, col++).Value = "-";
                        worksheet.Cell(currentRow, col++).Value = "-";
                        worksheet.Cell(currentRow, col++).Value = "-";
                    }
                }

                IXLRange dataRange = worksheet.Range(currentRow, 1, currentRow, col - 1);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                currentRow++;
                rowNumber++;
            }

            // Freeze first two columns (№ and Вопрос)
            worksheet.SheetView.FreezeColumns(2);
            worksheet.SheetView.FreezeRows(currentRow - rowNumber);

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return Task.FromResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating groups comparison Excel document for form {FormTitle}", formTitle);
            throw new InvalidOperationException($"Failed to generate groups comparison Excel report for form '{formTitle}'", ex);
        }
    }

    private static string FormatNumber(decimal value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }
}
