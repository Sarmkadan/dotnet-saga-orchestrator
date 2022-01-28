#nullable enable
// =============================================================================
// Author: 
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Provides JSON serialization and deserialization helpers.
/// </summary>
public static class ValidationExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Not supported. Static types cannot be serialized.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        throw new NotSupportedException("Static types cannot be serialized.");
    }

    /// <summary>
    /// Not supported. Static types cannot be deserialized.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public static object? FromJson(string json)
    {
        throw new NotSupportedException("Static types cannot be deserialized.");
    }

    /// <summary>
    /// Not supported. Static types cannot be deserialized.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        throw new NotSupportedException("Static types cannot be deserialized.");
    }
}
