using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalytics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using A = DocumentFormat.OpenXml.Wordprocessing;

namespace Infrastructure.Reports;

public sealed class WordReportGenerator(IApplicationDbContext context) : IReportGenerator
{
    public async Task<byte[]> GenerateAnalyticsReport(AnalyticsReportResponse analyticsReport,
        CancellationToken cancellationToken = default)
    {
        ReportDictionaries dictionaries = await LoadDisplayNamesAsync(analyticsReport, cancellationToken);

        using MemoryStream stream = new();

        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new A.Document();
            Body body = mainPart.Document.AppendChild(new Body());

            Run titleRun = new();
            titleRun.AppendChild(new A.Text($"Отчет по форме: {analyticsReport.FormTitle}"));

            RunProperties titleProps = new();
            titleProps.AppendChild(new Bold());
            titleProps.AppendChild(new FontSize { Val = "32" });

            titleRun.PrependChild(titleProps);

            Paragraph titlePara = new();
            titlePara.AppendChild(titleRun);
            body.AppendChild(titlePara);

            AddParagraph(body, $"Количество срезов: {analyticsReport.Slices.Count}");
            body.AppendChild(new Paragraph());

            foreach (AnalyticsSliceResponse slice in analyticsReport.Slices)
            {
                AddParagraph(body, $"Срез: {slice.Label}");
                AddParagraph(
                    body,
                    $"Период: {slice.DateFrom:yyyy-MM-dd} - {slice.DateTo:yyyy-MM-dd}");
                AddParagraph(body, $"Всего анкет: {slice.TotalSubmissions}");
                AddParagraph(
                    body,
                    $"Средний балл (общий): {slice.OverallAverage.ToString("F2", CultureInfo.InvariantCulture)}");
                AddParagraph(
                    body,
                    $"Отклонение: {slice.OverallStandardDeviation.ToString("F2", CultureInfo.InvariantCulture)}");

                string filtersLine = BuildFiltersLine(slice.Filters, dictionaries);
                if (!string.IsNullOrWhiteSpace(filtersLine))
                {
                    AddParagraph(body, $"Фильтры: {filtersLine}");
                }

                body.AppendChild(new Paragraph());
            }

            A.Table table = new();

            TableProperties tblProps = new();
            TableBorders borders = new();

            EnumValue<BorderValues> borderType = new(BorderValues.Single);
            UInt32Value borderSize = 4;

            borders.AppendChild(new TopBorder { Val = borderType, Size = borderSize });
            borders.AppendChild(new BottomBorder { Val = borderType, Size = borderSize });
            borders.AppendChild(new LeftBorder { Val = borderType, Size = borderSize });
            borders.AppendChild(new RightBorder { Val = borderType, Size = borderSize });
            borders.AppendChild(new InsideHorizontalBorder { Val = borderType, Size = borderSize });
            borders.AppendChild(new InsideVerticalBorder { Val = borderType, Size = borderSize });

            tblProps.AppendChild(borders);
            tblProps.AppendChild(new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct });

            table.AppendChild(tblProps);

            TableRow trHeader = new();
            trHeader.AppendChild(CreateCell("№", true));
            trHeader.AppendChild(CreateCell("Вопрос", true));
            foreach (AnalyticsSliceResponse slice in analyticsReport.Slices)
            {
                trHeader.AppendChild(CreateCell($"{slice.Label} (итог)", true));
                trHeader.AppendChild(CreateCell($"{slice.Label} (ср.)", true));
                trHeader.AppendChild(CreateCell($"{slice.Label} (sigma)", true));
            }

            table.AppendChild(trHeader);

            var orderedQuestions = analyticsReport.Questions
                .OrderBy(question => question.Order)
                .ToList();

            for (int i = 0; i < orderedQuestions.Count; i++)
            {
                AnalyticsQuestionResponse question = orderedQuestions[i];
                TableRow tr = new();

                tr.AppendChild(CreateCell((i + 1).ToString(CultureInfo.InvariantCulture)));
                tr.AppendChild(CreateCell(question.QuestionText));

                foreach (AnalyticsQuestionSliceMetricResponse metric in question.SliceMetrics)
                {
                    tr.AppendChild(CreateCell(metric.ResultScore.ToString("F2", CultureInfo.InvariantCulture)));
                    tr.AppendChild(CreateCell(metric.AverageScore.ToString("F2", CultureInfo.InvariantCulture)));
                    tr.AppendChild(CreateCell(metric.StandardDeviation.ToString("F2", CultureInfo.InvariantCulture)));
                }

                table.AppendChild(tr);
            }

            body.AppendChild(table);
            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private sealed record ReportDictionaries(
        Dictionary<Guid, string> Teachers,
        Dictionary<Guid, string> Disciplines,
        Dictionary<Guid, string> Departments,
        Dictionary<Guid, string> Specialities,
        Dictionary<Guid, string> Specializations);

    private async Task<ReportDictionaries> LoadDisplayNamesAsync(
        AnalyticsReportResponse report,
        CancellationToken ct)
    {
        var teacherIds = report.Slices.Select(s => s.Filters.TeacherId).OfType<Guid>().Distinct().ToList();
        var disciplineIds = report.Slices.Select(s => s.Filters.DisciplineId).OfType<Guid>().Distinct().ToList();
        var departmentIds = report.Slices.Select(s => s.Filters.DepartmentId).OfType<Guid>().Distinct().ToList();
        var specialityIds = report.Slices.Select(s => s.Filters.SpecialityId).OfType<Guid>().Distinct().ToList();
        var specializationIds =
            report.Slices.Select(s => s.Filters.SpecializationId).OfType<Guid>().Distinct().ToList();

        Dictionary<Guid, string> teachers = teacherIds.Count != 0
            ? await context.Teachers.Where(t => teacherIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.FullName, ct)
            : [];

        Dictionary<Guid, string> disciplines = disciplineIds.Count != 0
            ? await context.Disciplines.Where(d => disciplineIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name, ct)
            : [];

        Dictionary<Guid, string> departments = departmentIds.Count != 0
            ? await context.Departments.Where(d => departmentIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name, ct)
            : [];

        Dictionary<Guid, string> specialities = specialityIds.Count != 0
            ? await context.Specialities.Where(s => specialityIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, ct)
            : [];

        Dictionary<Guid, string> specializations = specializationIds.Count != 0
            ? await context.Specializations.Where(s => specializationIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, ct)
            : [];

        return new ReportDictionaries(teachers, disciplines, departments, specialities, specializations);
    }

    private static string BuildFiltersLine(AnalyticsFilterSet filters, ReportDictionaries dicts)
    {
        List<string> parts = [];
        
        if (filters.TeacherId.HasValue)
        {
            parts.Add($"Преподаватель={dicts.Teachers.GetValueOrDefault(filters.TeacherId.Value, filters.TeacherId.Value.ToString())}");
        }

        if (filters.DisciplineId.HasValue)
        {
            parts.Add($"Дисциплина={dicts.Disciplines.GetValueOrDefault(filters.DisciplineId.Value, filters.DisciplineId.Value.ToString())}");
        }

        if (filters.DepartmentId.HasValue)
        {
            parts.Add($"Кафедра={dicts.Departments.GetValueOrDefault(filters.DepartmentId.Value, filters.DepartmentId.Value.ToString())}");
        }

        if (filters.SpecialityId.HasValue)
        {
            parts.Add($"Специальность={dicts.Specialities.GetValueOrDefault(filters.SpecialityId.Value, filters.SpecialityId.Value.ToString())}");
        }

        if (filters.SpecializationId.HasValue)
        {
            parts.Add($"Специализация={dicts.Specializations.GetValueOrDefault(filters.SpecializationId.Value, filters.SpecializationId.Value.ToString())}");
        }

        if (!string.IsNullOrWhiteSpace(filters.OrganizationName))
        {
            parts.Add($"Организация={filters.OrganizationName}");
        }

        return string.Join(", ", parts);
    }

    private static void AddParagraph(Body body, string text)
    {
        Paragraph p = new();
        Run r = new();
        r.AppendChild(new A.Text(text));
        p.AppendChild(r);
        body.AppendChild(p);
    }

    private static TableCell CreateCell(string text, bool bold = false)
    {
        Run run = new();
        run.AppendChild(new A.Text(text));

        if (bold)
        {
            RunProperties props = new();
            props.AppendChild(new Bold());
            run.PrependChild(props);
        }

        Paragraph p = new();
        p.AppendChild(run);

        TableCell cell = new();

        TableCellProperties cellProps = new();
        cellProps.AppendChild(new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = "2400" });
        cell.AppendChild(cellProps);

        cell.AppendChild(p);

        return cell;
    }
}
