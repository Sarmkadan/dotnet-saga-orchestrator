#nullable enable

using System.Globalization;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="SagaIntegrationTests"/> to ensure test data integrity
/// and provide meaningful error messages when validation fails.
/// </summary>
public static class SagaIntegrationTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="SagaIntegrationTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation problems. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaIntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaIntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this SagaIntegrationTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SagaIntegrationTests"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with detailed error messages if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of problems.</exception>
    public static void EnsureValid(this SagaIntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"SagaIntegrationTests validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", problems)
            }",
            nameof(value));
    }
}