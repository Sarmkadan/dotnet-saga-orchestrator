#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Core.Builders;

/// <summary>
/// Provides validation helpers for <see cref="SagaStepBuilder"/> instances.
/// </summary>
public static class SagaStepBuilderValidation
{
    /// <summary>
    /// Validates the specified <see cref="SagaStepBuilder"/> instance.
    /// </summary>
    /// <param name="value">The builder instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaStepBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();
        var step = value.Build();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(step.Name))
        {
            errors.Add("Step name is required and cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(step.ServiceName))
        {
            errors.Add("Service name is required and cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(step.ServiceUrl))
        {
            errors.Add("Action URL is required and cannot be null or whitespace");
        }
        else if (!Uri.IsWellFormedUriString(step.ServiceUrl, UriKind.Absolute))
        {
            errors.Add("Action URL must be a valid absolute URI");
        }

        // Validate timeout - must be between 1 and 3600 seconds (1 hour)
        if (step.TimeoutSeconds < 1)
        {
            errors.Add("Timeout must be at least 1 second");
        }

        if (step.TimeoutSeconds > 3600)
        {
            errors.Add("Timeout cannot exceed 3600 seconds (1 hour)");
        }

        // Validate retry configuration - MaxRetries: 0-10, RetryDelayMilliseconds: 0-3600000
        if (step.MaxRetries < 0)
        {
            errors.Add("Max retries cannot be negative");
        }
        else if (step.MaxRetries > 10)
        {
            errors.Add("Max retries cannot exceed 10");
        }

        if (step.RetryDelayMilliseconds < 0)
        {
            errors.Add("Retry delay cannot be negative");
        }
        else if (step.RetryDelayMilliseconds > 3600000)
        {
            errors.Add("Retry delay cannot exceed 3600000 milliseconds (1 hour)");
        }

        // Validate compensation URL if compensable
        if (step.IsCompensable)
        {
            if (string.IsNullOrWhiteSpace(step.CompensationUrl))
            {
                errors.Add("Compensation URL is required for compensable steps");
            }
            else if (!Uri.IsWellFormedUriString(step.CompensationUrl, UriKind.Absolute))
            {
                errors.Add("Compensation URL must be a valid absolute URI if provided");
            }
        }
        else if (!string.IsNullOrWhiteSpace(step.CompensationUrl) &&
                 !Uri.IsWellFormedUriString(step.CompensationUrl, UriKind.Absolute))
        {
            errors.Add("Compensation URL must be a valid absolute URI if provided");
        }

        // Validate order - must be greater than 0
        if (step.Order < 1)
        {
            errors.Add("Step order must be at least 1");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaStepBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The builder instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SagaStepBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SagaStepBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The builder instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the builder is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this SagaStepBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaStepBuilder is invalid: {string.Join("; ", errors)}",
                nameof(value));
        }
    }
}
