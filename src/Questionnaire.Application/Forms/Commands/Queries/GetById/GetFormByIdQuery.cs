using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Contracts.Forms;

namespace Questionnaire.Application.Forms.Queries.GetById;

public sealed record GetFormByIdQuery(int Id) : IQuery<FormResponse>;