#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SagaOrchestrator.Infrastructure.Debugging;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SagaDebuggerService"/> instances.
/// Enables easy JSON serialization and deserialization of saga debugger state for
/// debugging, logging, validation, and transport scenarios.
/// </summary>
/// <remarks>
/// This class is designed to be used with <see cref="SagaDebuggerService"/> instances
/// that contain saga debugging state including snapshots, breakpoints, and timeline data.
/// </remarks>
public static class SagaDebuggerServiceValidationJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = CreateJsonSerializerOptions();

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        return options;
    }

    /// <summary>
    /// Serializes the <see cref="SagaDebuggerService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The debugger service instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON representation of the debugger service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SagaDebuggerService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="SagaDebuggerService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized debugger service instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    /// <exception cref="FormatException">Thrown when the JSON string cannot be deserialized into a <see cref="SagaDebuggerService"/> instance.</exception>
    public static SagaDebuggerService FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        try
        {
            return JsonSerializer.Deserialize<SagaDebuggerService>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new FormatException("Failed to deserialize JSON string into SagaDebuggerService.", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="SagaDebuggerService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized debugger service instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    public static bool TryFromJson(string json, out SagaDebuggerService? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        value = null;

        try
        {
            value = JsonSerializer.Deserialize<SagaDebuggerService>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
