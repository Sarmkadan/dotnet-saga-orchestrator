using System;
using System.Text.Json;
using System.Text.Json.Serialization;

// Required for AOT/trim compatibility warnings suppression
using System.Diagnostics.CodeAnalysis;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for enum types.
/// </summary>
[RequiresUnreferencedCode("JSON serialization and deserialization requires preserving enum types")]
[RequiresDynamicCode("JSON serialization and deserialization requires dynamic code generation for enum types")]
public static class EnumExtensionsJsonExtensions
{
    /// <summary>
    /// Configured JSON serializer options with camelCase naming policy.
    /// </summary>
    /// <remarks>
    /// This configuration uses runtime serialization which requires preserving enum types.
    /// For AOT scenarios, consider using <see cref="JsonSerializerOptions"/> with source generation.
    /// </remarks>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an enum value to a JSON string.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The enum value to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson<TEnum>(this TEnum value, bool indented = false) where TEnum : Enum
        => JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonOptions) { WriteIndented = indented });

    /// <summary>
    /// Deserializes a JSON string to an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized enum value, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
    public static TEnum? FromJson<TEnum>(string json) where TEnum : Enum
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<TEnum>(json, JsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized enum value, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson<TEnum>(string json, out TEnum? value) where TEnum : Enum
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<TEnum>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}