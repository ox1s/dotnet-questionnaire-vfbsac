using Domain.Questionnaires.Form;

namespace Application.Forms.Create;

public sealed record QuestionRequest(
    string Text,
    QuestionType Type,
    int Order);
