#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// JSON serialization helpers for <see cref="InMemoryCompensationTransactionRepository"/>.
/// </summary>
public static class InMemoryCompensationTransactionRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Serializes the <see cref="InMemoryCompensationTransactionRepository"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="InMemoryCompensationTransactionRepository"/> instance to serialize.</param>
    /// <param name="indented">Whether to indent the JSON output.</param>
    /// <returns>A JSON string representing the <see cref="InMemoryCompensationTransactionRepository"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this InMemoryCompensationTransactionRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? _jsonOptions : new JsonSerializerOptions(_jsonOptions) { WriteIndented = false };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="InMemoryCompensationTransactionRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="InMemoryCompensationTransactionRepository"/> instance deserialized from the JSON string, or null if deserialization fails.</returns>
    public static InMemoryCompensationTransactionRepository? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<InMemoryCompensationTransactionRepository>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to deserialize a JSON string to an <see cref="InMemoryCompensationTransactionRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="InMemoryCompensationTransactionRepository"/> instance, or null if deserialization fails.</param>
    /// <returns>true if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out InMemoryCompensationTransactionRepository? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<InMemoryCompensationTransactionRepository>(json, _jsonOptions);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}