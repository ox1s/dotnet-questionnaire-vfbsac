using Questionnaire.Domain.Questions;

namespace Questionnaire.Domain.Answers;

public class AnswerDetailSelectedOption
{
    public int AnswerDetailId { get; set; }
    public AnswerDetail AnswerDetail { get; set; } = null!;

    public int QuestionOptionId { get; set; }
    public QuestionOption QuestionOption { get; set; } = null!;
}
