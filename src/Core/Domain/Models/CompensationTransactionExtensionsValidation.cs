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
        ValidateMethodCall(
            transaction,
            static t => t.IsActive(),
            nameof(CompensationTransactionExtensions.IsActive),
            errors);

        // Validate IsCompletedSuccessfully result
        ValidateMethodCall(
            transaction,
            static t => t.IsCompletedSuccessfully(),
            nameof(CompensationTransactionExtensions.IsCompletedSuccessfully),
            errors);

        // Validate IsFailed result
        ValidateMethodCall(
            transaction,
            static t => t.IsFailed(),
            nameof(CompensationTransactionExtensions.IsFailed),
            errors);

        // Validate GetDurationMs - should be non-negative if not null
        ValidateMethodCall(
            transaction,
            static t => t.GetDurationMs(),
            nameof(CompensationTransactionExtensions.GetDurationMs),
            duration => duration.HasValue && duration <= 0,
            errors);

        // Validate GetElapsedTimeMs - should be non-negative if not null
        ValidateMethodCall(
            transaction,
            static t => t.GetElapsedTimeMs(),
            nameof(CompensationTransactionExtensions.GetElapsedTimeMs),
            elapsed => elapsed.HasValue && elapsed <= 0,
            errors);

        // Validate DeepCopy - should not be null
        ValidateMethodCall(
            transaction,
            static t => t.DeepCopy(),
            nameof(CompensationTransactionExtensions.DeepCopy),
            result => result is null,
            errors);

        // Validate UpdateRequestPayload - should not throw
        ValidateMethodCall(
            transaction,
            static t => t.UpdateRequestPayload([]),
            nameof(CompensationTransactionExtensions.UpdateRequestPayload),
            errors);

        // Validate HasExceededMaxRetries result
        ValidateMethodCall(
            transaction,
            static t => t.HasExceededMaxRetries(),
            nameof(CompensationTransactionExtensions.HasExceededMaxRetries),
            errors);

        // Validate GetSummary - should return non-null, non-empty string
        ValidateMethodCall(
            transaction,
            static t => t.GetSummary(),
            nameof(CompensationTransactionExtensions.GetSummary),
            result => string.IsNullOrEmpty(result),
            errors);

        // Validate CanSafelyRetry result
        ValidateMethodCall(
            transaction,
            static t => t.CanSafelyRetry(),
            nameof(CompensationTransactionExtensions.CanSafelyRetry),
            errors);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CompensationTransaction"/> can have its extension methods called safely.
    /// </summary>
    /// <param name="transaction">The transaction to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transaction"/> is <see langword="null"/>.</exception>
    public static bool IsValidExtensionMethods(this CompensationTransaction transaction) =>
        transaction.ValidateExtensionMethods().Count == 0;

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
                $"CompensationTransaction extension methods validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    private static void ValidateMethodCall<TResult>(
        CompensationTransaction transaction,
        Func<CompensationTransaction, TResult> methodCall,
        string methodName,
        List<string> errors)
    {
        try
        {
            _ = methodCall(transaction);
        }
        catch (Exception ex)
        {
            errors.Add($"{methodName}() threw an exception: {ex.Message}");
        }
    }

    private static void ValidateMethodCall(
        CompensationTransaction transaction,
        Action<CompensationTransaction> methodCall,
        string methodName,
        List<string> errors)
    {
        try
        {
            methodCall(transaction);
        }
        catch (Exception ex)
        {
            errors.Add($"{methodName}() threw an exception: {ex.Message}");
        }
    }

    private static void ValidateMethodCall<TResult>(
        CompensationTransaction transaction,
        Func<CompensationTransaction, TResult> methodCall,
        string methodName,
        Func<TResult, bool> validationPredicate,
        List<string> errors)
    {
        try
        {
            var result = methodCall(transaction);
            if (validationPredicate(result))
            {
                errors.Add($"{methodName}() must return a valid result, but validation failed.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"{methodName}() threw an exception: {ex.Message}");
        }
    }
}