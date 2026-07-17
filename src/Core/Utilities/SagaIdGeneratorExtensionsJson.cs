#nullable enable

// =====================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for saga identifiers.
/// </summary>
internal static class SagaIdGeneratorExtensionsJson
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a string value to JSON as a saga identifier.
    /// </summary>
    /// <param name="value">The string value to serialize as a saga identifier.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the saga identifier.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this string value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(new { SagaId = value }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a saga identifier string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A saga identifier string, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Expected JSON format: { "sagaId": "saga_<guid>" }
    /// </remarks>
    public static string? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var result = JsonSerializer.Deserialize<SagaIdModel>(json, _jsonSerializerOptions);
            return result?.SagaId;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a saga identifier string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized saga identifier string, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Expected JSON format: { "sagaId": "saga_<guid>" }
    /// </remarks>
    public static bool TryFromJson(string json, out string? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            var result = JsonSerializer.Deserialize<SagaIdModel>(json, _jsonSerializerOptions);
            value = result?.SagaId;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed class SagaIdModel
    {
        public string? SagaId { get; init; }
    }
}
