using System.Globalization;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalytics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Wordprocessing;

namespace Infrastructure.Reports;

public sealed class WordReportGenerator : IReportGenerator
{
    public byte[] GenerateAnalyticsReport(AnalyticsReportResponse analyticsReport)
    {
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

                string filtersLine = BuildFiltersLine(slice);
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

    private static string BuildFiltersLine(AnalyticsSliceResponse slice)
    {
        List<string> parts = [];
        AnalyticsFilterDisplaySet display = slice.FilterDisplay;
        AnalyticsFilterSet filters = slice.Filters;

        if (filters.TeacherId.HasValue)
        {
            parts.Add($"Преподаватель={display.Teacher ?? filters.TeacherId.Value.ToString()}");
        }

        if (filters.DisciplineId.HasValue)
        {
            parts.Add($"Дисциплина={display.Discipline ?? filters.DisciplineId.Value.ToString()}");
        }

        if (filters.DepartmentId.HasValue)
        {
            parts.Add($"Кафедра={display.Department ?? filters.DepartmentId.Value.ToString()}");
        }

        if (filters.SpecialityId.HasValue)
        {
            parts.Add($"Специальность={display.Speciality ?? filters.SpecialityId.Value.ToString()}");
        }

        if (filters.SpecializationId.HasValue)
        {
            parts.Add($"Специализация={display.Specialization ?? filters.SpecializationId.Value.ToString()}");
        }

        if (!string.IsNullOrWhiteSpace(filters.OrganizationName))
        {
            parts.Add($"Организация={display.Organization ?? filters.OrganizationName}");
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
