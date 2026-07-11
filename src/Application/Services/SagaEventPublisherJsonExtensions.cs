#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for SagaEventPublisher
/// </summary>
public static class SagaEventPublisherJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the SagaEventPublisher instance to a JSON string
    /// </summary>
    /// <param name="value">The SagaEventPublisher instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the SagaEventPublisher</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson(this SagaEventPublisher value, bool indented = false) =>
        JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(JsonSerializerOptions) { WriteIndented = true } : JsonSerializerOptions);

    /// <summary>
    /// Deserializes a JSON string to a SagaEventPublisher instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A SagaEventPublisher instance, or null if the JSON is empty or deserialization fails</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized</exception>
    public static SagaEventPublisher? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

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
    /// Attempts to deserialize a JSON string to a SagaEventPublisher instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The deserialized SagaEventPublisher instance, or null on failure</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized</exception>
    public static bool TryFromJson(string json, out SagaEventPublisher? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

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