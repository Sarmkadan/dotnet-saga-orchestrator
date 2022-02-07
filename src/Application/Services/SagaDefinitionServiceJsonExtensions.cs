using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides JSON serialization and deserialization helpers for the <see cref="SagaDefinitionService"/> class.
/// </summary>
public static class SagaDefinitionServiceJsonExtensions
{
    /// <summary>
    /// Configured JSON serializer options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="SagaDefinitionService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="SagaDefinitionService"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="SagaDefinitionService"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SagaDefinitionService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(JsonOptions)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SagaDefinitionService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="SagaDefinitionService"/> instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
    public static SagaDefinitionService? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<SagaDefinitionService>(json, JsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaDefinitionService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="SagaDefinitionService"/> instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out SagaDefinitionService? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<SagaDefinitionService>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
