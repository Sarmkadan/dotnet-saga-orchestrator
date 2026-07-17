#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SagaTimeoutException"/>.
/// </summary>
public static class SagaTimeoutExceptionJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="SagaTimeoutException"/> to a JSON string.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this SagaTimeoutException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return indented
            ? JsonSerializer.Serialize(value, new JsonSerializerOptions(_jsonOptions) { WriteIndented = true })
            : JsonSerializer.Serialize(value, _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SagaTimeoutException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized exception, or <see langword="null"/> if the JSON is <see langword="null"/>, empty, or invalid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static SagaTimeoutException? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrEmpty(json)
            ? null
            : SafeDeserialize(json);

        static SagaTimeoutException? SafeDeserialize(string jsonValue)
        {
            try
            {
                return JsonSerializer.Deserialize<SagaTimeoutException>(jsonValue, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaTimeoutException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized exception if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out SagaTimeoutException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<SagaTimeoutException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}