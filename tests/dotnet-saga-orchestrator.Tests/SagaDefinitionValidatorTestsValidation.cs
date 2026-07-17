#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="SagaDefinitionValidatorTests"/> instances.
/// </summary>
public static class SagaDefinitionValidatorTestsValidation
{
    /// <summary>
    /// Validates the <see cref="SagaDefinitionValidatorTests"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaDefinitionValidatorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate async methods (these are the actual test methods)
        var asyncMethodNames = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(SagaDefinitionValidatorTests.ValidateAsync_WithValidDefinition_DoesNotThrow),
            nameof(SagaDefinitionValidatorTests.ValidateAsync_WithInvalidDefinition_Throws),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_NullName_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_NameTooLong_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_NoSteps_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_TooManySteps_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_InvalidStepName_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_InvalidServiceUrl_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_InvalidCompensationUrl_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_TimeoutZero_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_TimeoutTooLarge_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_NegativeRetries_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_TooManyRetries_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_DuplicateStepOrder_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_OrderDoesNotStartAtOne_ReturnsError),
            nameof(SagaDefinitionValidatorTests.ValidateAndGetErrorsAsync_MultipleErrors_ReturnsAll),
            nameof(SagaDefinitionValidatorTests.ValidateAsync_ThrowsWithAllErrors_InExceptionMessage)
        };

        // Check if any expected test methods are missing
        var actualMethodNames = value.GetType().GetMethods()
            .Where(m => m.DeclaringType == typeof(SagaDefinitionValidatorTests))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        var missingMethods = asyncMethodNames.Where(name => !actualMethodNames.Contains(name)).ToList();

        if (missingMethods.Count > 0)
        {
            errors.Add($"Instance is missing required test methods: {string.Join(", ", missingMethods)}.");
        }

        // Validate that all required async methods exist and are in the correct type
        foreach (var methodName in asyncMethodNames)
        {
            var method = value.GetType().GetMethod(methodName);
            if (method?.DeclaringType?.Name != "SagaDefinitionValidatorTests")
            {
                errors.Add($"Required test method '{methodName}' is missing or not declared in SagaDefinitionValidatorTests.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="SagaDefinitionValidatorTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SagaDefinitionValidatorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates the <see cref="SagaDefinitionValidatorTests"/> instance and throws an <see cref="ArgumentException"/> if invalid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this SagaDefinitionValidatorTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaDefinitionValidatorTests instance is invalid. Errors: {string.Join("; ", errors)}",
                nameof(value));
        }
    }
}