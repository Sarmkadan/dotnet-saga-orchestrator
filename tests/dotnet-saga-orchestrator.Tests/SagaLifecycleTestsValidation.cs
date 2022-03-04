#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="SagaLifecycleTests"/> instances.
/// </summary>
public static class SagaLifecycleTestsValidation
{
    /// <summary>
    /// Validates that a <see cref="SagaLifecycleTests"/> instance is properly constructed for testing.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of validation errors (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaLifecycleTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // SagaLifecycleTests is a test class with no instance fields to validate
        // All validation is done through its test methods

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SagaLifecycleTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this SagaLifecycleTests value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SagaLifecycleTests"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this SagaLifecycleTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaLifecycleTests is invalid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}