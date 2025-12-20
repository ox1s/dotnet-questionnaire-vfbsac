using System.Globalization;
using Application.Submissions.GetStatistics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Wordprocessing;

namespace Infrastructure.Reports;

public class WordReportGenerator
{
    public byte[] GenerateFormReport(string formTitle, SubmissionStatisticsResponse stats)
    {
        using var stream = new MemoryStream();

        // Исправлено IDE0007: Использование var
        using var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
        mainPart.Document = new A.Document();
        Body body = mainPart.Document.AppendChild(new Body());

        // 1. Заголовок
        // Исправлено S3220: Использование AppendChild вместо конструкторов с params
        var titleRun = new Run();
        titleRun.AppendChild(new A.Text($"Отчет по форме: {formTitle}"));

        var titleProps = new RunProperties();
        titleProps.AppendChild(new Bold());
        titleProps.AppendChild(new FontSize { Val = "32" }); // 16pt

        titleRun.PrependChild(titleProps);

        var titlePara = new Paragraph();
        titlePara.AppendChild(titleRun);
        body.AppendChild(titlePara);

        // Статистика (параграфы)
        var pTotal = new Paragraph();
        var runTotal = new Run();
        runTotal.AppendChild(new A.Text($"Всего анкет: {stats.TotalSubmissions}"));
        pTotal.AppendChild(runTotal);
        body.AppendChild(pTotal);

        var pAvg = new Paragraph();
        var runAvg = new Run();
        runAvg.AppendChild(new A.Text($"Средний балл по всем вопросам: {stats.OverallAverage.ToString("F2", CultureInfo.InvariantCulture)}"));
        pAvg.AppendChild(runAvg);
        body.AppendChild(pAvg);

        // 2. Таблица
        var table = new A.Table();

        // Стили таблицы
        var tblProps = new TableProperties();
        var borders = new TableBorders();

        borders.AppendChild(new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 });
        borders.AppendChild(new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 });
        borders.AppendChild(new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 });
        borders.AppendChild(new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 });
        borders.AppendChild(new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 });
        borders.AppendChild(new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 });

        tblProps.AppendChild(borders);
        table.AppendChild(tblProps);

        // Шапка
        var trHeader = new TableRow();
        trHeader.AppendChild(CreateCell("№", true));
        trHeader.AppendChild(CreateCell("Средний балл", true));
        trHeader.AppendChild(CreateCell("Отклонение", true));
        table.AppendChild(trHeader);

        // Данные
        for (int i = 0; i < stats.ResultScores.Count; i++)
        {
            var tr = new TableRow();
            tr.AppendChild(CreateCell((i + 1).ToString(CultureInfo.InvariantCulture)));
            tr.AppendChild(CreateCell(stats.ResultScores[i].ToString("F2", CultureInfo.InvariantCulture)));

            string dev = i < stats.StandardDeviations.Count
                ? stats.StandardDeviations[i].ToString("F2", CultureInfo.InvariantCulture)
                : "-";

            tr.AppendChild(CreateCell(dev));
            table.AppendChild(tr);
        }

        body.AppendChild(table);
        mainPart.Document.Save();

        return stream.ToArray();
    }

    private static TableCell CreateCell(string text, bool bold = false)
    {
        var run = new Run();
        run.AppendChild(new A.Text(text));

        if (bold)
        {
            var props = new RunProperties();
            props.AppendChild(new Bold());
            run.PrependChild(props);
        }

        var p = new Paragraph();
        p.AppendChild(run);

        var cell = new TableCell();
        cell.AppendChild(p);

        return cell;
    }
}
