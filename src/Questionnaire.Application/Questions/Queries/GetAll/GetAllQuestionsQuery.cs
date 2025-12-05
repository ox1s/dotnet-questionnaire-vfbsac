using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Questions.Common;

namespace Questionnaire.Application.Questions.Queries.GetAll;

public sealed record GetAllQuestionsQuery : IQuery<IEnumerable<QuestionResponse>>;