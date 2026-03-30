using System.Globalization;
using Application.Abstractions.Reports;
using Application.Submissions.GetStatistics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Wordprocessing;

namespace Infrastructure.Reports;

public class WordReportGenerator : IReportGenerator
{
    public byte[] GenerateFormReport(string formTitle, SubmissionStatisticsResponse stats)
    {
        using MemoryStream stream = new();

        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new A.Document();
            Body body = mainPart.Document.AppendChild(new Body());

            // 1. Заголовок
            Run titleRun = new();
            titleRun.AppendChild(new A.Text($"Отчет по форме: {formTitle}"));

            RunProperties titleProps = new();
            titleProps.AppendChild(new Bold());
            titleProps.AppendChild(new FontSize { Val = "32" });

            titleRun.PrependChild(titleProps);

            Paragraph titlePara = new();
            titlePara.AppendChild(titleRun);
            body.AppendChild(titlePara);

            // 2. Общая статистика
            AddParagraph(body, $"Всего анкет: {stats.TotalSubmissions}");
            AddParagraph(body, $"Средний балл (общий): {stats.OverallAverage.ToString("F2", CultureInfo.InvariantCulture)}");
            AddParagraph(body, $"Отклонение: {stats.OverallStandardDeviation.ToString("F2", CultureInfo.InvariantCulture)}");

            body.AppendChild(new Paragraph());

            // 3. Таблица
            A.Table table = new();

            // Стили таблицы
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

            // Шапка таблицы
            TableRow trHeader = new();
            trHeader.AppendChild(CreateCell("№", true));
            trHeader.AppendChild(CreateCell("Вопрос", true));
            trHeader.AppendChild(CreateCell("Оценка", true));
            trHeader.AppendChild(CreateCell("Откл.", true));
            table.AppendChild(trHeader);

            // Данные
            for (int i = 0; i < stats.ResultScores.Count; i++)
            {
                TableRow tr = new();

                tr.AppendChild(CreateCell((i + 1).ToString(CultureInfo.InvariantCulture)));
                tr.AppendChild(CreateCell($"Вопрос {i + 1}"));
                tr.AppendChild(CreateCell(stats.ResultScores[i].ToString("F2", CultureInfo.InvariantCulture)));

                string dev = i < stats.StandardDeviations.Count
                    ? stats.StandardDeviations[i].ToString("F2", CultureInfo.InvariantCulture)
                    : "-";

                tr.AppendChild(CreateCell(dev));
                table.AppendChild(tr);
            }

            body.AppendChild(table);
            mainPart.Document.Save();
        }

        return stream.ToArray();
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
