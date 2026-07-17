#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="SagaStep"/> instances.
/// </summary>
public static class SagaStepValidation
{
    /// <summary>
    /// Validates a <see cref="SagaStep"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The saga step to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this SagaStep value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("SagaStep.Id cannot be null or whitespace.");
        }

        // Validate SagaId
        if (string.IsNullOrWhiteSpace(value.SagaId))
        {
            errors.Add("SagaStep.SagaId cannot be null or whitespace.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("SagaStep.Name cannot be null or whitespace.");
        }

        // Validate Order (must be non-negative)
        if (value.Order < 0)
        {
            errors.Add("SagaStep.Order must be a non-negative integer.");
        }

        // Validate Status (must be a valid enum value)
        if (!Enum.IsDefined(typeof(SagaStepStatus), value.Status))
        {
            errors.Add($"SagaStep.Status has invalid value: {value.Status}.");
        }

        // Validate ServiceUrl
        if (string.IsNullOrWhiteSpace(value.ServiceUrl))
        {
            errors.Add("SagaStep.ServiceUrl cannot be null or whitespace.");
        }
        else if (!Uri.IsWellFormedUriString(value.ServiceUrl, UriKind.Absolute) &&
                 !Uri.IsWellFormedUriString(value.ServiceUrl, UriKind.Relative))
        {
            errors.Add("SagaStep.ServiceUrl must be a valid URI.");
        }

        // Validate CompensationUrl (optional but must be valid if provided)
        if (!string.IsNullOrWhiteSpace(value.CompensationUrl) &&
            !Uri.IsWellFormedUriString(value.CompensationUrl, UriKind.Absolute) &&
            !Uri.IsWellFormedUriString(value.CompensationUrl, UriKind.Relative))
        {
            errors.Add("SagaStep.CompensationUrl must be a valid URI or empty.");
        }

        // Validate Payload (optional but if present must be initialized)
        if (value.Payload == null)
        {
            errors.Add("SagaStep.Payload cannot be null.");
        }

        // Validate Response (optional but if present must be initialized)
        if (value.Response == null)
        {
            errors.Add("SagaStep.Response cannot be null.");
        }

        // Validate StartedAt (must be UTC if set)
        if (value.StartedAt.HasValue && value.StartedAt.Value.Kind != DateTimeKind.Utc)
        {
            errors.Add("SagaStep.StartedAt must be in UTC format.");
        }

        // Validate CompletedAt (must be UTC if set, and must be after StartedAt if both are set)
        if (value.CompletedAt.HasValue)
        {
            if (value.CompletedAt.Value.Kind != DateTimeKind.Utc)
            {
                errors.Add("SagaStep.CompletedAt must be in UTC format.");
            }

            if (value.StartedAt.HasValue && value.CompletedAt.Value < value.StartedAt.Value)
            {
                errors.Add("SagaStep.CompletedAt cannot be earlier than StartedAt.");
            }
        }

        // Validate CompensatedAt (must be UTC if set, and must be after CompletedAt if both are set)
        if (value.CompensatedAt.HasValue)
        {
            if (value.CompensatedAt.Value.Kind != DateTimeKind.Utc)
            {
                errors.Add("SagaStep.CompensatedAt must be in UTC format.");
            }

            if (value.CompletedAt.HasValue && value.CompensatedAt.Value < value.CompletedAt.Value)
            {
                errors.Add("SagaStep.CompensatedAt cannot be earlier than CompletedAt.");
            }
        }

        // Validate ErrorMessage (optional but must be non-empty if set)
        if (value.ErrorMessage is { Length: > 0 } && string.IsNullOrWhiteSpace(value.ErrorMessage))
        {
            errors.Add("SagaStep.ErrorMessage cannot be whitespace.");
        }

        // Validate RetryCount (must be non-negative and not exceed MaxRetries)
        if (value.RetryCount < 0)
        {
            errors.Add("SagaStep.RetryCount must be a non-negative integer.");
        }
        else if (value.MaxRetries > 0 && value.RetryCount > value.MaxRetries)
        {
            errors.Add("SagaStep.RetryCount cannot exceed SagaStep.MaxRetries.");
        }

        // Validate MaxRetries (must be non-negative)
        if (value.MaxRetries < 0)
        {
            errors.Add("SagaStep.MaxRetries must be a non-negative integer.");
        }

        // Validate TimeoutSeconds (must be positive)
        if (value.TimeoutSeconds <= 0)
        {
            errors.Add("SagaStep.TimeoutSeconds must be a positive integer.");
        }

        // Validate RetryPolicy (if set, its MaxRetries must be non-negative)
        if (value.RetryPolicy != null && value.RetryPolicy.MaxRetries < 0)
        {
            errors.Add("SagaStep.RetryPolicy.MaxRetries must be a non-negative integer.");
        }

        // Validate status-specific constraints
        switch (value.Status)
        {
            case SagaStepStatus.Executing:
                if (!value.StartedAt.HasValue)
                {
                    errors.Add("SagaStep in Executing status must have StartedAt set.");
                }
                break;

            case SagaStepStatus.Completed:
                if (!value.StartedAt.HasValue)
                {
                    errors.Add("SagaStep in Completed status must have StartedAt set.");
                }
                if (!value.CompletedAt.HasValue)
                {
                    errors.Add("SagaStep in Completed status must have CompletedAt set.");
                }
                break;

            case SagaStepStatus.Failed:
                if (!value.StartedAt.HasValue)
                {
                    errors.Add("SagaStep in Failed status must have StartedAt set.");
                }
                if (string.IsNullOrWhiteSpace(value.ErrorMessage))
                {
                    errors.Add("SagaStep in Failed status must have ErrorMessage set.");
                }
                break;

            case SagaStepStatus.WaitingForRetry:
                if (!value.StartedAt.HasValue)
                {
                    errors.Add("SagaStep in WaitingForRetry status must have StartedAt set.");
                }
                if (value.RetryCount <= 0)
                {
                    errors.Add("SagaStep in WaitingForRetry status must have RetryCount > 0.");
                }
                if (value.MaxRetries <= 0)
                {
                    errors.Add("SagaStep in WaitingForRetry status must have MaxRetries > 0.");
                }
                break;

            case SagaStepStatus.Compensated:
                if (!value.StartedAt.HasValue)
                {
                    errors.Add("SagaStep in Compensated status must have StartedAt set.");
                }
                if (!value.CompletedAt.HasValue)
                {
                    errors.Add("SagaStep in Compensated status must have CompletedAt set.");
                }
                if (!value.CompensatedAt.HasValue)
                {
                    errors.Add("SagaStep in Compensated status must have CompensatedAt set.");
                }
                break;

            case SagaStepStatus.TimedOut:
                if (!value.StartedAt.HasValue)
                {
                    errors.Add("SagaStep in TimedOut status must have StartedAt set.");
                }
                break;

            case SagaStepStatus.Skipped:
                break;

            case SagaStepStatus.Pending:
                // Pending steps should not have timestamps set
                if (value.StartedAt.HasValue)
                {
                    errors.Add("SagaStep in Pending status must not have StartedAt set.");
                }
                if (value.CompletedAt.HasValue)
                {
                    errors.Add("SagaStep in Pending status must not have CompletedAt set.");
                }
                if (value.CompensatedAt.HasValue)
                {
                    errors.Add("SagaStep in Pending status must not have CompensatedAt set.");
                }
                break;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SagaStep"/> instance is valid.
    /// </summary>
    /// <param name="value">The saga step to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this SagaStep value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SagaStep"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed error message if it is not.
    /// </summary>
    /// <param name="value">The saga step to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid</exception>
    public static void EnsureValid(this SagaStep value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!IsValid(value))
        {
            var errors = Validate(value);
            throw new ArgumentException(
                $"SagaStep is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}