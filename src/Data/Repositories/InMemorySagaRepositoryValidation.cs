#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Provides validation helpers for <see cref="InMemorySagaRepository"/> instances.
/// </summary>
public static class InMemorySagaRepositoryValidation
{
    /// <summary>
    /// Validates the <see cref="InMemorySagaRepository"/> instance and returns a collection of human-readable problems.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of validation messages.
    /// If the instance is <c>null</c>, a single message describing the problem is returned.
    /// </returns>
    public static IReadOnlyList<string> Validate(this InMemorySagaRepository? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("InMemorySagaRepository instance is null.");
            return problems.AsReadOnly();
        }

        // Note: InMemorySagaRepository is a simple wrapper around Dictionary<string, Saga>
        // There's no meaningful state to validate beyond null checks
        // The actual validation happens when operations are performed on the repository

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="InMemorySagaRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <c>null</c>.
    /// </exception>
    public static bool IsValid(this InMemorySagaRepository value) => !value.Validate().Any();

    /// <summary>
    /// Ensures that the <see cref="InMemorySagaRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are found. The exception message contains a semicolon-separated list of problems.
    /// </exception>
    public static void EnsureValid(this InMemorySagaRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.Validate();
        if (problems.Any())
        {
            throw new ArgumentException(string.Join("; ", problems), nameof(value));
        }
    }
}