using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Forms.Common;

namespace Questionnaire.Application.Forms.Queries.GetAll;

public sealed record GetAllFormsQuery : IQuery<IEnumerable<FormResponse>>;
