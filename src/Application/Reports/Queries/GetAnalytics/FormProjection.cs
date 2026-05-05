using Domain.Questionnaires.Forms;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed record FormProjection(
    Guid Id,
    string Title,
    List<QuestionProjection> Questions);

internal sealed record QuestionProjection(
    Guid Id,
    string Text,
    QuestionType Type,
    int Order);
