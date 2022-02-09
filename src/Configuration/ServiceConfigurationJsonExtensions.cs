#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="SagaOptions"/>.
/// </summary>
public static class ServiceConfigurationJsonExtensions
{
    /// <summary>
    /// Configured JSON serializer options with camelCase property naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes <see cref="SagaOptions"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="SagaOptions"/> instance to serialize.</param>
    /// <param name="indented">If true, formats the JSON with indentation.</param>
    /// <returns>JSON string representation of the <see cref="SagaOptions"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this SagaOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(JsonSerializerOptions);
        options.WriteIndented = indented;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SagaOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="SagaOptions"/> instance populated from the JSON.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized.</exception>
    public static SagaOptions? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<SagaOptions>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="SagaOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="SagaOptions"/> instance, or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out SagaOptions? value)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            value = JsonSerializer.Deserialize<SagaOptions>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}