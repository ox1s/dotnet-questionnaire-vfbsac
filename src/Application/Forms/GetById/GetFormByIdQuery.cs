using Application.Abstractions.Messaging;

namespace Application.Forms.GetById;

public sealed record GetFormByIdQuery(Guid FormId) : IQuery<FormResponse>;
