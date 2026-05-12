using Application.Reports.Queries.Shared;

namespace Application.Reports.Queries.GetAnalyticsByGroups;

public sealed record GetAnalyticsByGroupsQueryResponse(
    string GroupKey,
    string GroupName,
    List<QuestionStatistics> QuestionStatistics);
