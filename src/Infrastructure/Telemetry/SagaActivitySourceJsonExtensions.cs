#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Text.Json;

namespace SagaOrchestrator.Infrastructure.Telemetry;

/// <summary>
/// Provides System.Text.Json serialization helpers for SagaActivitySource telemetry data.
/// </summary>
public static class SagaActivitySourceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes SagaActivitySource telemetry data to a JSON string.
    /// </summary>
    /// <param name="value">The SagaActivitySource.Name value to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the SagaActivitySource telemetry data.</returns>
    public static string ToJson(string value, bool indented = false)
    {
        if (value == null)
        {
            return "null";
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        var data = new { name = value };
        return JsonSerializer.Serialize(data, options);
    }

    /// <summary>
    /// Deserializes a JSON string to SagaActivitySource telemetry data.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A SagaActivitySource telemetry data object, or null if the JSON is null or empty.</returns>
    public static SagaActivitySourceTelemetry? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return null;
        }

        return JsonSerializer.Deserialize<SagaActivitySourceTelemetry>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to SagaActivitySource telemetry data.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized SagaActivitySource telemetry data, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out SagaActivitySourceTelemetry? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<SagaActivitySourceTelemetry>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Telemetry data transfer object for SagaActivitySource serialization.
    /// </summary>
    public sealed class SagaActivitySourceTelemetry
    {
        public string? Name { get; set; }
    }
}