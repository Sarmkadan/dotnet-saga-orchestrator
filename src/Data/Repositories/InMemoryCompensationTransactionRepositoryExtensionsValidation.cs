#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Validation helpers for <see cref="InMemoryCompensationTransactionRepositoryExtensions"/>.
/// </summary>
public static class InMemoryCompensationTransactionRepositoryExtensionsValidation
{
    /// <summary>
    /// Validates the <see cref="InMemoryCompensationTransactionRepositoryExtensions"/> type and returns a list of human-readable problems.
    /// </summary>
    /// <returns>A read-only list of validation problems; empty if the type is valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // InMemoryCompensationTransactionRepositoryExtensions is a static class with extension methods
        // There is no instance state to validate

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="InMemoryCompensationTransactionRepositoryExtensions"/> type is valid.
    /// </summary>
    /// <returns><see langword="true"/> if the type is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the <see cref="InMemoryCompensationTransactionRepositoryExtensions"/> type is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the type is not valid.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"InMemoryCompensationTransactionRepositoryExtensions type is not valid. Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }
}