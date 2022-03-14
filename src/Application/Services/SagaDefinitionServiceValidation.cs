#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides validation helpers for <see cref="SagaDefinitionService"/> instances.
/// </summary>
public static class SagaDefinitionServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="SagaDefinitionService"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this SagaDefinitionService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // SagaDefinitionService is a service class with no instance state to validate
        // The service itself is always valid as long as it's not null
        // All business logic validation is handled through the service's methods

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaDefinitionService"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this SagaDefinitionService? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="SagaDefinitionService"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this SagaDefinitionService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"SagaDefinitionService instance is not valid. Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }
}