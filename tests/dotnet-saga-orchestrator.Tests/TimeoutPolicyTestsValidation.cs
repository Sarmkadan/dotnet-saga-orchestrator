#nullable enable

using System.Globalization;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="TimeoutPolicyTests"/> instances.
/// </summary>
public static class TimeoutPolicyTestsValidation
{
    /// <summary>
    /// Validates that a <see cref="TimeoutPolicyTests"/> instance contains valid values.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation errors (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TimeoutPolicyTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.TimeoutSeconds <= 0)
        {
            errors.Add($"TimeoutSeconds must be positive, but was {value.TimeoutSeconds}.");
        }

        if (value.IsRelaxed && value.TimeoutSeconds < 60)
        {
            errors.Add("IsRelaxed cannot be true when TimeoutSeconds is less than 60 seconds.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="TimeoutPolicyTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this TimeoutPolicyTests? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="TimeoutPolicyTests"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this TimeoutPolicyTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"TimeoutPolicyTests is invalid. Validation errors: {string.Join(" ", errors)}");
        }
    }
}