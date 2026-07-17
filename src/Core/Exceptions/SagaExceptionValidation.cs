#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="SagaException"/> instances.
/// </summary>
public static class SagaExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="SagaException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if the exception is valid (no problems found).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this SagaException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message must be non-null and non-empty.");
        }

        if (value.SagaId is not null && string.IsNullOrWhiteSpace(value.SagaId))
        {
            problems.Add("SagaId must be non-null and non-empty when provided.");
        }

        if (value.ErrorCode is not null && string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add("ErrorCode must be non-null and non-empty when provided.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns><see langword="true"/> if the exception is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid([NotNullWhen(true)] this SagaException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SagaException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the exception is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid([NotNull] this SagaException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems is { Count: > 0 })
        {
            throw new ArgumentException(
                $"SagaException is invalid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}