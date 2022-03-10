#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Extension methods for <see cref="InMemorySagaStepRepository"/> providing
/// additional query and utility operations.
/// </summary>
public static class InMemorySagaStepRepositoryExtensions
{
    /// <summary>
    /// Gets all saga steps that belong to a specific saga and have a specific status.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="status">The status to filter by.</param>
    /// <returns>A list of matching saga steps, ordered by execution order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when sagaId is null or empty.</exception>
    public static async Task<IReadOnlyList<SagaStep>> GetBySagaIdAndStatusAsync(
        this InMemorySagaStepRepository repository,
        string sagaId,
        SagaStepStatus status)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);

        var steps = await repository.GetBySagaIdAsync(sagaId);
        return steps.Where(s => s.Status == status).OrderBy(s => s.Order).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the next pending step for a specific saga, ordered by execution order.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>The next pending step, or null if none exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when sagaId is null or empty.</exception>
    public static async Task<SagaStep?> GetNextPendingStepAsync(
        this InMemorySagaStepRepository repository,
        string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);

        var steps = await repository.GetBySagaIdAsync(sagaId);
        return steps
            .Where(s => s.Status == SagaStepStatus.Pending)
            .OrderBy(s => s.Order)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets all steps for a saga that are in a failed state and can be retried.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>A list of retryable failed steps, ordered by execution order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when sagaId is null or empty.</exception>
    public static async Task<IReadOnlyList<SagaStep>> GetRetryableFailedStepsAsync(
        this InMemorySagaStepRepository repository,
        string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);

        var steps = await repository.GetBySagaIdAsync(sagaId);
        return steps
            .Where(s => s.Status == SagaStepStatus.Failed && s.CanRetry())
            .OrderBy(s => s.Order)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets all steps for a saga that have timed out during execution.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>A list of timed out steps, ordered by execution order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when sagaId is null or empty.</exception>
    public static async Task<IReadOnlyList<SagaStep>> GetTimedOutStepsAsync(
        this InMemorySagaStepRepository repository,
        string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);

        var steps = await repository.GetBySagaIdAsync(sagaId);
        return steps
            .Where(s => s.IsTimedOut())
            .OrderBy(s => s.Order)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the highest order number for steps belonging to a specific saga.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>The highest order number, or 0 if no steps exist for the saga.</returns>
    /// <exception cref="ArgumentNullException">Thrown when sagaId is null or empty.</exception>
    public static async Task<int> GetMaxOrderForSagaAsync(
        this InMemorySagaStepRepository repository,
        string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);

        var steps = await repository.GetBySagaIdAsync(sagaId);
        return steps.Count > 0 ? steps.Max(s => s.Order) : 0;
    }

    /// <summary>
    /// Checks if all steps for a saga have been completed successfully.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>True if all steps are completed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when sagaId is null or empty.</exception>
    public static async Task<bool> AreAllStepsCompletedAsync(
        this InMemorySagaStepRepository repository,
        string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);

        var steps = await repository.GetBySagaIdAsync(sagaId);
        return steps.Count > 0 && steps.All(s => s.Status == SagaStepStatus.Completed);
    }

    /// <summary>
    /// Gets all steps for a saga that are currently executing or waiting for retry.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>A list of active steps, ordered by execution order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when sagaId is null or empty.</exception>
    public static async Task<IReadOnlyList<SagaStep>> GetActiveStepsAsync(
        this InMemorySagaStepRepository repository,
        string sagaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);

        var steps = await repository.GetBySagaIdAsync(sagaId);
        return steps
            .Where(s => s.Status == SagaStepStatus.Executing || s.Status == SagaStepStatus.WaitingForRetry)
            .OrderBy(s => s.Order)
            .ToList()
            .AsReadOnly();
    }
}