using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Contracts.Questions;

namespace Questionnaire.Application.Questions.Queries.GetAll;

public sealed record GetAllQuestionsQuery : IQuery<IEnumerable<QuestionResponse>>;