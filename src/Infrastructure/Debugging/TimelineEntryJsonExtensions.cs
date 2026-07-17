#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Infrastructure.Debugging;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="TimelineEntry"/>.
/// </summary>
public static class TimelineEntryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static JsonSerializerOptions GetSerializerOptions(bool indented) => new(_jsonSerializerOptions)
    {
        WriteIndented = indented
    };

    /// <summary>
    /// Serializes a <see cref="TimelineEntry"/> to a JSON string.
    /// </summary>
    /// <param name="value">The timeline entry to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON representation of the timeline entry.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this TimelineEntry value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, GetSerializerOptions(indented));
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="TimelineEntry"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized timeline entry, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized to a <see cref="TimelineEntry"/>.</exception>
    public static TimelineEntry? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<TimelineEntry>(json, GetSerializerOptions(false));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="TimelineEntry"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized timeline entry if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized to a <see cref="TimelineEntry"/>.</exception>
    public static bool TryFromJson(string json, out TimelineEntry? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<TimelineEntry>(json, GetSerializerOptions(false));
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}