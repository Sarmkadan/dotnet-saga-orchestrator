#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SagaDefinitionService"/>.
/// </summary>
public static class SagaDefinitionServiceExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes a <see cref="SagaDefinitionService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="SagaDefinitionService"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="SagaDefinitionService"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this SagaDefinitionService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SagaDefinitionService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="SagaDefinitionService"/> instance, or <see langword="null"/> if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static SagaDefinitionService? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            return JsonSerializer.Deserialize<SagaDefinitionService>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaDefinitionService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="SagaDefinitionService"/> instance if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or whitespace.</exception>
    public static bool TryFromJson(string json, out SagaDefinitionService? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        value = null;

        try
        {
            value = JsonSerializer.Deserialize<SagaDefinitionService>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}