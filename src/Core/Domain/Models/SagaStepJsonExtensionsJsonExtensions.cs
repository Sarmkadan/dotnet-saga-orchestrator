#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for SagaStep.
/// </summary>
public static class SagaStepJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Serializes the SagaStep instance to a JSON string.
    /// </summary>
    /// <param name="value">The SagaStep instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the SagaStep, or null if the input is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the value cannot be serialized.</exception>
    public static string? ToJson(this SagaStep? value, bool indented = false)
    {
        if (value is null)
        {
            return null;
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a SagaStep instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A SagaStep instance if deserialization succeeds, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static SagaStep? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<SagaStep>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a SagaStep instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized SagaStep instance if successful, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
    public static bool TryFromJson(string json, out SagaStep? value)
    {
        value = default;

        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<SagaStep>(json, _jsonSerializerOptions);
            return true;
        }
        catch
        {
            return false;
        }
    }
}