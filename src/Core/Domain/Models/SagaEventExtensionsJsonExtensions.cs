#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SagaEventExtensions"/>.
/// </summary>
public static class SagaEventExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a <see cref="SagaEventExtensions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The SagaEventExtensions instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    /// <returns>A JSON string representation of the SagaEventExtensions</returns>
    public static string ToJson(this SagaEventExtensions value, bool indented = false)
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
    /// Deserializes a JSON string to a <see cref="SagaEventExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized</exception>
    /// <returns>The deserialized SagaEventExtensions instance, or null if JSON is null or empty</returns>
    public static SagaEventExtensions? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return string.IsNullOrEmpty(json)
            ? null
            : JsonSerializer.Deserialize<SagaEventExtensions>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaEventExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized value if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty</exception>
    public static bool TryFromJson(string json, out SagaEventExtensions? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<SagaEventExtensions>(json, _jsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}