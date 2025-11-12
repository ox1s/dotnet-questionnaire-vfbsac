using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Application.Reports.Queries.GetSummary;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Infrastructure.Services;

public class OpenXmlReportGenerator : IReportGenerator
{
    public byte[] GenerateSummaryReport(SummaryReportResult data)
    {
        using var stream = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Заголовок
            body.AppendChild(CreateParagraph($"Отчет по анкете: {data.FormName}", bold: true, fontSize: "32"));
            body.AppendChild(CreateParagraph($"Всего прохождений: {data.TotalSubmissions}", fontSize: "28"));
            body.AppendChild(CreateParagraph("")); // Пустая строка

            foreach (var question in data.Questions)
            {
                body.AppendChild(CreateParagraph(question.QuestionText, bold: true, fontSize: "28"));

                switch (question.QuestionType)
                {
                    case QuestionType.Rating:
                        if (question.RatingResponseCount > 0)
                        {
                            body.AppendChild(CreateParagraph($"Средняя оценка: {question.AverageMark:F2}"));
                            body.AppendChild(CreateParagraph($"Средний вес: {question.AverageWeight:F2}"));
                            body.AppendChild(CreateParagraph($"Количество ответов: {question.RatingResponseCount}"));
                        }
                        else
                        {
                            body.AppendChild(CreateParagraph("Нет данных."));
                        }
                        break;

                    case QuestionType.Text:
                        if (question.TextResponses.Any())
                        {
                            foreach (var response in question.TextResponses)
                            {
                                body.AppendChild(CreateParagraph($"- {response}"));
                            }
                        }
                        else
                        {
                            body.AppendChild(CreateParagraph("Нет данных."));
                        }
                        break;

                    case QuestionType.Choice:
                        if (question.Options.Any())
                        {
                            foreach (var option in question.Options)
                            {
                                var count = question.ChoiceCounts.TryGetValue(option.Id, out var val) ? val : 0;
                                body.AppendChild(CreateParagraph($"{option.Text}: {count} выборов"));
                            }
                        }
                        else
                        {
                            body.AppendChild(CreateParagraph("Нет данных."));
                        }
                        break;
                }
                body.AppendChild(CreateParagraph("")); 
            }
        }
        return stream.ToArray();
    }

    private static Paragraph CreateParagraph(string text, string fontSize = "24", bool bold = false)
    {
        var run = new Run(new Text(text));
        var runProperties = new RunProperties(new FontSize { Val = fontSize });
        if (bold)
        {
            runProperties.Append(new Bold());
        }
        run.PrependChild(runProperties);
        return new Paragraph(run);
    }
}