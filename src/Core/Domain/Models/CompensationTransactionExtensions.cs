#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Extension methods for <see cref="CompensationTransaction"/> providing additional functionality
/// for working with compensation transactions in saga orchestration scenarios.
/// </summary>
public static class CompensationTransactionExtensions
{
    /// <summary>
    /// Determines if the compensation transaction is still active and can be processed.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>True if the transaction is active; otherwise false</returns>
    public static bool IsActive(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return transaction.Status is CompensationStatus.Pending or CompensationStatus.InProgress;
    }

    /// <summary>
    /// Determines if the compensation transaction has completed successfully.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>True if the transaction completed successfully; otherwise false</returns>
    public static bool IsCompletedSuccessfully(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return transaction.Status == CompensationStatus.Completed &&
               transaction.CompletedAt.HasValue;
    }

    /// <summary>
    /// Determines if the compensation transaction has failed.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>True if the transaction failed; otherwise false</returns>
    public static bool IsFailed(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return transaction.Status == CompensationStatus.Failed &&
               transaction.FailedAt.HasValue;
    }

    /// <summary>
    /// Gets the duration of the compensation transaction in milliseconds.
    /// Returns null if the transaction hasn't completed or failed yet.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>Duration in milliseconds or null if not completed</returns>
    public static long? GetDurationMs(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction.CompletedAt.HasValue)
            return (long)(transaction.CompletedAt.Value - transaction.InitiatedAt).TotalMilliseconds;

        if (transaction.FailedAt.HasValue)
            return (long)(transaction.FailedAt.Value - transaction.InitiatedAt).TotalMilliseconds;

        return null;
    }

    /// <summary>
    /// Gets the elapsed time of the compensation transaction in milliseconds.
    /// Returns null if the transaction hasn't started yet.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>Elapsed time in milliseconds or null if not started</returns>
    public static long? GetElapsedTimeMs(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction.Status == CompensationStatus.Pending)
            return null;

        var endTime = transaction.CompletedAt ?? transaction.FailedAt ?? DateTime.UtcNow;
        return (long)(endTime - transaction.InitiatedAt).TotalMilliseconds;
    }

    /// <summary>
    /// Creates a deep copy of the compensation transaction.
    /// </summary>
    /// <param name="transaction">The compensation transaction to copy</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>A new CompensationTransaction instance with copied values</returns>
    public static CompensationTransaction DeepCopy(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var copy = new CompensationTransaction
        {
            Id = transaction.Id,
            SagaId = transaction.SagaId,
            StepId = transaction.StepId,
            StepName = transaction.StepName ?? string.Empty,
            Order = transaction.Order,
            Status = transaction.Status,
            CompensationUrl = transaction.CompensationUrl ?? string.Empty,
            RequestPayload = transaction.RequestPayload != null
                ? new Dictionary<string, object>(transaction.RequestPayload, StringComparer.Ordinal)
                : new Dictionary<string, object>(StringComparer.Ordinal),
            ResponsePayload = transaction.ResponsePayload != null
                ? new Dictionary<string, object>(transaction.ResponsePayload, StringComparer.Ordinal)
                : new Dictionary<string, object>(StringComparer.Ordinal),
            InitiatedAt = transaction.InitiatedAt,
            CompletedAt = transaction.CompletedAt,
            FailedAt = transaction.FailedAt,
            ErrorMessage = transaction.ErrorMessage,
            RetryCount = transaction.RetryCount,
            MaxRetries = transaction.MaxRetries,
            TimeoutSeconds = transaction.TimeoutSeconds
        };

        return copy;
    }

    /// <summary>
    /// Updates the request payload with new values, preserving existing keys unless explicitly overridden.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <param name="payloadUpdates">Dictionary of key-value pairs to update or add</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="transaction"/> is null
    /// <para>-or-</para>
    /// <paramref name="payloadUpdates"/> is null
    /// </exception>
    public static void UpdateRequestPayload(this CompensationTransaction transaction, Dictionary<string, object> payloadUpdates)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(payloadUpdates);

        transaction.RequestPayload ??= new Dictionary<string, object>(StringComparer.Ordinal);

        foreach (var kvp in payloadUpdates)
        {
            transaction.RequestPayload[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Checks if the compensation transaction has exceeded its maximum retry count.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>True if max retries exceeded; otherwise false</returns>
    public static bool HasExceededMaxRetries(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return transaction.RetryCount >= transaction.MaxRetries;
    }

    /// <summary>
    /// Gets a summary string representation of the compensation transaction.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>Formatted summary string</returns>
    public static string GetSummary(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return $"CompensationTransaction [Id={transaction.Id}, SagaId={transaction.SagaId}, Step={transaction.StepName ?? "null"}, Status={transaction.Status}, Order={transaction.Order}]";
    }

    /// <summary>
    /// Determines if the compensation transaction can be safely retried based on its current state.
    /// </summary>
    /// <param name="transaction">The compensation transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is null</exception>
    /// <returns>True if retry is safe; otherwise false</returns>
    public static bool CanSafelyRetry(this CompensationTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return transaction.IsFailed() && !transaction.HasExceededMaxRetries();
    }
}