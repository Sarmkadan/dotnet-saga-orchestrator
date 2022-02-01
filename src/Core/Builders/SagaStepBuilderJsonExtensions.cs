#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Core.Builders;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="SagaStepBuilder"/>.
/// </summary>
public static class SagaStepBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="SagaStepBuilder"/> to a JSON string.
    /// </summary>
    /// <param name="value">The builder instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SagaStepBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value.Build(), options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SagaStepBuilder"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="SagaStepBuilder"/> instance, or null if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static SagaStepBuilder? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        try
        {
            var stepDefinition = JsonSerializer.Deserialize<SagaStepDefinition>(json, _jsonOptions);
            return stepDefinition == null
                ? null
                : SagaStepBuilder.Create(stepDefinition.Name, stepDefinition.ServiceName, stepDefinition.ServiceUrl)
                    .WithOrder(stepDefinition.Order)
                    .WithTimeout(stepDefinition.TimeoutSeconds)
                    .WithRetryPolicy(stepDefinition.MaxRetries, stepDefinition.RetryDelayMilliseconds);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaStepBuilder"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The resulting builder instance, or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out SagaStepBuilder? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        try
        {
            value = FromJson(json);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}