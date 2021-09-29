#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Infrastructure.Context;

/// <summary>
/// Request context for tracking saga operations across the request lifecycle.
/// Manages correlation IDs, user context, and performance metrics.
/// </summary>
public interface IRequestContext
{
    string CorrelationId { get; }
    string? UserId { get; set; }
    string? TenantId { get; set; }
    DateTime StartTime { get; }
    Dictionary<string, object> Metadata { get; }
    TimeSpan Elapsed { get; }
}

public class RequestContext : IRequestContext
{
    public string CorrelationId { get; }
    public string? UserId { get; set; }
    public string? TenantId { get; set; }
    public DateTime StartTime { get; }
    public Dictionary<string, object> Metadata { get; }

    public TimeSpan Elapsed => DateTime.UtcNow - StartTime;

    public RequestContext()
    {
        CorrelationId = Guid.NewGuid().ToString();
        StartTime = DateTime.UtcNow;
        Metadata = new();
    }

    public override string ToString() =>
        $"RequestId: {CorrelationId}, UserId: {UserId}, TenantId: {TenantId}, Elapsed: {Elapsed.TotalMilliseconds}ms";
}

/// <summary>
/// Scoped context provider for request context throughout the application.
/// </summary>
public interface IRequestContextProvider
{
    IRequestContext GetContext();
    void SetContext(IRequestContext context);
}

public class RequestContextProvider : IRequestContextProvider
{
    private static readonly AsyncLocal<IRequestContext> _context = new();

    public IRequestContext GetContext()
    {
        return _context.Value ?? new RequestContext();
    }

    public void SetContext(IRequestContext context)
    {
        _context.Value = context ?? throw new ArgumentNullException(nameof(context));
    }
}

/// <summary>
/// Performance tracking for request execution.
/// </summary>
public class PerformanceTracker
{
    private readonly Dictionary<string, long> _timings;
    private readonly object _lock = new();

    public PerformanceTracker()
    {
        _timings = new();
    }

    public void RecordTiming(string operationName, long elapsedMs)
    {
        lock (_lock)
        {
            _timings[operationName] = elapsedMs;
        }
    }

    public long? GetTiming(string operationName)
    {
        lock (_lock)
        {
            _timings.TryGetValue(operationName, out var timing);
            return timing > 0 ? timing : null;
        }
    }

    public Dictionary<string, long> GetAllTimings()
    {
        lock (_lock)
        {
            return new Dictionary<string, long>(_timings);
        }
    }

    public long GetTotalElapsedMs()
    {
        lock (_lock)
        {
            return _timings.Values.Sum();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _timings.Clear();
        }
    }

    public override string ToString()
    {
        var timings = GetAllTimings();
        if (timings.Count == 0)
            return "No timings recorded";

        var lines = timings.Select(kvp => $"  {kvp.Key}: {kvp.Value}ms");
        var total = GetTotalElapsedMs();
        return $"Performance Timings (Total: {total}ms):\n{string.Join("\n", lines)}";
    }
}
