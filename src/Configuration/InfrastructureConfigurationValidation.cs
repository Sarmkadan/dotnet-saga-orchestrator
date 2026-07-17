#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="InfrastructureConfiguration"/> instances.
/// </summary>
public static class InfrastructureConfigurationValidation
{
    /// <summary>
    /// Validates the specified <see cref="InfrastructureConfiguration"/> instance.
    /// </summary>
    /// <remarks>
    /// This method performs null validation on the configuration instance.
    /// Since <see cref="InfrastructureConfiguration"/> is a record with all non-nullable boolean properties
    /// having default values of <see langword="true"/>, no additional validation is required.
    /// </remarks>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>An empty list if the configuration is valid; otherwise a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this InfrastructureConfiguration? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return [];
    }

    /// <summary>
    /// Determines whether the specified <see cref="InfrastructureConfiguration"/> is valid.
    /// </summary>
    /// <param name="value">The configuration to check.</param>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    public static bool IsValid(this InfrastructureConfiguration? value)
        => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="InfrastructureConfiguration"/> is valid.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the configuration contains validation problems.</exception>
    public static void EnsureValid(this InfrastructureConfiguration? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Infrastructure configuration validation failed. " +
                "Problems: " + string.Join("; ", problems),
                nameof(value));
        }
    }
}