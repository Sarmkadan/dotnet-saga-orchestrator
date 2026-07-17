#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="CompensationTransaction"/> instances.
/// </summary>
public static class CompensationTransactionValidation
{
    /// <summary>
    /// Validates the specified compensation transaction.
    /// </summary>
    /// <param name="value">The compensation transaction to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CompensationTransaction value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
            errors.Add("CompensationTransaction.Id must not be null or whitespace.");
        else if (!IsValidGuidFormat(value.Id))
            errors.Add("CompensationTransaction.Id must be a valid GUID format.");

        // Validate SagaId
        if (string.IsNullOrWhiteSpace(value.SagaId))
            errors.Add("CompensationTransaction.SagaId must not be null or whitespace.");

        // Validate StepId
        if (string.IsNullOrWhiteSpace(value.StepId))
            errors.Add("CompensationTransaction.StepId must not be null or whitespace.");

        // Validate StepName
        if (string.IsNullOrWhiteSpace(value.StepName))
            errors.Add("CompensationTransaction.StepName must not be null or whitespace.");

        // Validate Order (should be non-negative)
        if (value.Order < 0)
            errors.Add("CompensationTransaction.Order must be a non-negative integer.");

        // Validate Status (should be a valid enum value)
        if (!Enum.IsDefined(typeof(CompensationStatus), value.Status))
            errors.Add("CompensationTransaction.Status must be a valid CompensationStatus value.");

        // Validate CompensationUrl
        if (string.IsNullOrWhiteSpace(value.CompensationUrl))
            errors.Add("CompensationTransaction.CompensationUrl must not be null or whitespace.");
        else if (!Uri.IsWellFormedUriString(value.CompensationUrl, UriKind.Absolute))
            errors.Add("CompensationTransaction.CompensationUrl must be a valid absolute URI.");

        // Validate RequestPayload (should not be null)
        if (value.RequestPayload is null)
            errors.Add("CompensationTransaction.RequestPayload must not be null.");

        // Validate ResponsePayload (should not be null)
        if (value.ResponsePayload is null)
            errors.Add("CompensationTransaction.ResponsePayload must not be null.");

        // Validate InitiatedAt (should not be default DateTime)
        if (value.InitiatedAt == default)
            errors.Add("CompensationTransaction.InitiatedAt must be set to a valid DateTime.");
        else if (value.InitiatedAt.Kind != DateTimeKind.Utc)
            errors.Add("CompensationTransaction.InitiatedAt must be in UTC.");

        // Validate CompletedAt (if set, should be valid and after InitiatedAt)
        if (value.CompletedAt.HasValue)
        {
            if (value.CompletedAt.Value == default)
                errors.Add("CompensationTransaction.CompletedAt must be a valid DateTime if set.");
            else if (value.CompletedAt.Value.Kind != DateTimeKind.Utc)
                errors.Add("CompensationTransaction.CompletedAt must be in UTC if set.");
            else if (value.CompletedAt.Value < value.InitiatedAt)
                errors.Add("CompensationTransaction.CompletedAt must not be earlier than InitiatedAt.");
        }

        // Validate FailedAt (if set, should be valid and after InitiatedAt)
        if (value.FailedAt.HasValue)
        {
            if (value.FailedAt.Value == default)
                errors.Add("CompensationTransaction.FailedAt must be a valid DateTime if set.");
            else if (value.FailedAt.Value.Kind != DateTimeKind.Utc)
                errors.Add("CompensationTransaction.FailedAt must be in UTC if set.");
            else if (value.FailedAt.Value < value.InitiatedAt)
                errors.Add("CompensationTransaction.FailedAt must not be earlier than InitiatedAt.");
        }

        // Validate ErrorMessage (if set, should not be whitespace)
        if (value.ErrorMessage is not null && string.IsNullOrWhiteSpace(value.ErrorMessage.Trim()))
            errors.Add("CompensationTransaction.ErrorMessage must not be whitespace if set.");

        // Validate RetryCount (should not exceed MaxRetries)
        if (value.RetryCount < 0)
            errors.Add("CompensationTransaction.RetryCount must be a non-negative integer.");
        else if (value.MaxRetries <= 0)
            errors.Add("CompensationTransaction.MaxRetries must be a positive integer.");
        else if (value.RetryCount > value.MaxRetries)
            errors.Add("CompensationTransaction.RetryCount must not exceed MaxRetries.");

        // Validate MaxRetries (should be positive)
        if (value.MaxRetries <= 0)
            errors.Add("CompensationTransaction.MaxRetries must be a positive integer.");

        // Validate TimeoutSeconds (should be positive)
        if (value.TimeoutSeconds <= 0)
            errors.Add("CompensationTransaction.TimeoutSeconds must be a positive integer.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified compensation transaction is valid.
    /// </summary>
    /// <param name="value">The compensation transaction to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this CompensationTransaction value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified compensation transaction is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The compensation transaction to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the compensation transaction is invalid.</exception>
    /// <remarks>
    /// Calls the <see cref="Validate"/> method and throws an <see cref="ArgumentException"/> if any validation errors are found.
    /// The exception message includes all validation errors separated by newlines.
    /// </remarks>
    public static void EnsureValid(this CompensationTransaction value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
            throw new ArgumentException(
                $"CompensationTransaction is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    /// <summary>
    /// Checks if a string represents a valid GUID format.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns><see langword="true"/> if valid GUID format; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    private static bool IsValidGuidFormat(string value)
    {
        return Guid.TryParse(value, out _);
    }
}