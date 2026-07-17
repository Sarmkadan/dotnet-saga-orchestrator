#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="SagaDefinition"/> instances.
/// </summary>
public static class SagaDefinitionValidation
{
    /// <summary>
    /// Validates the saga definition and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The saga definition to validate.</param>
    /// <returns>An enumerable of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaDefinition value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>(8);

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("SagaDefinition.Id cannot be null or whitespace.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("SagaDefinition.Name cannot be null or whitespace.");
        }
        else if (value.Name.Length > 255)
        {
            errors.Add("SagaDefinition.Name cannot exceed 255 characters.");
        }

        // Validate Description
        if (value.Description?.Length > 2048)
        {
            errors.Add("SagaDefinition.Description cannot exceed 2048 characters.");
        }

        // Validate Version
        if (value.Version <= 0)
        {
            errors.Add("SagaDefinition.Version must be a positive integer.");
        }

        // Validate Steps
        if (value.Steps == null)
        {
            errors.Add("SagaDefinition.Steps cannot be null.");
        }
        else if (value.Steps.Count == 0)
        {
            errors.Add("SagaDefinition.Steps must contain at least one step.");
        }
        else
        {
            // Validate each step
            for (int i = 0; i < value.Steps.Count; i++)
            {
                var step = value.Steps[i];
                if (step == null)
                {
                    errors.Add($"SagaDefinition.Steps[{i}] cannot be null.");
                    continue;
                }

                // Validate step properties
                if (string.IsNullOrWhiteSpace(step.Id))
                {
                    errors.Add($"SagaDefinition.Steps[{i}].Id cannot be null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(step.Name))
                {
                    errors.Add($"SagaDefinition.Steps[{i}].Name cannot be null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(step.ServiceName))
                {
                    errors.Add($"SagaDefinition.Steps[{i}].ServiceName cannot be null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(step.ServiceUrl))
                {
                    errors.Add($"SagaDefinition.Steps[{i}].ServiceUrl cannot be null or whitespace.");
                }

                if (step.IsCompensable && string.IsNullOrWhiteSpace(step.CompensationUrl))
                {
                    errors.Add($"SagaDefinition.Steps[{i}].CompensationUrl cannot be null or whitespace for compensable step.");
                }

                if (step.TimeoutSeconds <= 0)
                {
                    errors.Add($"SagaDefinition.Steps[{i}].TimeoutSeconds must be a positive integer. Current value: {step.TimeoutSeconds}.");
                }

                if (step.MaxRetries < 0)
                {
                    errors.Add($"SagaDefinition.Steps[{i}].MaxRetries cannot be negative. Current value: {step.MaxRetries}.");
                }

                if (step.RetryDelayMilliseconds < 0)
                {
                    errors.Add($"SagaDefinition.Steps[{i}].RetryDelayMilliseconds cannot be negative. Current value: {step.RetryDelayMilliseconds}.");
                }

                if (step.Order <= 0)
                {
                    errors.Add($"SagaDefinition.Steps[{i}].Order must be a positive integer. Current value: {step.Order}.");
                }

                if (step.Order > value.Steps.Count)
                {
                    errors.Add($"SagaDefinition.Steps[{i}].Order {step.Order} exceeds the total number of steps {value.Steps.Count}.");
                }
            }

            // Validate step ordering uniqueness
            var orderCounts = new Dictionary<int, int>();
            foreach (var step in value.Steps)
            {
                if (step.Order > 0)
                {
                    orderCounts[step.Order] = orderCounts.GetValueOrDefault(step.Order, 0) + 1;
                }
            }

            foreach (var (order, count) in orderCounts)
            {
                if (count > 1)
                {
                    errors.Add($"Multiple steps have Order {order}. Each step must have a unique order.");
                }
            }

            // Validate step ordering sequence
            for (int i = 0; i < value.Steps.Count; i++)
            {
                var expectedOrder = i + 1;
                var actualOrder = value.Steps[i].Order;

                if (actualOrder != expectedOrder)
                {
                    errors.Add($"SagaDefinition.Steps[{i}].Order is {actualOrder} but expected {expectedOrder} for sequential ordering.");
                }
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("SagaDefinition.CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("SagaDefinition.CreatedAt cannot be in the future.");
        }

        // Validate CompensationStrategy
        // All enum values are valid by default, so no validation needed

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the saga definition is valid.
    /// </summary>
    /// <param name="value">The saga definition to check.</param>
    /// <returns>True if the saga definition is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SagaDefinition value)
    {
        var validationErrors = SagaDefinitionValidation.Validate(value);
        return validationErrors.Count == 0;
    }

    /// <summary>
    /// Ensures that the saga definition is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The saga definition to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the saga definition is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this SagaDefinition value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = SagaDefinitionValidation.Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaDefinition is invalid. Validation errors: {string.Join(", ", errors)}",
                nameof(value));
        }
    }
}