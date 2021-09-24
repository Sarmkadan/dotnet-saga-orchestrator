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

    public static string BuildSagaKey(string sagaId) =>
        $"saga{Delimiter}{sagaId}";

    public static string BuildDefinitionKey(string definitionId) =>
        $"definition{Delimiter}{definitionId}";

    public static string BuildAllSagasKey() =>
        $"sagas{Delimiter}all";

    public static string BuildAllDefinitionsKey() =>
        $"definitions{Delimiter}all";

    public static string BuildSagasByStatusKey(string status) =>
        $"sagas{Delimiter}status{Delimiter}{status}";

    public static string BuildDefinitionByNameKey(string name) =>
        $"definitions{Delimiter}name{Delimiter}{name}";

    public static string BuildCompensationKey(string sagaId) =>
        $"compensation{Delimiter}{sagaId}";

    public static string BuildEventHistoryKey(string sagaId) =>
        $"events{Delimiter}{sagaId}";

    public static string BuildServiceKey(string serviceName) =>
        $"service{Delimiter}{serviceName}";

    public static string BuildHealthCheckKey() =>
        "health{Delimiter}check";

    public static string BuildMetricsKey() =>
        "metrics";

    public static string BuildWebhookKey(string webhookId) =>
        $"webhook{Delimiter}{webhookId}";

    public static string BuildRateLimitKey(string identifier, string resource) =>
        $"ratelimit{Delimiter}{identifier}{Delimiter}{resource}";

    public static string BuildUserCacheKey(string userId) =>
        $"user{Delimiter}{userId}";

    public static string BuildSessionKey(string sessionId) =>
        $"session{Delimiter}{sessionId}";

    public static string GenerateTempKey() =>
        $"temp{Delimiter}{Guid.NewGuid()}";

    public static bool IsSagaKey(string key) =>
        key.StartsWith($"saga{Delimiter}");

    public static bool IsDefinitionKey(string key) =>
        key.StartsWith($"definition{Delimiter}");

    public static string ExtractIdFromKey(string key) =>
        key.Contains(Delimiter) ? key.Split(Delimiter).Last() : key;

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
