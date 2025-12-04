using Questionnaire.Application.Abstractions.Messaging;

namespace Questionnaire.Application.Reports.Queries.Export;

public sealed record ExportReportQuery(int FormId) : IQuery<byte[]>;