using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Validation helpers for SagaIdGeneratorTests to ensure test data is valid.
/// </summary>
public static class SagaIdGeneratorTestsValidation
{
    /// <summary>
    /// Validates the SagaIdGeneratorTests instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The SagaIdGeneratorTests instance to validate.</param>
    /// <returns>A read-only list of human-readable validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
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

        // Validate IsValidSagaId_ShouldValidateCorrectly
        try
        {
            value.IsValidSagaId_ShouldValidateCorrectly("saga_1234567890abcdef1234567890abcdef", true);
            value.IsValidSagaId_ShouldValidateCorrectly("corr_12345", false);
            value.IsValidSagaId_ShouldValidateCorrectly("", false);
        }
        catch (Exception ex)
        {
            problems.Add($"IsValidSagaId_ShouldValidateCorrectly failed: {ex.Message}");
        }

        // Validate IsValidCorrelationId_ShouldValidateCorrectly
        try
        {
            value.IsValidCorrelationId_ShouldValidateCorrectly("corr_1234567890abcdef1234567890abcdef", true);
            value.IsValidCorrelationId_ShouldValidateCorrectly("12345678-1234-1234-1234-1234567890ab", true);
            value.IsValidCorrelationId_ShouldValidateCorrectly("saga_123", false);
            value.IsValidCorrelationId_ShouldValidateCorrectly("", false);
        }
        catch (Exception ex)
        {
            problems.Add($"IsValidCorrelationId_ShouldValidateCorrectly failed: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the SagaIdGeneratorTests instance is valid.
    /// </summary>
    /// <param name="value">The SagaIdGeneratorTests instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this SagaIdGeneratorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the SagaIdGeneratorTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The SagaIdGeneratorTests instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if value is not valid, containing the list of problems.</exception>
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