using Application.Abstractions.Messaging;

namespace Application.Teachers.GetList;

public sealed record TeacherResponse(Guid Id, string FullName, Guid DepartmentId);

public sealed record GetTeachersQuery() : IQuery<List<TeacherResponse>>;
