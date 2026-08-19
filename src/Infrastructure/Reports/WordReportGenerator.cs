using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Application.Reports.Queries.GetAnalyticsByPeriods;
using Application.Reports.Queries.Shared;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Infrastructure.Resources;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reports;

[SuppressMessage("SonarLint", "S3220:Method calls should not resolve ambiguously to overloads with params", Justification = "OpenXml Append params overload is intentional")]

public sealed class WordReportGenerator(ILogger<WordReportGenerator> logger) : IReportGenerator
{
    public Task<byte[]> GeneratePeriodReportAsync(
        string formTitle,
        DateTime periodStart,
        DateTime periodEnd,
        Dictionary<string, string> resolvedFilters,
        List<PeriodReportSheet> sheets,
        CancellationToken cancellationToken = default)
    {
        // Word export never grew multi-sheet support; it renders only the first sheet and is not
        // DI-registered (see DependencyInjection.cs), so this is a reasonable stopgap.
        GetAnalyticsByPeriodQueryResult analyticsResult = sheets.Count > 0
            ? sheets[0].AnalyticsResult
            : new GetAnalyticsByPeriodQueryResult([], new OverallSatisfaction(0, 0, SatisfactionRating.Unsatisfactory, HasData: false), 0);
        List<GetAnalyticsByPeriodQueryResponse> statistics = analyticsResult.Questions;
        MemoryStream? memoryStream = null;
        WordprocessingDocument? document = null;

        try
        {
            memoryStream = new MemoryStream();
            document = WordprocessingDocument.Create(
                memoryStream,
                WordprocessingDocumentType.Document);

            MainDocumentPart mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            // Add title
            AddTitle(body, formTitle);

            // Add period information
            AddMetadataLine(
                body,
                ReportResources.Label_Period,
                $"{periodStart:dd.MM.yyyy} - {periodEnd:dd.MM.yyyy}");

            // Add filters
            foreach (KeyValuePair<string, string> filter in resolvedFilters)
            {
                AddMetadataLine(body, filter.Key, filter.Value);
            }

            AddEmptyLine(body);

            // Create table
            if (statistics.Count == 0)
            {
                Paragraph noDataParagraph = new();
                Run noDataRun = new();
                noDataRun.Append(new Text(ReportResources.NoDataMessage));
                noDataParagraph.Append(noDataRun);
                body.Append(noDataParagraph);
            }
            else
            {
                Table table = new();

                // Set table width to 100%
                TableProperties tableProperties = new();
                TableWidth tableWidth = new() { Width = "5000", Type = TableWidthUnitValues.Pct };
                tableProperties.Append(tableWidth);
                table.Append(tableProperties);

                // Add header row
                TableRow headerRow = new();
                headerRow.Append(
                    CreateCell(ReportResources.Header_Number, true),
                    CreateCell(ReportResources.Header_Question, true),
                    CreateCell(ReportResources.Header_SatisfactionPercent, true),
                    CreateCell(ReportResources.Header_AverageScore, true),
                    CreateCell(ReportResources.Header_StandardDeviation, true),
                    CreateCell(ReportResources.Header_Rating, true),
                    CreateCell(ReportResources.Header_ResponseCount, true)
                );
                table.Append(headerRow);

                // Add data rows
                int rowNumber = 1;
                foreach (GetAnalyticsByPeriodQueryResponse stat in statistics)
                {
                    TableRow dataRow = new();
                    dataRow.Append(
                        CreateCell(rowNumber.ToString(CultureInfo.InvariantCulture)),
                        CreateCell(stat.QuestionText),
                        CreateCell(FormatNumber(stat.SatisfactionPercentage)),
                        CreateCell(FormatNumber(stat.AverageScore)),
                        CreateCell(FormatNumber(stat.StandardDeviation)),
                        CreateCell(FormatRating(stat.Rating)),
                        CreateCell(stat.ResponseCount.ToString(CultureInfo.InvariantCulture))
                    );
                    table.Append(dataRow);
                    rowNumber++;
                }

                body.Append(table);

                AddEmptyLine(body);
                AddOverallSatisfactionSummary(body, analyticsResult.Overall);
            }

            // Save and return
            document.Save();
            document.Dispose();
            document = null;

            return Task.FromResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating Word document for form {FormTitle}",
                formTitle);
            throw new InvalidOperationException($"Failed to generate Word report for form '{formTitle}'", ex);
        }
        finally
        {
            document?.Dispose();
#pragma warning disable CA1849
            memoryStream?.Dispose();
#pragma warning restore CA1849
        }
    }

    public Task<byte[]> GeneratePeriodsComparisonReportAsync(
        string formTitle,
        List<GetAnalyticsByPeriodsQueryResponse> periodsData,
        CancellationToken cancellationToken = default)
    {
        MemoryStream? memoryStream = null;
        WordprocessingDocument? document = null;

        try
        {
            memoryStream = new MemoryStream();
            document = WordprocessingDocument.Create(
                memoryStream,
                WordprocessingDocumentType.Document);

            MainDocumentPart mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            // Add title
            AddTitle(body, ReportResources.ReportTitle_PeriodsComparison);

            // Add form name
            AddMetadataLine(body, ReportResources.Label_Form, formTitle);

            AddEmptyLine(body);

            // Create table
            if (periodsData.Count == 0)
            {
                Paragraph noDataParagraph = new();
                Run noDataRun = new();
                noDataRun.Append(new Text(ReportResources.NoDataMessage));
                noDataParagraph.Append(noDataRun);
                body.Append(noDataParagraph);
            }
            else
            {
                Table table = new();

                // Set table width to 100%
                TableProperties tableProperties = new();
                TableWidth tableWidth = new() { Width = "5000", Type = TableWidthUnitValues.Pct };
                tableProperties.Append(tableWidth);
                table.Append(tableProperties);

                // Add header row with dynamic columns
                TableRow headerRow = new();
                headerRow.Append(CreateCell(ReportResources.Header_Number, true));
                headerRow.Append(CreateCell(ReportResources.Header_Question, true));

                foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
                {
                    string periodLabel = $"{period.Label} ({period.PeriodStart:dd.MM.yyyy} - {period.PeriodEnd:dd.MM.yyyy})";
                    headerRow.Append(
                        CreateCell(periodLabel, true),
                        CreateCell(ReportResources.Header_SatisfactionPercent, true),
                        CreateCell(ReportResources.Header_AverageScore, true),
                        CreateCell(ReportResources.Header_StandardDeviation, true),
                        CreateCell(ReportResources.Header_Rating, true),
                        CreateCell(ReportResources.Header_ResponseCountShort, true)
                    );
                }

                table.Append(headerRow);

                // Get all unique questions
                Dictionary<Guid, string> allQuestions = new();
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

                // Add data rows
                int rowNumber = 1;
                foreach (KeyValuePair<Guid, string> question in allQuestions)
                {
                    TableRow dataRow = new();
                    dataRow.Append(CreateCell(rowNumber.ToString(CultureInfo.InvariantCulture)));
                    dataRow.Append(CreateCell(question.Value));

                    foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
                    {
                        QuestionStatistics? stat = period.QuestionStatistics
                            .FirstOrDefault(s => s.QuestionId == question.Key);

                        if (stat is not null)
                        {
                            dataRow.Append(
                                CreateCell(FormatNumber(stat.SatisfactionPercentage)),
                                CreateCell(FormatNumber(stat.AverageScore)),
                                CreateCell(FormatNumber(stat.StandardDeviation)),
                                CreateCell(FormatRating(stat.Rating)),
                                CreateCell(stat.ResponseCount.ToString(CultureInfo.InvariantCulture))
                            );
                        }
                        else
                        {
                            dataRow.Append(
                                CreateCell("-"),
                                CreateCell("-"),
                                CreateCell("-"),
                                CreateCell("-"),
                                CreateCell("-")
                            );
                        }
                    }

                    table.Append(dataRow);
                    rowNumber++;
                }

                body.Append(table);

                AddEmptyLine(body);
                AddOverallSatisfactionComparisonSummary(
                    body,
                    ReportResources.Label_Period,
                    periodsData.Select(p => (p.Label, p.Overall)).ToList());
            }

            // Save and return
            document.Save();
            document.Dispose();
            document = null;

            return Task.FromResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating periods comparison Word document for form {FormTitle}",
                formTitle);
            throw new InvalidOperationException($"Failed to generate periods comparison report for form '{formTitle}'", ex);
        }
        finally
        {
            document?.Dispose();
#pragma warning disable CA1849
            memoryStream?.Dispose();
#pragma warning restore CA1849
        }
    }

    public Task<byte[]> GenerateGroupsComparisonReportAsync(
        string formTitle,
        List<GetAnalyticsByGroupsQueryResponse> groupsData,
        CancellationToken cancellationToken = default)
    {
        MemoryStream? memoryStream = null;
        WordprocessingDocument? document = null;

        try
        {
            memoryStream = new MemoryStream();
            document = WordprocessingDocument.Create(
                memoryStream,
                WordprocessingDocumentType.Document);

            MainDocumentPart mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            // Add title
            AddTitle(body, ReportResources.ReportTitle_GroupsComparison);

            // Add form name
            AddMetadataLine(body, ReportResources.Label_Form, formTitle);

            AddEmptyLine(body);

            // Create table
            if (groupsData.Count == 0)
            {
                Paragraph noDataParagraph = new();
                Run noDataRun = new();
                noDataRun.Append(new Text(ReportResources.NoDataMessage));
                noDataParagraph.Append(noDataRun);
                body.Append(noDataParagraph);
            }
            else
            {
                Table table = new();

                // Set table width to 100%
                TableProperties tableProperties = new();
                TableWidth tableWidth = new() { Width = "5000", Type = TableWidthUnitValues.Pct };
                tableProperties.Append(tableWidth);
                table.Append(tableProperties);

                // Add header row with dynamic columns
                TableRow headerRow = new();
                headerRow.Append(CreateCell(ReportResources.Header_Number, true));
                headerRow.Append(CreateCell(ReportResources.Header_Question, true));

                foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
                {
                    headerRow.Append(
                        CreateCell(group.GroupName, true),
                        CreateCell(ReportResources.Header_SatisfactionPercent, true),
                        CreateCell(ReportResources.Header_AverageScore, true),
                        CreateCell(ReportResources.Header_StandardDeviation, true),
                        CreateCell(ReportResources.Header_Rating, true),
                        CreateCell(ReportResources.Header_ResponseCountShort, true)
                    );
                }

                table.Append(headerRow);

                // Get all unique questions
                Dictionary<Guid, string> allQuestions = new();
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

                // Add data rows
                int rowNumber = 1;
                foreach (KeyValuePair<Guid, string> question in allQuestions)
                {
                    TableRow dataRow = new();
                    dataRow.Append(CreateCell(rowNumber.ToString(CultureInfo.InvariantCulture)));
                    dataRow.Append(CreateCell(question.Value));

                    foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
                    {
                        QuestionStatistics? stat = group.QuestionStatistics
                            .FirstOrDefault(s => s.QuestionId == question.Key);

                        if (stat is not null)
                        {
                            dataRow.Append(
                                CreateCell(FormatNumber(stat.SatisfactionPercentage)),
                                CreateCell(FormatNumber(stat.AverageScore)),
                                CreateCell(FormatNumber(stat.StandardDeviation)),
                                CreateCell(FormatRating(stat.Rating)),
                                CreateCell(stat.ResponseCount.ToString(CultureInfo.InvariantCulture))
                            );
                        }
                        else
                        {
                            dataRow.Append(
                                CreateCell("-"),
                                CreateCell("-"),
                                CreateCell("-"),
                                CreateCell("-"),
                                CreateCell("-")
                            );
                        }
                    }

                    table.Append(dataRow);
                    rowNumber++;
                }

                body.Append(table);

                AddEmptyLine(body);
                AddOverallSatisfactionComparisonSummary(
                    body,
                    ReportResources.Header_GroupLabel,
                    groupsData.Select(g => (Label: g.GroupName, g.Overall)).ToList());
            }

            // Save and return
            document.Save();
            document.Dispose();
            document = null;

            return Task.FromResult(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating groups comparison Word document for form {FormTitle}",
                formTitle);
            throw new InvalidOperationException($"Failed to generate groups comparison report for form '{formTitle}'", ex);
        }
        finally
        {
            document?.Dispose();
#pragma warning disable CA1849
            memoryStream?.Dispose();
#pragma warning restore CA1849
        }
    }

    /// <summary>
    /// Renders the formula (5)/(6) overall form/blank satisfaction as a title and a few metadata lines.
    /// </summary>
    private static void AddOverallSatisfactionSummary(Body body, OverallSatisfaction overall)
    {
        AddTitle(body, ReportResources.OverallSatisfaction_Title);

        if (!overall.HasData)
        {
            Paragraph noDataParagraph = new();
            Run noDataRun = new();
            noDataRun.Append(new Text(ReportResources.OverallSatisfaction_NoData));
            noDataParagraph.Append(noDataRun);
            body.Append(noDataParagraph);
            return;
        }

        AddMetadataLine(body, ReportResources.OverallSatisfaction_MeanPercent, FormatNumber(overall.MeanPercentage));
        AddMetadataLine(body, ReportResources.OverallSatisfaction_AvgStdDev, FormatNumber(overall.AverageStandardDeviation));
        AddMetadataLine(body, ReportResources.OverallSatisfaction_Rating, FormatRating(overall.Rating));
    }

    /// <summary>
    /// Renders the formula (5)/(6) overall satisfaction for each compared period/group as a title
    /// followed by one metadata line per entry.
    /// </summary>
    private static void AddOverallSatisfactionComparisonSummary(
        Body body,
        string entryLabelPrefix,
        List<(string Label, OverallSatisfaction Overall)> entries)
    {
        AddTitle(body, ReportResources.OverallSatisfaction_Title);

        foreach ((string label, OverallSatisfaction overall) in entries)
        {
            string summary = overall.HasData
                ? $"{FormatNumber(overall.MeanPercentage)}% ± {FormatNumber(overall.AverageStandardDeviation)} ({FormatRating(overall.Rating)})"
                : ReportResources.OverallSatisfaction_NoData;
            AddMetadataLine(body, $"{entryLabelPrefix} \"{label}\"", summary);
        }
    }

    private static void AddTitle(Body body, string title)
    {
        Paragraph paragraph = new();
        Run run = new();
        RunProperties runProperties = new();
        Bold bold = new();
        FontSize fontSize = new() { Val = "32" }; // 16pt = 32 half-points

        runProperties.Append(bold, fontSize);
        run.Append(runProperties, new Text(title));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private static void AddMetadataLine(Body body, string label, string value)
    {
        Paragraph paragraph = new();
        Run run = new();
        run.Append(new Text($"{label}: {value}"));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private static void AddEmptyLine(Body body)
    {
        Paragraph paragraph = new();
        body.Append(paragraph);
    }

    private static TableCell CreateCell(string text, bool isBold = false)
    {
        TableCell cell = new();
        Paragraph paragraph = new();
        Run run = new();

        if (isBold)
        {
            RunProperties runProperties = new();
            Bold bold = new();
            runProperties.Append(bold);
            run.Append(runProperties);
        }

        run.Append(new Text(text));
        paragraph.Append(run);
        cell.Append(paragraph, CreateCellProperties());

        return cell;
    }

    private static TableCellProperties CreateCellProperties()
    {
        TableCellProperties properties = new();
        TableCellBorders borders = new()
        {
            TopBorder = new TopBorder { Val = BorderValues.Single, Size = 4 },
            BottomBorder = new BottomBorder { Val = BorderValues.Single, Size = 4 },
            LeftBorder = new LeftBorder { Val = BorderValues.Single, Size = 4 },
            RightBorder = new RightBorder { Val = BorderValues.Single, Size = 4 }
        };
        properties.Append(borders);
        return properties;
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
