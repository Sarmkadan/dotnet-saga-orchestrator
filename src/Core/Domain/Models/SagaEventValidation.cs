#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="SagaEvent"/> instances.
/// </summary>
/// <remarks>
/// This static class offers extension methods for validating <see cref="SagaEvent"/> objects
/// against domain constraints including length limits, timestamp validity, and required fields.
/// </remarks>
public static class SagaEventValidation
{
    /// <summary>
    /// Validates the specified <see cref="SagaEvent"/> instance.
    /// </summary>
    /// <param name="value">The saga event to validate.</param>
    /// <returns>A list of validation error messages; empty if the event is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("SagaEvent.Id cannot be null or whitespace.");
        }
        else if (value.Id.Length > 100)
        {
            errors.Add("SagaEvent.Id cannot exceed 100 characters.");
        }

        // Validate SagaId
        if (string.IsNullOrWhiteSpace(value.SagaId))
        {
            errors.Add("SagaEvent.SagaId cannot be null or whitespace.");
        }
        else if (value.SagaId.Length > 100)
        {
            errors.Add("SagaEvent.SagaId cannot exceed 100 characters.");
        }

        // Validate EventType
        if (string.IsNullOrWhiteSpace(value.EventType))
        {
            errors.Add("SagaEvent.EventType cannot be null or whitespace.");
        }
        else if (value.EventType.Length > 50)
        {
            errors.Add("SagaEvent.EventType cannot exceed 50 characters.");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            errors.Add("SagaEvent.EventName cannot be null or whitespace.");
        }
        else if (value.EventName.Length > 100)
        {
            errors.Add("SagaEvent.EventName cannot exceed 100 characters.");
        }

        // Validate Description
        if (value.Description is null)
        {
            errors.Add("SagaEvent.Description cannot be null.");
        }
        else if (value.Description.Length > 500)
        {
            errors.Add("SagaEvent.Description cannot exceed 500 characters.");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            errors.Add("SagaEvent.Timestamp must be set to a non-default DateTime value.");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("SagaEvent.Timestamp cannot be in the future.");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("SagaEvent.Timestamp cannot be more than one year in the past.");
        }

        // Validate Severity
        if (!Enum.IsDefined(typeof(EventSeverity), value.Severity))
        {
            errors.Add("SagaEvent.Severity must be a valid EventSeverity value.");
        }

        // Validate Data - ensure dictionary is initialized
        if (value.Data is null)
        {
            errors.Add("SagaEvent.Data dictionary cannot be null.");
        }

        // Validate Source
        if (string.IsNullOrWhiteSpace(value.Source))
        {
            errors.Add("SagaEvent.Source cannot be null or whitespace.");
        }
        else if (value.Source.Length > 100)
        {
            errors.Add("SagaEvent.Source cannot exceed 100 characters.");
        }

        // Validate optional fields if they are set
        if (value.StepId is not null && value.StepId.Length > 100)
        {
            errors.Add("SagaEvent.StepId cannot exceed 100 characters when set.");
        }

        if (value.StepName is not null && value.StepName.Length > 100)
        {
            errors.Add("SagaEvent.StepName cannot exceed 100 characters when set.");
        }

        if (value.CorrelationId is not null && value.CorrelationId.Length > 100)
        {
            errors.Add("SagaEvent.CorrelationId cannot exceed 100 characters when set.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaEvent"/> instance is valid.
    /// </summary>
    /// <param name="value">The saga event to check.</param>
    /// <returns><see langword="true"/> if the event is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SagaEvent value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="SagaEvent"/> instance is valid.
    /// </summary>
    /// <param name="value">The saga event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the event is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this SagaEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaEvent validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}