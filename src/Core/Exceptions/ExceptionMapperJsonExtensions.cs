#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="ErrorResponse"/>.
/// </summary>
public static class ExceptionMapperJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an <see cref="ErrorResponse"/> instance to a JSON string using camelCase property naming.
    /// </summary>
    /// <param name="value">The <see cref="ErrorResponse"/> to serialize.</param>
    /// <param name="indented">If true, the JSON will be formatted with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="ErrorResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this ErrorResponse value, bool indented = false) =>
        JsonSerializer.Serialize(value, indented ? new(JsonSerializerOptions) { WriteIndented = true } : JsonSerializerOptions);

    /// <summary>
    /// Deserializes a JSON string into an <see cref="ErrorResponse"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="ErrorResponse"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static ErrorResponse FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ErrorResponse>(json, JsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize ErrorResponse from JSON.");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="ErrorResponse"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="ErrorResponse"/> if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty.</exception>
    public static bool TryFromJson(string json, out ErrorResponse? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ErrorResponse>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
