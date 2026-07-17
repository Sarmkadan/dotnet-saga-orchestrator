#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Infrastructure.Telemetry;

/// <summary>
/// Provides System.Text.Json serialization extensions for activity source telemetry data.
/// </summary>
public static class SagaActivitySourceExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes an activity source telemetry object to a JSON string.
    /// </summary>
    /// <param name="value">The activity source telemetry object to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the activity source telemetry object.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an activity source telemetry object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized activity source telemetry object, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to deserialize JSON string to activity source telemetry object.", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an activity source telemetry object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized activity source telemetry object, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}