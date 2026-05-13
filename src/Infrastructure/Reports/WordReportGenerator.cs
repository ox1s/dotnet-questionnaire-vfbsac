using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Application.Reports.Queries.GetAnalyticsByPeriods;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reports;

[SuppressMessage("SonarLint", "S3220:Method calls should not resolve ambiguously to overloads with params", Justification = "OpenXml Append params overload is intentional")]

public sealed class WordReportGenerator(ILogger<WordReportGenerator> logger) : IWordReportGenerator
{
    public Task<byte[]> GeneratePeriodReportAsync(
        string formTitle,
        DateTime periodStart,
        DateTime periodEnd,
        Dictionary<string, string> resolvedFilters,
        List<GetAnalyticsByPeriodQueryResponse> statistics,
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
            AddTitle(body, formTitle);

            // Add period information
            AddMetadataLine(
                body,
                "Период",
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
                // noDataRun.Append(new Text("Нет данных для отображения"));
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
                    CreateCell("№", true),
                    CreateCell("Вопрос", true),
                    CreateCell("Медиана", true),
                    CreateCell("Среднее", true),
                    CreateCell("Мода", true),
                    CreateCell("Ст. откл.", true),
                    CreateCell("Кол-во ответов", true)
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
                        CreateCell(FormatNumber(stat.Median)),
                        CreateCell(FormatNumber(stat.Mean)),
                        CreateCell(FormatNumber(stat.Mode)),
                        CreateCell(FormatNumber(stat.StandardDeviation)),
                        CreateCell(stat.ResponseCount.ToString(CultureInfo.InvariantCulture))
                    );
                    table.Append(dataRow);
                    rowNumber++;
                }

                body.Append(table);
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
            AddTitle(body, "Сравнительный отчет по периодам");

            // Add form name
            AddMetadataLine(body, "Форма", formTitle);

            AddEmptyLine(body);

            // Create table
            if (periodsData.Count == 0)
            {
                Paragraph noDataParagraph = new();
                Run noDataRun = new();
                noDataRun.Append(new Text("Нет данных для отображения"));
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
                headerRow.Append(CreateCell("№", true));
                headerRow.Append(CreateCell("Вопрос", true));

                foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
                {
                    string periodLabel = $"{period.Label} ({period.PeriodStart:dd.MM.yyyy} - {period.PeriodEnd:dd.MM.yyyy})";
                    headerRow.Append(
                        CreateCell(periodLabel, true),
                        CreateCell("Медиана", true),
                        CreateCell("Среднее", true),
                        CreateCell("Мода", true),
                        CreateCell("Ст. откл.", true),
                        CreateCell("Кол-во", true)
                    );
                }

                table.Append(headerRow);

                // Get all unique questions
                Dictionary<Guid, string> allQuestions = new();
                foreach (GetAnalyticsByPeriodsQueryResponse period in periodsData)
                {
                    foreach (Application.Reports.Queries.Shared.QuestionStatistics stat in period.QuestionStatistics)
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
                        Application.Reports.Queries.Shared.QuestionStatistics? stat = period.QuestionStatistics
                            .FirstOrDefault(s => s.QuestionId == question.Key);

                        if (stat is not null)
                        {
                            dataRow.Append(
                                CreateCell(FormatNumber(stat.Median)),
                                CreateCell(FormatNumber(stat.Mean)),
                                CreateCell(FormatNumber(stat.Mode)),
                                CreateCell(FormatNumber(stat.StandardDeviation)),
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
            AddTitle(body, "Сравнительный отчет по группам");

            // Add form name
            AddMetadataLine(body, "Форма", formTitle);

            AddEmptyLine(body);

            // Create table
            if (groupsData.Count == 0)
            {
                Paragraph noDataParagraph = new();
                Run noDataRun = new();
                noDataRun.Append(new Text("Нет данных для отображения"));
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
                headerRow.Append(CreateCell("№", true));
                headerRow.Append(CreateCell("Вопрос", true));

                foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
                {
                    headerRow.Append(
                        CreateCell(group.GroupName, true),
                        CreateCell("Медиана", true),
                        CreateCell("Среднее", true),
                        CreateCell("Мода", true),
                        CreateCell("Ст. откл.", true),
                        CreateCell("Кол-во", true)
                    );
                }

                table.Append(headerRow);

                // Get all unique questions
                Dictionary<Guid, string> allQuestions = new();
                foreach (GetAnalyticsByGroupsQueryResponse group in groupsData)
                {
                    foreach (Application.Reports.Queries.Shared.QuestionStatistics stat in group.QuestionStatistics)
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
                        Application.Reports.Queries.Shared.QuestionStatistics? stat = group.QuestionStatistics
                            .FirstOrDefault(s => s.QuestionId == question.Key);

                        if (stat is not null)
                        {
                            dataRow.Append(
                                CreateCell(FormatNumber(stat.Median)),
                                CreateCell(FormatNumber(stat.Mean)),
                                CreateCell(FormatNumber(stat.Mode)),
                                CreateCell(FormatNumber(stat.StandardDeviation)),
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
}
