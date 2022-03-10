#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="SagaStep"/> that add common functionality
/// for working with saga steps, including retry logic, status checks, and timeout handling.
/// </summary>
public static class SagaStepExtensions
{
    /// <summary>
    /// Determines if the step is in a terminal state (Completed, Failed, Compensated, Skipped, or TimedOut).
    /// </summary>
    /// <param name="step">The saga step to check</param>
    /// <returns>True if the step is in a terminal state; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static bool IsTerminal(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return step.Status is SagaStepStatus.Completed
            or SagaStepStatus.Failed
            or SagaStepStatus.Compensated
            or SagaStepStatus.Skipped
            or SagaStepStatus.TimedOut;
    }

    /// <summary>
    /// Determines if the step is in a retryable state (Failed and can be retried).
    /// </summary>
    /// <param name="step">The saga step to check</param>
    /// <returns>True if the step is in a retryable state; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static bool IsRetryable(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return step.Status == SagaStepStatus.Failed && step.CanRetry();
    }

    /// <summary>
    /// Calculates the next retry delay in milliseconds for this step based on its retry policy.
    /// Returns 0 if the step cannot be retried or has no retry policy.
    /// </summary>
    /// <param name="step">The saga step to calculate retry delay for</param>
    /// <returns>The delay in milliseconds before the next retry, or 0 if no retry is allowed</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static int GetNextRetryDelayMs(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        if (!step.IsRetryable())
        {
            return 0;
        }

        var retryPolicy = step.RetryPolicy ?? new RetryPolicy(step.MaxRetries, 1000);
        return retryPolicy.CalculateDelay(step.RetryCount + 1);
    }

    /// <summary>
    /// Gets the duration of step execution in milliseconds, or null if the step was never started.
    /// </summary>
    /// <param name="step">The saga step to get duration for</param>
    /// <returns>The execution duration in milliseconds, or null if not started</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static long? GetExecutionDurationMs(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        if (step.StartedAt == null)
        {
            return null;
        }

        var endTime = step.CompletedAt ?? DateTime.UtcNow;
        return (long)(endTime - step.StartedAt.Value).TotalMilliseconds;
    }

    /// <summary>
    /// Gets the duration of compensation in milliseconds, or null if the step was never compensated.
    /// </summary>
    /// <param name="step">The saga step to get compensation duration for</param>
    /// <returns>The compensation duration in milliseconds, or null if not compensated</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static long? GetCompensationDurationMs(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        if (step.CompensatedAt == null || step.StartedAt == null)
        {
            return null;
        }

        return (long)(step.CompensatedAt.Value - step.StartedAt.Value).TotalMilliseconds;
    }

    /// <summary>
    /// Determines if the step has timed out based on its current status and timeout configuration.
    /// </summary>
    /// <param name="step">The saga step to check</param>
    /// <returns>True if the step has timed out; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static bool HasTimedOut(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return step.Status == SagaStepStatus.Executing && step.IsTimedOut();
    }

    /// <summary>
    /// Creates a deep copy of the saga step with a new unique identifier.
    /// </summary>
    /// <param name="step">The saga step to clone</param>
    /// <returns>A new SagaStep instance with copied properties</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static SagaStep Clone(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        var clone = new SagaStep
        {
            Id = Guid.NewGuid().ToString(),
            SagaId = step.SagaId,
            Name = step.Name,
            Order = step.Order,
            Status = step.Status,
            ServiceUrl = step.ServiceUrl,
            CompensationUrl = step.CompensationUrl,
            Payload = new Dictionary<string, object>(step.Payload),
            Response = step.Response != null ? new Dictionary<string, object>(step.Response) : new Dictionary<string, object>(),
            StartedAt = step.StartedAt,
            CompletedAt = step.CompletedAt,
            CompensatedAt = step.CompensatedAt,
            ErrorMessage = step.ErrorMessage,
            RetryCount = step.RetryCount,
            MaxRetries = step.MaxRetries,
            TimeoutSeconds = step.TimeoutSeconds,
            RetryPolicy = step.RetryPolicy
        };

        return clone;
    }

    /// <summary>
    /// Updates the step's payload with new data, preserving existing keys unless explicitly overwritten.
    /// </summary>
    /// <param name="step">The saga step to update</param>
    /// <param name="data">The data to merge into the payload</param>
    /// <param name="overwriteExisting">If true, overwrites existing keys; if false, merges new keys only</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> or <paramref name="data"/> is null</exception>
    public static void UpdatePayload(this SagaStep step, Dictionary<string, object> data, bool overwriteExisting = false)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(data);

        if (step.Payload == null)
        {
            step.Payload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        foreach (var kvp in data)
        {
            if (overwriteExisting || !step.Payload.ContainsKey(kvp.Key))
            {
                step.Payload[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Gets the effective maximum retries for this step, considering both the step's MaxRetries
    /// and any per-step RetryPolicy override.
    /// </summary>
    /// <param name="step">The saga step to check</param>
    /// <returns>The effective maximum retries</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static int GetEffectiveMaxRetries(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return step.RetryPolicy?.MaxRetries ?? step.MaxRetries;
    }

    /// <summary>
    /// Gets the effective timeout in seconds for this step, considering both the step's TimeoutSeconds
    /// and any per-step RetryPolicy override.
    /// </summary>
    /// <param name="step">The saga step to check</param>
    /// <returns>The effective timeout in seconds</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null</exception>
    public static int GetEffectiveTimeoutSeconds(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return step.RetryPolicy?.MaxDelayMs > 0
            ? (int)Math.Ceiling(step.RetryPolicy.MaxDelayMs / 1000.0)
            : step.TimeoutSeconds;
    }
}