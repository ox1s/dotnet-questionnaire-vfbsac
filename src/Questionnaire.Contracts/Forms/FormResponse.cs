using Questionnaire.Contracts.Questions;

namespace Questionnaire.Contracts.Forms;

public record FormResponse(
    int Id,
    string Name,
    bool IsActive,
    List<QuestionResponse>? Questions);