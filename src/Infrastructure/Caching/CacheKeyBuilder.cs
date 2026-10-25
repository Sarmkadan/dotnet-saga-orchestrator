#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Infrastructure.Caching;

/// <summary>
/// Cache key builder for standardized cache key generation.
/// Ensures consistent key format across the application for cache operations.
/// </summary>
public static class CacheKeyBuilder
{
    private const string Delimiter = ":";

    private const string SagaPrefix = "saga";
    private const string DefinitionPrefix = "definition";
    private const string SagasPrefix = "sagas";
    private const string DefinitionsPrefix = "definitions";
    private const string CompensationPrefix = "compensation";
    private const string EventsPrefix = "events";
    private const string ServicePrefix = "service";
    private const string HealthPrefix = "health";
    private const string MetricsPrefix = "metrics";
    private const string WebhookPrefix = "webhook";
    private const string RateLimitPrefix = "ratelimit";
    private const string UserPrefix = "user";
    private const string SessionPrefix = "session";
    private const string TempPrefix = "temp";

    private const string AllSegment = "all";
    private const string StatusSegment = "status";
    private const string NameSegment = "name";
    private const string CheckSegment = "check";

    public static string BuildSagaKey(string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);
        return $"{SagaPrefix}{Delimiter}{sagaId}";
    }

    public static string BuildDefinitionKey(string definitionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(definitionId);
        return $"{DefinitionPrefix}{Delimiter}{definitionId}";
    }

    public static string BuildAllSagasKey() =>
        $"{SagasPrefix}{Delimiter}{AllSegment}";

    public static string BuildAllDefinitionsKey() =>
        $"{DefinitionsPrefix}{Delimiter}{AllSegment}";

    public static string BuildSagasByStatusKey(string status)
    {
        ArgumentException.ThrowIfNullOrEmpty(status);
        return $"{SagasPrefix}{Delimiter}{StatusSegment}{Delimiter}{status}";
    }

    public static string BuildDefinitionByNameKey(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return $"{DefinitionsPrefix}{Delimiter}{NameSegment}{Delimiter}{name}";
    }

    public static string BuildCompensationKey(string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);
        return $"{CompensationPrefix}{Delimiter}{sagaId}";
    }

    public static string BuildEventHistoryKey(string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);
        return $"{EventsPrefix}{Delimiter}{sagaId}";
    }

    public static string BuildServiceKey(string serviceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        return $"{ServicePrefix}{Delimiter}{serviceName}";
    }

    public static string BuildHealthCheckKey() =>
        $"{HealthPrefix}{Delimiter}{CheckSegment}";

    public static string BuildMetricsKey() =>
        MetricsPrefix;

    public static string BuildWebhookKey(string webhookId)
    {
        ArgumentException.ThrowIfNullOrEmpty(webhookId);
        return $"{WebhookPrefix}{Delimiter}{webhookId}";
    }

    public static string BuildRateLimitKey(string identifier, string resource)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        ArgumentException.ThrowIfNullOrEmpty(resource);
        return $"{RateLimitPrefix}{Delimiter}{identifier}{Delimiter}{resource}";
    }

    public static string BuildUserCacheKey(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        return $"{UserPrefix}{Delimiter}{userId}";
    }

    public static string BuildSessionKey(string sessionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        return $"{SessionPrefix}{Delimiter}{sessionId}";
    }

    public static string GenerateTempKey() =>
        $"{TempPrefix}{Delimiter}{Guid.NewGuid()}";

    public static bool IsSagaKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return key.StartsWith($"{SagaPrefix}{Delimiter}");
    }

    public static bool IsDefinitionKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return key.StartsWith($"{DefinitionPrefix}{Delimiter}");
    }

    public static string ExtractIdFromKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return key.Contains(Delimiter) ? key.Split(Delimiter).Last() : key;
    }

    public static Dictionary<string, string> GetAllPrefixes() =>
        new()
        {
            [SagaPrefix] = $"{SagaPrefix}{Delimiter}",
            [DefinitionPrefix] = $"{DefinitionPrefix}{Delimiter}",
            [SagasPrefix] = $"{SagasPrefix}{Delimiter}",
            [DefinitionsPrefix] = $"{DefinitionsPrefix}{Delimiter}",
            [CompensationPrefix] = $"{CompensationPrefix}{Delimiter}",
            [EventsPrefix] = $"{EventsPrefix}{Delimiter}",
            [ServicePrefix] = $"{ServicePrefix}{Delimiter}",
            [HealthPrefix] = $"{HealthPrefix}{Delimiter}",
            [MetricsPrefix] = MetricsPrefix,
            [WebhookPrefix] = $"{WebhookPrefix}{Delimiter}",
            [RateLimitPrefix] = $"{RateLimitPrefix}{Delimiter}",
            [UserPrefix] = $"{UserPrefix}{Delimiter}",
            [SessionPrefix] = $"{SessionPrefix}{Delimiter}",
            [TempPrefix] = $"{TempPrefix}{Delimiter}"
        };
}

/// <summary>
/// Cache expiration time constants.
/// </summary>
public static class CacheExpiration
{
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan Long = TimeSpan.FromHours(1);
    public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);

    public static TimeSpan GetExpiration(string cacheType) => cacheType switch
    {
        "saga" => Medium,
        "definition" => Long,
        "metrics" => Short,
        "healthcheck" => Short,
        "webhook" => Long,
        _ => Medium
    };
}
