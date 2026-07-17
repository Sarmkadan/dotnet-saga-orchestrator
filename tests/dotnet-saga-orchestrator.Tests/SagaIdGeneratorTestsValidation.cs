using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation methods for <see cref="SagaIdGeneratorTests"/> to verify test behavior.
/// </summary>
public static class SagaIdGeneratorTestsValidation
{
    /// <summary>
    /// Validates that all test methods in the <see cref="SagaIdGeneratorTests"/> instance execute successfully.
    /// </summary>
    /// <param name="value">The <see cref="SagaIdGeneratorTests"/> instance to validate.</param>
    /// <returns>A read-only list of validation problems, or empty if all tests pass.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaIdGeneratorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate GenerateSagaId_ShouldHaveCorrectPrefix
        try
        {
            value.GenerateSagaId_ShouldHaveCorrectPrefix();
        }
        catch (Exception ex)
        {
            problems.Add($"GenerateSagaId_ShouldHaveCorrectPrefix failed: {ex.Message}");
        }

        // Validate GenerateCorrelationId_ShouldHaveCorrectPrefix
        try
        {
            value.GenerateCorrelationId_ShouldHaveCorrectPrefix();
        }
        catch (Exception ex)
        {
            problems.Add($"GenerateCorrelationId_ShouldHaveCorrectPrefix failed: {ex.Message}");
        }

        // Validate GenerateStepId_ShouldHaveCorrectPrefix
        try
        {
            value.GenerateStepId_ShouldHaveCorrectPrefix();
        }
        catch (Exception ex)
        {
            problems.Add($"GenerateStepId_ShouldHaveCorrectPrefix failed: {ex.Message}");
        }

        // Validate GenerateTraceId_ShouldHaveCorrectPrefix
        try
        {
            value.GenerateTraceId_ShouldHaveCorrectPrefix();
        }
        catch (Exception ex)
        {
            problems.Add($"GenerateTraceId_ShouldHaveCorrectPrefix failed: {ex.Message}");
        }

        // Validate GenerateRequestId_ShouldHaveCorrectPrefix
        try
        {
            value.GenerateRequestId_ShouldHaveCorrectPrefix();
        }
        catch (Exception ex)
        {
            problems.Add($"GenerateRequestId_ShouldHaveCorrectPrefix failed: {ex.Message}");
        }

        // Validate IsValidSagaId_ShouldValidateCorrectly with various inputs
        try
        {
            value.IsValidSagaId_ShouldValidateCorrectly("saga_1234567890abcdef1234567890abcdef", true);
            value.IsValidSagaId_ShouldValidateCorrectly("corr_12345", false);
            value.IsValidSagaId_ShouldValidateCorrectly(string.Empty, false);
        }
        catch (Exception ex)
        {
            problems.Add($"IsValidSagaId_ShouldValidateCorrectly failed: {ex.Message}");
        }

        // Validate IsValidCorrelationId_ShouldValidateCorrectly with various inputs
        try
        {
            value.IsValidCorrelationId_ShouldValidateCorrectly("corr_1234567890abcdef1234567890abcdef", true);
            value.IsValidCorrelationId_ShouldValidateCorrectly("12345678-1234-1234-1234-1234567890ab", true);
            value.IsValidCorrelationId_ShouldValidateCorrectly("saga_123", false);
            value.IsValidCorrelationId_ShouldValidateCorrectly(string.Empty, false);
        }
        catch (Exception ex)
        {
            problems.Add($"IsValidCorrelationId_ShouldValidateCorrectly failed: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="SagaIdGeneratorTests"/> instance passes all validations.
    /// </summary>
    /// <param name="value">The <see cref="SagaIdGeneratorTests"/> instance to check.</param>
    /// <returns>True if all validations pass; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SagaIdGeneratorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates the <see cref="SagaIdGeneratorTests"/> instance and throws an exception if validation fails.
    /// </summary>
    /// <param name="value">The <see cref="SagaIdGeneratorTests"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems.</exception>
    public static void EnsureValid(this SagaIdGeneratorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SagaIdGeneratorTests instance is not valid. Problems: {string.Join("; ", problems)}");
        }
    }
}