#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="Saga"/>.
/// </summary>
public static class SagaOrchestrationServiceJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Serializes the <see cref="Saga"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The saga instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <returns>A JSON string representation of the saga.</returns>
    public static string ToJson(this Saga value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true,
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="Saga"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or whitespace.</exception>
    /// <returns>The deserialized saga instance, or null if the JSON is invalid.</returns>
    public static Saga? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            return JsonSerializer.Deserialize<Saga>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="Saga"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized saga instance, or null if deserialization failed.</param>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or whitespace.</exception>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out Saga? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        value = null;

        try
        {
            value = JsonSerializer.Deserialize<Saga>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
