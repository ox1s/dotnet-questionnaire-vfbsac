using Application.Abstractions.Messaging;
using Domain.User;

namespace Application.Forms.GetList;

/// <summary>
/// CallerRole scopes results to forms with no TargetRole (general) or a TargetRole
/// matching the caller. Leave null to bypass audience filtering (used by the
/// admin-only listing, which must show every form regardless of its audience).
/// </summary>
public sealed record GetFormsQuery(bool? IsActive = null, UserRole? CallerRole = null) : IQuery<List<GetFormsQueryResponse>>;
