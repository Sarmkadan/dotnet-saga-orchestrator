#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="CircuitBreakerTests"/> instances.
/// </summary>
public static class CircuitBreakerTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="CircuitBreakerTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this CircuitBreakerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // CircuitBreakerTests is a test class with no instance fields to validate
        // All validation is done through its test methods

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CircuitBreakerTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this CircuitBreakerTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="CircuitBreakerTests"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this CircuitBreakerTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"CircuitBreakerTests instance is not valid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}
