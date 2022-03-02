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
    /// <param name="value">The configuration to validate.</param>
    /// <returns>A list of validation problems; empty if the configuration is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this InfrastructureConfiguration? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // InfrastructureConfiguration is a record with all boolean properties
        // No additional validation needed beyond null check since all properties are non-nullable booleans
        // with default values of true

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="InfrastructureConfiguration"/> is valid.
    /// </summary>
    /// <param name="value">The configuration to check.</param>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    public static bool IsValid(this InfrastructureConfiguration? value)
    {
        return value?.Validate().Count == 0;
    }

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