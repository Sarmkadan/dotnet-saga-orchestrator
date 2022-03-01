#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Validation helpers for <see cref="SagaOrchestrationService"/>.
/// </summary>
public static class SagaOrchestrationServiceValidation
{
    /// <summary>
    /// Validates the <see cref="SagaOrchestrationService"/> instance and returns a collection of human‑readable problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of validation messages.
    /// If the instance is <c>null</c>, a single message describing the problem is returned.
    /// </returns>
    public static IReadOnlyList<string> Validate(this SagaOrchestrationService? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("SagaOrchestrationService instance is null.");
            return problems;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="SagaOrchestrationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this SagaOrchestrationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return !value.Validate().Any();
    }

    /// <summary>
    /// Ensures that the <see cref="SagaOrchestrationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are found. The exception message contains a semicolon‑separated list of problems.
    /// </exception>
    public static void EnsureValid(this SagaOrchestrationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.Validate();
        if (problems.Any())
        {
            throw new ArgumentException(string.Join("; ", problems), nameof(value));
        }
    }
}