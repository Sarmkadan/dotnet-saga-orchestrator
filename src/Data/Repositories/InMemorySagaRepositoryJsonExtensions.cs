#nullable enable
// =============================================================================
// Author: 
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// JSON serialization helpers for <see cref="InMemorySagaRepository"/>.
/// </summary>
public static class InMemorySagaRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Serializes the <see cref="InMemorySagaRepository"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="InMemorySagaRepository"/> instance to serialize.</param>
    /// <param name="indented">Whether to indent the JSON output.</param>
    /// <returns>A JSON string representing the <see cref="InMemorySagaRepository"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this InMemorySagaRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? _jsonOptions : new JsonSerializerOptions(_jsonOptions) { WriteIndented = false };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="InMemorySagaRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="InMemorySagaRepository"/> instance deserialized from the JSON string, or null if deserialization fails.</returns>
    public static InMemorySagaRepository? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<InMemorySagaRepository>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to deserialize a JSON string to an <see cref="InMemorySagaRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="InMemorySagaRepository"/> instance, or null if deserialization fails.</param>
    /// <returns>true if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out InMemorySagaRepository? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<InMemorySagaRepository>(json, _jsonOptions);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
