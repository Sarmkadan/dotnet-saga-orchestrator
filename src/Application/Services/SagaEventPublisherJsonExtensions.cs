#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SagaEventPublisher"/> instances
/// </summary>
public static class SagaEventPublisherJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="SagaEventPublisher"/> instance to a JSON string
    /// </summary>
    /// <param name="value">The <see cref="SagaEventPublisher"/> instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the <see cref="SagaEventPublisher"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson(this SagaEventPublisher value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented
            ? new JsonSerializerOptions(JsonSerializerOptions) { WriteIndented = true }
            : JsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SagaEventPublisher"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A <see cref="SagaEventPublisher"/> instance, or null if the JSON is null, empty, or deserialization fails</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace</exception>
    public static SagaEventPublisher? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            return JsonSerializer.Deserialize<SagaEventPublisher>(json, JsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaEventPublisher"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The deserialized <see cref="SagaEventPublisher"/> instance, or null on failure</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace</exception>
    public static bool TryFromJson(string json, out SagaEventPublisher? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        value = null;

        try
        {
            value = JsonSerializer.Deserialize<SagaEventPublisher>(json, JsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}