#nullable enable

namespace SagaOrchestrator.Infrastructure.Context;

/// <summary>
/// Extension methods for RequestContext.
/// </summary>
public static class RequestContextExtensions
{
    /// <summary>
    /// Sets the correlation ID and returns the context for chaining.
    /// </summary>
    public static RequestContext WithCorrelationId(this RequestContext context, string correlationId)
    {
        context.CorrelationId = correlationId;
        return context;
    }

    /// <summary>
    /// Gets the elapsed time since the context was created.
    /// </summary>
    public static TimeSpan GetElapsed(this RequestContext context)
    {
        return context.Elapsed;
    }

    /// <summary>
    /// Converts the context to a dictionary for structured logging.
    /// </summary>
    public static Dictionary<string, string> ToLogDictionary(this RequestContext context)
    {
        return new Dictionary<string, string>
        {
            ["CorrelationId"] = context.CorrelationId,
            ["UserId"] = context.UserId ?? string.Empty,
            ["TenantId"] = context.TenantId ?? string.Empty,
            ["StartTime"] = context.StartTime.ToString("o"),
            ["Elapsed"] = context.Elapsed.ToString()
        };
    }
}