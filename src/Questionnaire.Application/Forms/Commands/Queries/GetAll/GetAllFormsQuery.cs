using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Contracts.Forms;

namespace Questionnaire.Application.Forms.Queries.GetAll;

public sealed record GetAllFormsQuery : IQuery<IEnumerable<FormResponse>>;