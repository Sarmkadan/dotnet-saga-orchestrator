#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    /// Converts a <see cref="CacheKeyBuilder"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="CacheKeyBuilder"/> to convert.</param>
    /// <param name="indented">Whether the JSON should be formatted with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="CacheKeyBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this CacheKeyBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CacheKeyBuilder"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="CacheKeyBuilder"/> or null if the JSON is invalid.</returns>
    public static CacheKeyBuilder? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CacheKeyBuilder>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="CacheKeyBuilder"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="CacheKeyBuilder"/> or null if the JSON is invalid.</param>
    /// <returns>True if the JSON was successfully deserialized; otherwise, false.</returns>
    public static bool TryFromJson(string json, out CacheKeyBuilder? value)
    {
        try
        {
            value = FromJson(json);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
