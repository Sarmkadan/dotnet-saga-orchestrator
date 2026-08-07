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

    public static string BuildSagaKey(string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(sagaId));
        return $"saga{Delimiter}{sagaId}";
    }

    public static string BuildDefinitionKey(string definitionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(definitionId));
        return $"definition{Delimiter}{definitionId}";
    }

    public static string BuildAllSagasKey() =>
        $"sagas{Delimiter}all";

    public static string BuildAllDefinitionsKey() =>
        $"definitions{Delimiter}all";

    public static string BuildSagasByStatusKey(string status)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(status));
        return $"sagas{Delimiter}status{Delimiter}{status}";
    }

    public static string BuildDefinitionByNameKey(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(name));
        return $"definitions{Delimiter}name{Delimiter}{name}";
    }

    public static string BuildCompensationKey(string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(sagaId));
        return $"compensation{Delimiter}{sagaId}";
    }

    public static string BuildEventHistoryKey(string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(sagaId));
        return $"events{Delimiter}{sagaId}";
    }

    public static string BuildServiceKey(string serviceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(serviceName));
        return $"service{Delimiter}{serviceName}";
    }

    public static string BuildHealthCheckKey() =>
        "health{Delimiter}check";

    public static string BuildMetricsKey() =>
        "metrics";

    public static string BuildWebhookKey(string webhookId)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(webhookId));
        return $"webhook{Delimiter}{webhookId}";
    }

    public static string BuildRateLimitKey(string identifier, string resource)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(identifier));
        ArgumentException.ThrowIfNullOrEmpty(nameof(resource));
        return $"ratelimit{Delimiter}{identifier}{Delimiter}{resource}";
    }

    public static string BuildUserCacheKey(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(userId));
        return $"user{Delimiter}{userId}";
    }

    public static string BuildSessionKey(string sessionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(sessionId));
        return $"session{Delimiter}{sessionId}";
    }

    public static string GenerateTempKey() =>
        $"temp{Delimiter}{Guid.NewGuid()}";

    public static bool IsSagaKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(key));
        return key.StartsWith($"saga{Delimiter}");
    }

    public static bool IsDefinitionKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(key));
        return key.StartsWith($"definition{Delimiter}");
    }

    public static string ExtractIdFromKey(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(key));
        return key.Contains(Delimiter) ? key.Split(Delimiter).Last() : key;
    }

    public static Dictionary<string, string> GetAllPrefixes() =>
        new()
        {
            ["saga"] = "saga:",
            ["definition"] = "definition:",
            ["sagas"] = "sagas:",
            ["definitions"] = "definitions:",
            ["compensation"] = "compensation:",
            ["events"] = "events:",
            ["service"] = "service:",
            ["health"] = "health:",
            ["metrics"] = "metrics",
            ["webhook"] = "webhook:",
            ["ratelimit"] = "ratelimit:",
            ["user"] = "user:",
            ["session"] = "session:",
            ["temp"] = "temp:"
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