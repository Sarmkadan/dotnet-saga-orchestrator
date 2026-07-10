#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace SagaOrchestrator.Infrastructure.Messaging;

/// <summary>
/// Provides System.Text.Json serialization extensions for SagaMessageTemplates.
/// </summary>
public static class SagaMessageTemplatesJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the SagaMessageTemplates type to a JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the SagaMessageTemplates type.</returns>
    public static string ToJson(bool indented = false)
    {
        return JsonSerializer.Serialize(typeof(SagaMessageTemplates), GetOptions(indented));
    }

    private static JsonSerializerOptions GetOptions(bool indented) => indented
        ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
        : _jsonOptions;

    /// <summary>
    /// Deserializes a JSON string to a SagaMessageTemplates type reference.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A Type instance representing SagaMessageTemplates, or null if deserialization fails.</returns>
    public static Type? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Type>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a SagaMessageTemplates type reference.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The resulting Type instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out Type? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<Type>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}