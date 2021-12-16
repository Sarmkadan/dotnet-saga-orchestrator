#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides validation methods for <see cref="Saga"/> instances.
/// </summary>
public static class SagaValidation
{
    /// <summary>
    /// Validates a saga instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The saga to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Saga value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Saga.Id cannot be null or whitespace.");
        }

        // Validate CorrelationId
        if (string.IsNullOrWhiteSpace(value.CorrelationId))
        {
            errors.Add("Saga.CorrelationId cannot be null or whitespace.");
        }

        // Validate Status
        if (!Enum.IsDefined(typeof(SagaStatus), value.Status))
        {
            errors.Add("Saga.Status is not a valid SagaStatus value.");
        }

        // Validate Definition
        if (value.Definition is null)
        {
            errors.Add("Saga.Definition cannot be null.");
        }

        // Validate Steps
        if (value.Steps is null)
        {
            errors.Add("Saga.Steps cannot be null.");
        }
        else if (value.Steps.Count < 0)
        {
            errors.Add("Saga.Steps.Count cannot be negative.");
        }

        // Validate StartedAt
        if (value.StartedAt == default)
        {
            errors.Add("Saga.StartedAt cannot be the default DateTime value.");
        }
        else if (value.StartedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("Saga.StartedAt must be in UTC format.");
        }

        // Validate CompletedAt
        if (value.CompletedAt.HasValue)
        {
            if (value.CompletedAt.Value == default)
            {
                errors.Add("Saga.CompletedAt cannot be the default DateTime value when set.");
            }
            else if (value.CompletedAt.Value.Kind != DateTimeKind.Utc)
            {
                errors.Add("Saga.CompletedAt must be in UTC format when set.");
            }
            else if (value.CompletedAt.Value < value.StartedAt)
            {
                errors.Add("Saga.CompletedAt cannot be earlier than Saga.StartedAt.");
            }
        }

        // Validate FailedAt
        if (value.FailedAt.HasValue)
        {
            if (value.FailedAt.Value == default)
            {
                errors.Add("Saga.FailedAt cannot be the default DateTime value when set.");
            }
            else if (value.FailedAt.Value.Kind != DateTimeKind.Utc)
            {
                errors.Add("Saga.FailedAt must be in UTC format when set.");
            }
            else if (value.FailedAt.Value < value.StartedAt)
            {
                errors.Add("Saga.FailedAt cannot be earlier than Saga.StartedAt.");
            }
        }

        // Validate FailureReason
        if (value.FailureReason is not null && string.IsNullOrWhiteSpace(value.FailureReason))
        {
            errors.Add("Saga.FailureReason cannot be empty or whitespace when set.");
        }

        // Validate CompensationStartedAt
        if (value.CompensationStartedAt.HasValue)
        {
            if (value.CompensationStartedAt.Value == default)
            {
                errors.Add("Saga.CompensationStartedAt cannot be the default DateTime value when set.");
            }
            else if (value.CompensationStartedAt.Value.Kind != DateTimeKind.Utc)
            {
                errors.Add("Saga.CompensationStartedAt must be in UTC format when set.");
            }
            else if (value.CompensationStartedAt.Value < value.StartedAt)
            {
                errors.Add("Saga.CompensationStartedAt cannot be earlier than Saga.StartedAt.");
            }
        }

        // Validate RetryCount
        if (value.RetryCount < 0)
        {
            errors.Add("Saga.RetryCount cannot be negative.");
        }

        // Validate MaxRetries
        if (value.MaxRetries < 0)
        {
            errors.Add("Saga.MaxRetries cannot be negative.");
        }

        // Validate TimeoutSeconds
        if (value.TimeoutSeconds <= 0)
        {
            errors.Add("Saga.TimeoutSeconds must be a positive value greater than zero.");
        }

        // Validate Metadata
        if (value.Metadata is null)
        {
            errors.Add("Saga.Metadata cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a saga instance is valid.
    /// </summary>
    /// <param name="value">The saga to check.</param>
    /// <returns>True if the saga is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Saga value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a saga instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The saga to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the saga is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this Saga value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!IsValid(value))
        {
            var errors = Validate(value);
            throw new ArgumentException(
                $"Saga validation failed with {errors.Count} error(s):{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}