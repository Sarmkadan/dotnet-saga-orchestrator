using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides extension methods for serializing and deserializing <see cref="SagaStepExecutionException"/> instances to and from JSON.
/// </summary>
public static class SagaStepExecutionExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <paramref name="value"/> to a JSON string, optionally with indentation.
    /// </summary>
    /// <param name="value">The <see cref="SagaStepExecutionException"/> instance to serialize.</param>
    /// <param name="indented">Whether to include indentation in the JSON output.</param>
    /// <returns>The JSON representation of the <paramref name="value"/>.</returns>
    public static string ToJson(this SagaStepExecutionException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (indented)
        {
            _jsonSerializerOptions.WriteIndented = true;
        }

        return JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaStepExecutionException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="SagaStepExecutionException"/> instance, or <c>null</c> if deserialization fails.</returns>
    public static SagaStepExecutionException? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<SagaStepExecutionException>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaStepExecutionException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="SagaStepExecutionException"/> instance, or <c>null</c> if deserialization fails.</param>
    /// <returns><c>true</c> if deserialization is successful, <c>false</c> otherwise.</returns>
    public static bool TryFromJson(string json, out SagaStepExecutionException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<SagaStepExecutionException>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
