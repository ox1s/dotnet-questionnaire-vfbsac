using Application.Abstractions.Messaging;
using Application.Reports.Queries.Shared;

namespace Application.Reports.Queries.GetAnalyticsByGroups;

public enum GroupingType
{
    Department,
    Discipline,
    Speciality,
    Specialization,
    EducationForm,
    EmployeeCategory,
    Teacher
}

public sealed record GetAnalyticsByGroupsQuery(
    Guid FormId,
    DateTime FromDate,
    DateTime ToDate,
    GroupingType GroupBy,
    AnalyticsFilterSet FilterSet)
    : IQuery<List<GetAnalyticsByGroupsQueryResponse>>;
