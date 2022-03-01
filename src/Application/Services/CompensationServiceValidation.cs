#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Validation helpers for <see cref="CompensationService"/>.
/// </summary>
public static class CompensationServiceValidation
{
    /// <summary>
    /// Validates the <see cref="CompensationService"/> instance and returns a collection of human‑readable problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of validation messages.
    /// If the instance is <c>null</c>, a single message describing the problem is returned.
    /// </returns>
    public static IReadOnlyList<string> Validate(this CompensationService? value) =>
        value is null
            ? new[] { "CompensationService instance is null." }
            : Array.Empty<string>();

    /// <summary>
    /// Determines whether the <see cref="CompensationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this CompensationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return !value.Validate().Any();
    }

    /// <summary>
    /// Ensures that the <see cref="CompensationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are found. The exception message contains a semicolon‑separated list of problems.
    /// </exception>
    public static void EnsureValid(this CompensationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.Validate();
        if (problems.Any())
        {
            throw new ArgumentException(string.Join("; ", problems), nameof(value));
        }
    }
}
