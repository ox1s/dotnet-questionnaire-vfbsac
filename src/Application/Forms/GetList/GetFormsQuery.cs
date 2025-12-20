using Application.Abstractions.Messaging;

namespace Application.Forms.GetList;

public sealed record GetFormsQuery(bool? IsActive = null) : IQuery<List<FormListItemResponse>>;
