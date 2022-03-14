#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace SagaOrchestrator.Infrastructure.Caching;

/// <summary>
/// JSON serialization and deserialization extensions for cache keys.
/// </summary>
public static class CacheKeyBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Converts a cache key to a JSON string.
    /// </summary>
    /// <param name="key">The cache key to convert.</param>
    /// <param name="indented">Whether the JSON should be formatted with indentation.</param>
    /// <returns>A JSON string representation of the cache key.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
    public static string ToJson(this string key, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(key);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        var cacheKey = new { Key = key };

        return JsonSerializer.Serialize(cacheKey, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a cache key.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized cache key or null if the JSON is invalid.</returns>
    public static string? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            var cacheKey = JsonSerializer.Deserialize<CacheKey>(json, JsonOptions);

            return cacheKey?.Key;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a cache key.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="key">The deserialized cache key or null if the JSON is invalid.</param>
    /// <returns>True if the JSON was successfully deserialized; otherwise, false.</returns>
    public static bool TryFromJson(string json, out string? key)
    {
        try
        {
            key = FromJson(json);
            return key != null;
        }
        catch (JsonException)
        {
            key = null;
            return false;
        }
    }
}

public class CacheKey
{
    public string Key { get; set; }

    public CacheKey(string key)
    {
        Key = key;
    }
}
