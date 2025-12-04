using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Contracts.Reports;
using Questionnaire.Contracts.Questions;

namespace Questionnaire.Infrastructure.Services;

public class OpenXmlReportGenerator : IReportGenerator
{
    public byte[] GenerateSummaryReport(SummaryReportResponse data)
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
                        if (question.RatingData is not null)
                        {
                            body.AppendChild(CreateParagraph($"Средняя оценка: {question.RatingData.AverageMark:F2}"));
                            body.AppendChild(CreateParagraph($"Средний вес: {question.RatingData.AverageWeight:F2}"));
                            body.AppendChild(CreateParagraph($"Количество ответов: {question.RatingData.ResponseCount}"));
                        }
                        else
                        {
                            body.AppendChild(CreateParagraph("Нет данных."));
                        }
                        break;

                    case QuestionType.Text:
                        if (question.TextData is not null && question.TextData.Any())
                        {
                            foreach (string response in question.TextData)
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
                        if (question.ChoiceData is not null && question.ChoiceData.Any())
                        {
                            foreach (var choice in question.ChoiceData)
                            {
                                body.AppendChild(CreateParagraph($"{choice.OptionText}: {choice.SelectedCount} выборов"));
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