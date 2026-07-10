#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// =============================================================================

using System;
using System.Text.Json;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ValidationException"/>.
/// </summary>
public static class ValidationExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="ValidationException"/> to a JSON string.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the exception.</returns>
    public static string ToJson(this ValidationException value, bool indented = false)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="ValidationException"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized exception, or null if the JSON is null or empty.</returns>
    public static ValidationException? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="ValidationException"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized exception, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ValidationException? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}