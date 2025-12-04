using System.Diagnostics;
using Serilog.Context;

namespace Questionnaire.Api.Middleware;

internal sealed class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = GetOrGenerateCorrelationId(context);

        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }
}
