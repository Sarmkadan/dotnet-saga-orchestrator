#nullable enable
// =============================================================================
// Author: 
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="DateTimeExtensions"/>.
/// </summary>
public static class DateTimeExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="DateTimeExtensions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="DateTimeExtensions"/> to serialize.</param>
    /// <param name="indented">If true, the JSON will be formatted with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="DateTimeExtensions"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this DateTime value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = indented ? new(JsonSerializerOptions) { WriteIndented = true } : JsonSerializerOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="DateTime"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static DateTime? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<DateTime>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="DateTime"/> if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out DateTime? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<DateTime>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
