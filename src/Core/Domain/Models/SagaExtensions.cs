#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="Saga"/> to simplify common saga operations.
/// </summary>
public static class SagaExtensions
{
    /// <summary>
    /// Determines whether the saga is in a completed state (either successfully completed or compensated).
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>True if the saga is completed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static bool IsCompleted(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);
        return saga.Status is SagaStatus.Completed or SagaStatus.Compensated;
    }

    /// <summary>
    /// Determines whether the saga is in a terminal state (completed, compensated, aborted, or timed out).
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>True if the saga is in a terminal state; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static bool IsTerminal(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);
        return saga.Status is SagaStatus.Completed or SagaStatus.Compensated or SagaStatus.Aborted or SagaStatus.TimedOut;
    }

    /// <summary>
    /// Gets the duration of the saga in seconds, or null if the saga hasn't started.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>The duration in seconds, or null if not started.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static double? GetDurationSeconds(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);

        if (saga.StartedAt == default)
            return null;

        var endTime = saga.CompletedAt ?? saga.FailedAt ?? saga.CompensationStartedAt ?? DateTime.UtcNow;
        return (endTime - saga.StartedAt).TotalSeconds;
    }

    /// <summary>
    /// Gets the current step index based on the compensation strategy and completed steps.
    /// Returns -1 if no steps have been executed yet.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>The current step index to execute or compensate, or -1 if no steps exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static int GetCurrentStepIndex(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);

        if (saga.Steps.Count == 0)
            return -1;

        return saga.Status switch
        {
            SagaStatus.Running => saga.Steps.FindIndex(s => s.Status == SagaStepStatus.Pending),
            SagaStatus.Failed => saga.Definition.CompensationStrategy switch
            {
                CompensationStrategy.ReverseOrder => saga.Steps.FindLastIndex(s => s.Status is SagaStepStatus.Completed or SagaStepStatus.Failed),
                CompensationStrategy.ForwardOrder => saga.Steps.FindIndex(s => s.Status is SagaStepStatus.Completed or SagaStepStatus.Failed),
                CompensationStrategy.FromFailurePoint => saga.Steps.FindLastIndex(s => s.Status == SagaStepStatus.Failed),
                _ => -1
            },
            SagaStatus.Compensating => saga.Steps.FindLastIndex(s => s.Status == SagaStepStatus.Compensated),
            _ => -1
        };
    }

    /// <summary>
    /// Gets a read-only view of failed steps in the saga.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>An enumerable of failed steps.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static IReadOnlyList<SagaStep> GetFailedSteps(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);
        return saga.Steps.FindAll(s => s.Status == SagaStepStatus.Failed).AsReadOnly();
    }

    /// <summary>
    /// Gets a read-only view of completed steps in the saga.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>An enumerable of completed steps.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static IReadOnlyList<SagaStep> GetCompletedSteps(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);
        return saga.Steps.FindAll(s => s.Status == SagaStepStatus.Completed).AsReadOnly();
    }

    /// <summary>
    /// Determines whether the saga has any pending steps that haven't been executed.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>True if there are pending steps; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static bool HasPendingSteps(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);
        return saga.Steps.Exists(s => s.Status == SagaStepStatus.Pending);
    }

    /// <summary>
    /// Gets the retry delay in seconds for the current retry count, using exponential backoff with jitter.
    /// Returns 0 if no retry policy is configured.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>The delay in seconds, or 0 if no retry is needed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static int GetRetryDelaySeconds(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);

        if (saga.Status != SagaStatus.Failed || saga.RetryCount <= 0)
            return 0;

        // Exponential backoff: base * 2^retry_count, with jitter
        var baseDelay = 5;
        var exponentialDelay = baseDelay * Math.Pow(2, saga.RetryCount - 1);
        var jitter = new Random().Next(0, 5);
        var totalDelay = exponentialDelay + jitter;

        return (int)Math.Min(totalDelay, int.MaxValue);
    }

    /// <summary>
    /// Gets the percentage of completion (0-100) based on completed vs total steps.
    /// Returns 0 if no steps exist or if the saga hasn't started.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>The completion percentage (0-100).</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static int GetCompletionPercentage(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);

        if (saga.Steps.Count == 0)
            return 0;

        var completedSteps = saga.Steps.Count(s => s.Status == SagaStepStatus.Completed);
        return (int)Math.Round((double)completedSteps / saga.Steps.Count * 100, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Adds metadata to the saga using invariant culture for key formatting.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <exception cref="ArgumentNullException">Thrown when saga or key is null.</exception>
    public static void AddMetadata(this Saga saga, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(saga);
        ArgumentException.ThrowIfNullOrEmpty(key);

        saga.Metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets metadata value with invariant culture parsing.
    /// </summary>
    /// <typeparam name="T">The type to parse the metadata value as.</typeparam>
    /// <param name="saga">The saga instance.</param>
    /// <param name="key">The metadata key.</param>
    /// <returns>The parsed value, or default(T) if not found or parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga or key is null.</exception>
    public static T? GetMetadata<T>(this Saga saga, string key)
    {
        ArgumentNullException.ThrowIfNull(saga);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (saga.Metadata.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;

        return default;
    }

    /// <summary>
    /// Determines whether the saga is in a retryable state (failed but can still retry).
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>True if the saga can be retried; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    public static bool CanRetry(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);
        return saga.Status == SagaStatus.Failed && saga.RetryCount < saga.MaxRetries;
    }

    /// <summary>
    /// Increments the retry count and returns the new retry delay in seconds.
    /// </summary>
    /// <param name="saga">The saga instance.</param>
    /// <returns>The delay in seconds before the next retry attempt.</returns>
    /// <exception cref="ArgumentNullException">Thrown when saga is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when saga cannot be retried.</exception>
    public static int IncrementRetry(this Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);

        if (!saga.CanRetry())
            throw new InvalidOperationException("Saga cannot be retried in its current state");

        saga.RetryCount++;
        return saga.GetRetryDelaySeconds();
    }
}