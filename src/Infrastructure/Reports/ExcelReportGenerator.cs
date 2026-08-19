using System.Globalization;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Application.Reports.Queries.GetAnalyticsByPeriods;
using Application.Reports.Queries.Shared;
using ClosedXML.Excel;
using Infrastructure.Reports.Charts;
using Infrastructure.Resources;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reports;

public sealed class ExcelReportGenerator(ILogger<ExcelReportGenerator> logger) : IReportGenerator
{
    public Task<byte[]> GeneratePeriodReportAsync(
        string formTitle,
        DateTime periodStart,
        DateTime periodEnd,
        Dictionary<string, string> resolvedFilters,
        GetAnalyticsByPeriodQueryResult analyticsResult,
        CancellationToken cancellationToken = default)
    {
        List<GetAnalyticsByPeriodQueryResponse> statistics = analyticsResult.Questions;
        try
        {
            using var workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add(ReportResources.SheetTitle_Report);

            int currentRow = 1;

            // Title
            worksheet.Cell(currentRow, 1).Value = formTitle;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
            currentRow += 2;

            // Period
            worksheet.Cell(currentRow, 1).Value = $"{ReportResources.Label_Period}:";
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
                worksheet.Cell(currentRow, 1).Value = ReportResources.NoDataMessage;
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return Task.FromResult(stream.ToArray());
            }

            // Header row
            worksheet.Cell(currentRow, 1).Value = ReportResources.Header_Number;
            worksheet.Cell(currentRow, 2).Value = ReportResources.Header_Question;
            worksheet.Cell(currentRow, 3).Value = ReportResources.Header_SatisfactionPercent;
            worksheet.Cell(currentRow, 4).Value = ReportResources.Header_AverageScore;
            worksheet.Cell(currentRow, 5).Value = ReportResources.Header_StandardDeviation;
            worksheet.Cell(currentRow, 6).Value = ReportResources.Header_Rating;
            worksheet.Cell(currentRow, 7).Value = ReportResources.Header_ResponseCount;

            IXLRange headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            currentRow++;

            // Data rows
            int rowNumber = 1;
            int firstDataRow = currentRow;
            foreach (GetAnalyticsByPeriodQueryResponse stat in statistics)
            {
                worksheet.Cell(currentRow, 1).Value = rowNumber;
                worksheet.Cell(currentRow, 2).Value = stat.QuestionText;

                worksheet.Cell(currentRow, 3).Value = stat.SatisfactionPercentage;
                worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "0.00";
                worksheet.Cell(currentRow, 4).Value = stat.AverageScore;
                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "0.00";
                worksheet.Cell(currentRow, 5).Value = stat.StandardDeviation;
                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "0.00";

                worksheet.Cell(currentRow, 6).Value = FormatRating(stat.Rating);
                worksheet.Cell(currentRow, 7).Value = stat.ResponseCount;

                IXLRange dataRange = worksheet.Range(currentRow, 1, currentRow, 7);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                currentRow++;
                rowNumber++;
            }

            int lastDataRow = currentRow - 1;

            currentRow++;
            AppendOverallSatisfactionSummary(worksheet, currentRow, analyticsResult.Overall, firstDataRow, lastDataRow);

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);

            var chartCategories = Enumerable.Range(1, statistics.Count)
                .Select(i => i.ToString(CultureInfo.InvariantCulture))
                .ToList();
            var chartValues = statistics.Select(s => s.SatisfactionPercentage).ToList();

            byte[] reportBytes = ExcelChartBuilder.AddSatisfactionCharts(
                memoryStream.ToArray(),
                ReportResources.SheetTitle_Report,
                chartCategories,
                chartValues);

            return Task.FromResult(reportBytes);
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
            IXLWorksheet worksheet = workbook.Worksheets.Add(ReportResources.SheetTitle_PeriodsComparison);

            int currentRow = 1;

            // Title
            worksheet.Cell(currentRow, 1).Value = ReportResources.ReportTitle_PeriodsComparison;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
            currentRow += 2;

            // Form name
            worksheet.Cell(currentRow, 1).Value = $"{ReportResources.Label_Form}:";
            worksheet.Cell(currentRow, 2).Value = formTitle;
            currentRow += 2;

            if (periodsData.Count == 0)
            {
                worksheet.Cell(currentRow, 1).Value = ReportResources.NoDataMessage;
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return Task.FromResult(stream.ToArray());
            }

            // Header row - vertical layout
            int col = 1;
            worksheet.Cell(currentRow, col++).Value = ReportResources.Header_Number;
            worksheet.Cell(currentRow, col++).Value = ReportResources.Header_Question;
            worksheet.Cell(currentRow, col++).Value = ReportResources.Header_Statistics;

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
            string[] statNames =
            [
                ReportResources.Header_SatisfactionPercent,
                ReportResources.Header_AverageScore,
                ReportResources.Header_StandardDeviation,
                ReportResources.Header_Rating,
                ReportResources.Header_ResponseCountShort
            ];

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

            currentRow++;
            AppendOverallSatisfactionComparisonSummary(
                worksheet,
                currentRow,
                ReportResources.Label_Period,
                periodsData.Select(p => (p.Label, p.Overall)).ToList());

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
            IXLWorksheet worksheet = workbook.Worksheets.Add(ReportResources.SheetTitle_GroupsComparison);

            int currentRow = 1;

            // Title
            worksheet.Cell(currentRow, 1).Value = ReportResources.ReportTitle_GroupsComparison;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
            currentRow += 2;

            // Form name
            worksheet.Cell(currentRow, 1).Value = $"{ReportResources.Label_Form}:";
            worksheet.Cell(currentRow, 2).Value = formTitle;
            currentRow += 2;

            if (groupsData.Count == 0)
            {
                worksheet.Cell(currentRow, 1).Value = ReportResources.NoDataMessage;
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return Task.FromResult(stream.ToArray());
            }

            // Header row - vertical layout
            int col = 1;
            worksheet.Cell(currentRow, col++).Value = ReportResources.Header_Number;
            worksheet.Cell(currentRow, col++).Value = ReportResources.Header_Question;
            worksheet.Cell(currentRow, col++).Value = ReportResources.Header_Statistics;

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
            string[] statNames =
            [
                ReportResources.Header_SatisfactionPercent,
                ReportResources.Header_AverageScore,
                ReportResources.Header_StandardDeviation,
                ReportResources.Header_Rating,
                ReportResources.Header_ResponseCountShort
            ];

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

            currentRow++;
            AppendOverallSatisfactionComparisonSummary(
                worksheet,
                currentRow,
                ReportResources.Header_GroupLabel,
                groupsData.Select(g => (Label: g.GroupName, g.Overall)).ToList());

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

    /// <summary>
    /// Renders the formula (5)/(6) overall form/blank satisfaction as a small label/value block,
    /// starting at <paramref name="startRow"/>. Formula (6)'s mean and formula (5)'s average
    /// standard deviation are written as live <c>AVERAGE</c> formulas over the per-question rows
    /// (<paramref name="firstDataRow"/>-<paramref name="lastDataRow"/>, columns C and E) rather
    /// than pre-computed text, so the workbook stays auditable/recalculable like the manual
    /// college-specs reference spreadsheet. The rating is a nested <c>IF</c> over Table 1's
    /// thresholds, referencing the mean-percentage cell.
    /// </summary>
    private static void AppendOverallSatisfactionSummary(
        IXLWorksheet worksheet,
        int startRow,
        OverallSatisfaction overall,
        int firstDataRow,
        int lastDataRow)
    {
        int currentRow = startRow;

        worksheet.Cell(currentRow, 1).Value = ReportResources.OverallSatisfaction_Title;
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        currentRow++;

        if (!overall.HasData)
        {
            worksheet.Cell(currentRow, 1).Value = ReportResources.OverallSatisfaction_NoData;
            return;
        }

        int meanRow = currentRow;
        worksheet.Cell(meanRow, 1).Value = ReportResources.OverallSatisfaction_MeanPercent;
        worksheet.Cell(meanRow, 2).FormulaA1 = $"=AVERAGE(C{firstDataRow}:C{lastDataRow})";
        worksheet.Cell(meanRow, 2).Style.NumberFormat.Format = "0.00";
        currentRow++;

        worksheet.Cell(currentRow, 1).Value = ReportResources.OverallSatisfaction_AvgStdDev;
        worksheet.Cell(currentRow, 2).FormulaA1 = $"=AVERAGE(E{firstDataRow}:E{lastDataRow})";
        worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "0.00";
        currentRow++;

        string meanRef = $"B{meanRow}";
        worksheet.Cell(currentRow, 1).Value = ReportResources.OverallSatisfaction_Rating;
        worksheet.Cell(currentRow, 2).FormulaA1 =
            $"=IF({meanRef}<40,\"{ReportResources.Rating_Unsatisfactory}\"," +
            $"IF({meanRef}<60,\"{ReportResources.Rating_Satisfactory}\"," +
            $"IF({meanRef}<80,\"{ReportResources.Rating_Good}\",\"{ReportResources.Rating_Excellent}\")))";
    }

    /// <summary>
    /// Renders the formula (5)/(6) overall satisfaction for each compared period/group as a small
    /// table, starting at <paramref name="startRow"/>.
    /// </summary>
    private static void AppendOverallSatisfactionComparisonSummary(
        IXLWorksheet worksheet,
        int startRow,
        string entryLabelHeader,
        List<(string Label, OverallSatisfaction Overall)> entries)
    {
        int currentRow = startRow;

        worksheet.Cell(currentRow, 1).Value = ReportResources.OverallSatisfaction_Title;
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        currentRow++;

        int summaryHeaderRow = currentRow;
        worksheet.Cell(currentRow, 1).Value = entryLabelHeader;
        worksheet.Cell(currentRow, 2).Value = ReportResources.OverallSatisfaction_MeanPercent;
        worksheet.Cell(currentRow, 3).Value = ReportResources.OverallSatisfaction_AvgStdDev;
        worksheet.Cell(currentRow, 4).Value = ReportResources.OverallSatisfaction_Rating;

        IXLRange summaryHeaderRange = worksheet.Range(summaryHeaderRow, 1, summaryHeaderRow, 4);
        summaryHeaderRange.Style.Font.Bold = true;
        summaryHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        summaryHeaderRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        summaryHeaderRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        currentRow++;

        foreach ((string label, OverallSatisfaction overall) in entries)
        {
            worksheet.Cell(currentRow, 1).Value = label;

            if (overall.HasData)
            {
                worksheet.Cell(currentRow, 2).Value = FormatNumber(overall.MeanPercentage);
                worksheet.Cell(currentRow, 3).Value = FormatNumber(overall.AverageStandardDeviation);
                worksheet.Cell(currentRow, 4).Value = FormatRating(overall.Rating);
            }
            else
            {
                worksheet.Cell(currentRow, 2).Value = ReportResources.OverallSatisfaction_NoData;
            }

            IXLRange summaryRowRange = worksheet.Range(currentRow, 1, currentRow, 4);
            summaryRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summaryRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            currentRow++;
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
            SatisfactionRating.Excellent => ReportResources.Rating_Excellent,
            SatisfactionRating.Good => ReportResources.Rating_Good,
            SatisfactionRating.Satisfactory => ReportResources.Rating_Satisfactory,
            _ => ReportResources.Rating_Unsatisfactory
        };
    }
}
