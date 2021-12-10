#nullable enable

using System.Text.Json;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="DebuggerOptions"/>.
/// </summary>
public static class DebuggerOptionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Serializes the <see cref="DebuggerOptions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The options to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the options.</returns>
    public static string ToJson(this DebuggerOptions value, bool indented = false)
    {
        if (value is null)
        {
            return "{}";
        }

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="DebuggerOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized options, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static DebuggerOptions? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DebuggerOptions>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="DebuggerOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized options if successful, otherwise null.</param>
    /// <returns>True if deserialization succeeded; false if an exception occurred.</returns>
    public static bool TryFromJson(string json, out DebuggerOptions? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<DebuggerOptions>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}