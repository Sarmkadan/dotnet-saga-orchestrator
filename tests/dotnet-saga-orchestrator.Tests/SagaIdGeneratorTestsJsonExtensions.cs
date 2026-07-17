using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SagaIdGeneratorTests"/>.
/// </summary>
public static class SagaIdGeneratorTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="SagaIdGeneratorTests"/> instance to JSON.
    /// </summary>
    /// <param name="value">The <see cref="SagaIdGeneratorTests"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="SagaIdGeneratorTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this SagaIdGeneratorTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SagaIdGeneratorTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="SagaIdGeneratorTests"/> instance, or <see langword="null"/> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static SagaIdGeneratorTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : TryDeserialize(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SagaIdGeneratorTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="SagaIdGeneratorTests"/> instance, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out SagaIdGeneratorTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;
        return !string.IsNullOrWhiteSpace(json)
            && TryDeserialize(json, _jsonSerializerOptions, out value);
    }

    private static SagaIdGeneratorTests? TryDeserialize(string json, JsonSerializerOptions options)
    {
        try
        {
            return JsonSerializer.Deserialize<SagaIdGeneratorTests>(json, options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool TryDeserialize(string json, JsonSerializerOptions options, out SagaIdGeneratorTests? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<SagaIdGeneratorTests>(json, options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}