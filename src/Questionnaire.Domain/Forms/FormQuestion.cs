using Questionnaire.Domain.Questions;

namespace Questionnaire.Domain.Forms;

public class FormQuestion
{
    public int FormId { get; set; }
    public Form Form { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public int Order { get; set; }
}
