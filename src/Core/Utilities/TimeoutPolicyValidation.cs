#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Core.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="TimeoutPolicy"/> instances.
/// </summary>
public static class TimeoutPolicyValidation
{
    /// <summary>
    /// Validates a <see cref="TimeoutPolicy"/> instance.
    /// </summary>
    /// <param name="value">The timeout policy to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TimeoutPolicy? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate TimeoutSeconds
        if (value.TimeoutSeconds <= 0)
        {
            errors.Add($"TimeoutSeconds must be positive, but was {value.TimeoutSeconds}.");
        }

        // Validate Timeout
        if (value.Timeout <= TimeSpan.Zero)
        {
            errors.Add($"Timeout must be positive, but was {value.Timeout.TotalSeconds} seconds.");
        }

        // Validate IsRelaxed consistency with TimeoutSeconds
        if (value.IsRelaxed && value.TimeoutSeconds < 300)
        {
            errors.Add("IsRelaxed is true but TimeoutSeconds is less than 300 seconds (5 minutes).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="TimeoutPolicy"/> instance is valid.
    /// </summary>
    /// <param name="value">The timeout policy to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this TimeoutPolicy? value)
        => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="TimeoutPolicy"/> instance is valid.
    /// </summary>
    /// <param name="value">The timeout policy to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this TimeoutPolicy? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"TimeoutPolicy is invalid. Validation errors:\n{string.Join("\n", errors)}");
        }
    }
}