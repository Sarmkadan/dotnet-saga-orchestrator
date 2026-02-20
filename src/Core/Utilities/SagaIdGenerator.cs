// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Utilities;

/// <summary>
/// Utility class for generating saga-related identifiers.
/// </summary>
public static class SagaIdGenerator
{
    private static readonly Random _random = new Random();
    private static readonly object _lockObject = new();

    /// <summary>
    /// Generates a unique saga ID with prefix
    /// </summary>
    public static string GenerateSagaId()
    {
        return $"saga_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Generates a unique correlation ID
    /// </summary>
    public static string GenerateCorrelationId()
    {
        return $"corr_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Generates a unique step ID
    /// </summary>
    public static string GenerateStepId()
    {
        return $"step_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Generates a trace ID for distributed tracing
    /// </summary>
    public static string GenerateTraceId()
    {
        return $"trace_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Generates a request ID
    /// </summary>
    public static string GenerateRequestId()
    {
        lock (_lockObject)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var random = _random.Next(1000, 9999);
            return $"req_{timestamp}_{random}";
        }
    }

    /// <summary>
    /// Validates a saga ID format
    /// </summary>
    public static bool IsValidSagaId(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return id.StartsWith("saga_") && id.Length > 5;
    }

    /// <summary>
    /// Validates a correlation ID format
    /// </summary>
    public static bool IsValidCorrelationId(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return (id.StartsWith("corr_") && id.Length > 5) || Guid.TryParse(id, out _);
    }
}
