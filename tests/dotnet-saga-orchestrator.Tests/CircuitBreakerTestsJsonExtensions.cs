#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides System.Text.Json serialization extensions for CircuitBreakerTests.
/// </summary>
public static class CircuitBreakerTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a CircuitBreakerTests instance to JSON.
    /// </summary>
    /// <param name="value">The CircuitBreakerTests instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the CircuitBreakerTests instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this CircuitBreakerTests value, bool indented = false) =>
        JsonSerializer.Serialize(value, indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions);

    /// <summary>
    /// Deserializes a JSON string to a CircuitBreakerTests instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A CircuitBreakerTests instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static CircuitBreakerTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<CircuitBreakerTests>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a CircuitBreakerTests instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized CircuitBreakerTests instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out CircuitBreakerTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        return !string.IsNullOrWhiteSpace(json)
            && (value = JsonSerializer.Deserialize<CircuitBreakerTests>(json, _jsonSerializerOptions)) is not null;
    }
}