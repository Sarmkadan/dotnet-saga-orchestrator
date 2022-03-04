using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="CompensationTransactionExtensions"/> extension methods.
/// </summary>
public static class CompensationTransactionExtensionsValidation
{
    /// <summary>
    /// Validates that the specified <see cref="CompensationTransaction"/> can have its extension methods called safely.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transaction"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateExtensionMethods(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var errors = new List<string>();

        // Validate IsActive result
        try
        {
            _ = transaction.IsActive();
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.IsActive)}() threw an exception: {ex.Message}");
        }

        // Validate IsCompletedSuccessfully result
        try
        {
            _ = transaction.IsCompletedSuccessfully();
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.IsCompletedSuccessfully)}() threw an exception: {ex.Message}");
        }

        // Validate IsFailed result
        try
        {
            _ = transaction.IsFailed();
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.IsFailed)}() threw an exception: {ex.Message}");
        }

        // Validate GetDurationMs - should be non-negative if not null
        try
        {
            var durationMs = transaction.GetDurationMs();
            if (durationMs.HasValue && durationMs <= 0)
            {
                errors.Add($"{nameof(CompensationTransactionExtensions.GetDurationMs)}() must return a positive value when set, but returned {durationMs}.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.GetDurationMs)}() threw an exception: {ex.Message}");
        }

        // Validate GetElapsedTimeMs - should be non-negative if not null
        try
        {
            var elapsedTimeMs = transaction.GetElapsedTimeMs();
            if (elapsedTimeMs.HasValue && elapsedTimeMs <= 0)
            {
                errors.Add($"{nameof(CompensationTransactionExtensions.GetElapsedTimeMs)}() must return a positive value when set, but returned {elapsedTimeMs}.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.GetElapsedTimeMs)}() threw an exception: {ex.Message}");
        }

        // Validate DeepCopy - should not be null
        try
        {
            var copy = transaction.DeepCopy();
            if (copy is null)
            {
                errors.Add($"{nameof(CompensationTransactionExtensions.DeepCopy)}() returned null.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.DeepCopy)}() threw an exception: {ex.Message}");
        }

        // Validate UpdateRequestPayload - should not throw
        try
        {
            transaction.UpdateRequestPayload([]);
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.UpdateRequestPayload)}() threw an exception: {ex.Message}");
        }

        // Validate HasExceededMaxRetries result
        try
        {
            _ = transaction.HasExceededMaxRetries();
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.HasExceededMaxRetries)}() threw an exception: {ex.Message}");
        }

        // Validate GetSummary - should return non-null, non-empty string
        try
        {
            var summary = transaction.GetSummary();
            if (string.IsNullOrEmpty(summary))
            {
                errors.Add($"{nameof(CompensationTransactionExtensions.GetSummary)}() returned null or empty string.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.GetSummary)}() threw an exception: {ex.Message}");
        }

        // Validate CanSafelyRetry result
        try
        {
            _ = transaction.CanSafelyRetry();
        }
        catch (Exception ex)
        {
            errors.Add($"{nameof(CompensationTransactionExtensions.CanSafelyRetry)}() threw an exception: {ex.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CompensationTransaction"/> can have its extension methods called safely.
    /// </summary>
    /// <param name="transaction">The transaction to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transaction"/> is <see langword="null"/>.</exception>
    public static bool IsValidExtensionMethods(this CompensationTransaction transaction)
    {
        return transaction.ValidateExtensionMethods().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CompensationTransaction"/> can have its extension methods called safely.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transaction"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the transaction is not valid, containing a list of validation errors.</exception>
    public static void EnsureValidExtensionMethods(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var errors = transaction.ValidateExtensionMethods();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"CompensationTransaction extension methods validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}
