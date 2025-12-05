using Questionnaire.Application.Questions.Common;

namespace Questionnaire.Application.Forms.Common;

public record FormResponse(
    int Id,
    string Name,
    bool IsActive,
    List<QuestionResponse>? Questions);
