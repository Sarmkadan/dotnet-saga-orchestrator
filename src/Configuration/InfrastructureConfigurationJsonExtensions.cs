using System.Text.Json;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="InfrastructureConfiguration"/>.
/// </summary>
public static class InfrastructureConfigurationJsonExtensions
{
    /// <summary>
    /// Configured JSON serializer options with camelCase property naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an <see cref="InfrastructureConfiguration"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="InfrastructureConfiguration"/> instance to serialize.</param>
    /// <param name="indented">If true, formats the JSON with indentation.</param>
    /// <returns>JSON string representation of the <see cref="InfrastructureConfiguration"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this InfrastructureConfiguration value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="InfrastructureConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="InfrastructureConfiguration"/> instance populated from the JSON.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized.</exception>
    public static InfrastructureConfiguration? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<InfrastructureConfiguration>(json, _jsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to an <see cref="InfrastructureConfiguration"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="InfrastructureConfiguration"/> instance, or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out InfrastructureConfiguration? value)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(json);
            value = JsonSerializer.Deserialize<InfrastructureConfiguration>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}