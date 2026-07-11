#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Extension methods for <see cref="SagaResponse"/> providing additional functionality
/// for working with saga responses.
/// </summary>
public static class SagaResponseExtensions
{
    /// <summary>
    /// Determines whether the saga has completed successfully.
    /// </summary>
    /// <param name="sagaResponse">The saga response to check.</param>
    /// <returns>True if the saga completed successfully; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static bool IsCompletedSuccessfully(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return string.Equals(sagaResponse.Status, "Completed", StringComparison.OrdinalIgnoreCase) &&
               sagaResponse.CompletedAt.HasValue &&
               sagaResponse.FailedSteps == 0;
    }

    /// <summary>
    /// Determines whether the saga is still in progress.
    /// </summary>
    /// <param name="sagaResponse">The saga response to check.</param>
    /// <returns>True if the saga is in progress; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static bool IsInProgress(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return !sagaResponse.CompletedAt.HasValue &&
               sagaResponse.FailedSteps < sagaResponse.StepCount &&
               !string.Equals(sagaResponse.Status, "Failed", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the saga has failed.
    /// </summary>
    /// <param name="sagaResponse">The saga response to check.</param>
    /// <returns>True if the saga has failed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static bool IsFailed(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return string.Equals(sagaResponse.Status, "Failed", StringComparison.OrdinalIgnoreCase) ||
               sagaResponse.FailedSteps > 0;
    }

    /// <summary>
    /// Calculates the total duration of the saga in milliseconds.
    /// Returns null if the saga hasn't completed yet.
    /// </summary>
    /// <param name="sagaResponse">The saga response to calculate duration for.</param>
    /// <returns>The total duration in milliseconds, or null if not completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static long? GetDurationMilliseconds(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        if (sagaResponse.CompletedAt == null)
        {
            return null;
        }

        var duration = sagaResponse.CompletedAt.Value - sagaResponse.StartedAt;
        return (long)duration.TotalMilliseconds;
    }

    /// <summary>
    /// Gets the percentage of completed steps (0-100).
    /// Returns 0 if stepCount is 0 to avoid division by zero.
    /// </summary>
    /// <param name="sagaResponse">The saga response to calculate progress for.</param>
    /// <returns>The completion percentage (0-100).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static int GetCompletionPercentage(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return sagaResponse.StepCount == 0
            ? 0
            : (int)Math.Round((double)sagaResponse.CompletedSteps / sagaResponse.StepCount * 100);
    }

    /// <summary>
    /// Gets the list of failed steps in the saga.
    /// Returns an empty list if no steps have failed.
    /// </summary>
    /// <param name="sagaResponse">The saga response to get failed steps from.</param>
    /// <returns>An enumerable of failed steps, or empty if none failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static IReadOnlyList<SagaStepResponse> GetFailedSteps(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return sagaResponse.Steps
            .Where(step => string.Equals(step.Status, "Failed", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the list of completed steps in the saga.
    /// Returns an empty list if no steps have completed.
    /// </summary>
    /// <param name="sagaResponse">The saga response to get completed steps from.</param>
    /// <returns>An enumerable of completed steps, or empty if none completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static IReadOnlyList<SagaStepResponse> GetCompletedSteps(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return sagaResponse.Steps
            .Where(step => string.Equals(step.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the list of steps that are currently in progress.
    /// Returns an empty list if no steps are in progress.
    /// </summary>
    /// <param name="sagaResponse">The saga response to get in-progress steps from.</param>
    /// <returns>An enumerable of in-progress steps, or empty if none in progress.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static IReadOnlyList<SagaStepResponse> GetInProgressSteps(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return sagaResponse.Steps
            .Where(step => string.Equals(step.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the list of steps that are pending execution.
    /// Returns an empty list if no steps are pending.
    /// </summary>
    /// <param name="sagaResponse">The saga response to get pending steps from.</param>
    /// <returns>An enumerable of pending steps, or empty if none pending.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static IReadOnlyList<SagaStepResponse> GetPendingSteps(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return sagaResponse.Steps
            .Where(step => string.Equals(step.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the average duration of completed steps in milliseconds.
    /// Returns null if no steps are completed or if the saga hasn't completed.
    /// </summary>
    /// <param name="sagaResponse">The saga response to calculate average step duration for.</param>
    /// <returns>The average duration in milliseconds, or null if not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static double? GetAverageStepDurationMilliseconds(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        if (!sagaResponse.CompletedAt.HasValue || sagaResponse.CompletedSteps == 0)
        {
            return null;
        }

        var completedSteps = sagaResponse.GetCompletedSteps();
        var totalMilliseconds = completedSteps.Sum(step => step.Duration?.TotalMilliseconds ?? 0);

        return totalMilliseconds / sagaResponse.CompletedSteps;
    }

    /// <summary>
    /// Gets the retry count for the saga as a formatted string.
    /// </summary>
    /// <param name="sagaResponse">The saga response to format retry count for.</param>
    /// <returns>A formatted string representing the retry count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static string GetRetryCountString(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return sagaResponse.RetryCount == 0
            ? "No retries"
            : $"Retried {sagaResponse.RetryCount} time{(sagaResponse.RetryCount == 1 ? "" : "s")}";
    }

    /// <summary>
    /// Gets the failure reason if the saga failed, or null if it completed successfully.
    /// </summary>
    /// <param name="sagaResponse">The saga response to check for failure.</param>
    /// <returns>The failure reason if failed, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaResponse"/> is null.</exception>
    public static string? GetFailureReasonOrDefault(this SagaResponse sagaResponse)
    {
        ArgumentNullException.ThrowIfNull(sagaResponse);

        return sagaResponse.IsFailed()
            ? sagaResponse.FailureReason ?? "Unknown failure"
            : null;
    }
}