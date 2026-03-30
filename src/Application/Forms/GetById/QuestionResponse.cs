using Domain.Questionnaires.Form;

namespace Application.Forms.GetById;

public sealed record QuestionResponse
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public QuestionType Type { get; init; }
    public int Order { get; init; }
}
