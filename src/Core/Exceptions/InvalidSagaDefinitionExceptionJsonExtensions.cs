#nullable enable
using System;
using System.Text.Json;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="InvalidSagaDefinitionException"/>.
/// </summary>
public static class InvalidSagaDefinitionExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Preserve case‑insensitive matching for robustness.
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the <see cref="InvalidSagaDefinitionException"/> to a JSON string.
    /// </summary>
    /// <param name="value">The exception instance to serialize.</param>
    /// <param name="indented">
    /// If <c>true</c>, the resulting JSON will be formatted with indentation; otherwise it will be compact.
    /// </param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this InvalidSagaDefinitionException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // If indentation is requested, clone the shared options and enable WriteIndented.
        var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="InvalidSagaDefinitionException"/> instance.
    /// </summary>
    /// <param name="json">The JSON representation of the exception.</param>
    /// <returns>The deserialized <see cref="InvalidSagaDefinitionException"/>, or <c>null</c> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static InvalidSagaDefinitionException? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<InvalidSagaDefinitionException>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="InvalidSagaDefinitionException"/> instance.
    /// </summary>
    /// <param name="json">The JSON representation of the exception.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="InvalidSagaDefinitionException"/> if the operation succeeded;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out InvalidSagaDefinitionException? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
