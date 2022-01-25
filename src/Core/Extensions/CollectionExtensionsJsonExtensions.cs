#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="CollectionExtensions"/>.
/// </summary>
public static class CollectionExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="CollectionExtensionsData"/> instance to a JSON string.
    /// </summary>
    /// <param name="data">The <see cref="CollectionExtensionsData"/> to serialize.</param>
    /// <param name="indented">If true, the JSON will be formatted with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="CollectionExtensionsData"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
    public static string ToJson(this CollectionExtensionsData data, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(data);

        JsonSerializerOptions options = indented ? new(JsonSerializerOptions) { WriteIndented = true } : JsonSerializerOptions;
        return JsonSerializer.Serialize(data, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="CollectionExtensionsData"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="CollectionExtensionsData"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static CollectionExtensionsData? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<CollectionExtensionsData>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="CollectionExtensionsData"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="CollectionExtensionsData"/> if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out CollectionExtensionsData? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<CollectionExtensionsData>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}

public class CollectionExtensionsData
{
    // Add properties here that match the data you want to serialize
}
