using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides JSON serialization and deserialization helpers for the <see cref="CompensationService"/> class.
/// </summary>
/// <remarks>
/// This class cannot be inherited.
/// </remarks>
public static class CompensationServiceJsonExtensions
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
    /// Serializes a <see cref="CompensationService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="CompensationService"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="CompensationService"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CompensationService value, bool indented = false) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonOptions) { WriteIndented = indented });

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CompensationService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="CompensationService"/> instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when <paramref name="json"/> is not valid JSON or cannot be deserialized to <see cref="CompensationService"/>.</exception>
    public static CompensationService? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<CompensationService>(json, JsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="CompensationService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="CompensationService"/> instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    public static bool TryFromJson(string json, out CompensationService? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<CompensationService>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}