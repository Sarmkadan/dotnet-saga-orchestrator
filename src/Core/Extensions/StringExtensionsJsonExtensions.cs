#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for string extensions.
/// </summary>
public static class StringExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a string to a JSON string.
    /// </summary>
    /// <param name="value">The string to serialize.</param>
    /// <param name="indented">If true, the JSON will be formatted with indentation.</param>
    /// <returns>A JSON string representation of the string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this string value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = indented ? new(JsonSerializerOptions) { WriteIndented = true } : JsonSerializerOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A string instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static string FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<string>(json, JsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize string from JSON.");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized string if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out string? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<string>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
