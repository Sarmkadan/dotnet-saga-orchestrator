#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Core.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="RetryPolicy"/> instances.
/// </summary>
public static class RetryPolicyValidation
{
    /// <summary>
    /// Validates a <see cref="RetryPolicy"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The retry policy to validate.</param>
    /// <returns>An enumerable of validation problems; empty if the policy is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this RetryPolicy value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxRetries < 0)
        {
            problems.Add($"MaxRetries must be non-negative, but was {value.MaxRetries}.");
        }

        if (value.InitialDelayMs < 0)
        {
            problems.Add($"InitialDelayMs must be non-negative, but was {value.InitialDelayMs}.");
        }

        if (value.BackoffMultiplier < 1.0)
        {
            problems.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"BackoffMultiplier must be >= 1.0, but was {value.BackoffMultiplier:G}."));
        }

        if (value.MaxDelayMs < 0)
        {
            problems.Add($"MaxDelayMs must be non-negative, but was {value.MaxDelayMs}.");
        }

        if (value.MaxDelayMs < value.InitialDelayMs)
        {
            problems.Add(
                $"MaxDelayMs ({value.MaxDelayMs}) must be >= InitialDelayMs ({value.InitialDelayMs}).");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether a <see cref="RetryPolicy"/> instance is valid.
    /// </summary>
    /// <param name="value">The retry policy to check.</param>
    /// <returns><see langword="true"/> if the policy is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this RetryPolicy value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="RetryPolicy"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The retry policy to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid with a detailed message.</exception>
    public static void EnsureValid(this RetryPolicy value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"RetryPolicy is invalid. Problems:\n\t- {
                string.Join("\n\t- ", problems)
            }");
    }
}