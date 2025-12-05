using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Forms.Common;

namespace Questionnaire.Application.Forms.Queries.GetById;

public sealed record GetFormByIdQuery(int Id) : IQuery<FormResponse>;
