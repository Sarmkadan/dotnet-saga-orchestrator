#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides validation extension methods for <see cref="SagaDefinitionService"/> instances.
/// <para>
/// Note: SagaDefinitionService is a stateless service with no instance state to validate.
/// These extension methods provide a consistent validation interface for service validation patterns.
/// </para>
/// </summary>
public static class SagaDefinitionServiceExtensionsValidation
{
    /// <summary>
    /// Validates the specified <see cref="SagaDefinitionService"/> instance.
    /// </summary>
    /// <remarks>
    /// Since <see cref="SagaDefinitionService"/> is a stateless service with no instance state to validate,
    /// this method always returns an empty list of problems for non-null instances.
    /// </remarks>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this SagaDefinitionService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaDefinitionService"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this SagaDefinitionService? value) => value is not null;

    /// <summary>
    /// Ensures that the specified <see cref="SagaDefinitionService"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this SagaDefinitionService? value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
    }
}