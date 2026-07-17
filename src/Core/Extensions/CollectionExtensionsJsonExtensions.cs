#nullable enable

using System.Text.Json;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for collections.
/// </summary>
public static class CollectionExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a collection to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to serialize.</param>
    /// <param name="indented">If true, the JSON will be formatted with indentation.</param>
    /// <returns>A JSON string representation of the collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> is null.</exception>
    public static string ToJson<T>(this IEnumerable<T> collection, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(collection);

        JsonSerializerOptions options = indented ? new(JsonSerializerOptions) { WriteIndented = true } : JsonSerializerOptions;
        return JsonSerializer.Serialize(collection, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An enumerable instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static IEnumerable<T>? FromJson<T>(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IEnumerable<T>>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an enumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized enumerable if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty.</exception>
    public static bool TryFromJson<T>(string json, out IEnumerable<T>? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<IEnumerable<T>>(json, JsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
