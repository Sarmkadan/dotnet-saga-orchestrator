#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Extension methods for <see cref="CompensationService"/> providing additional functionality
/// for compensation transaction management and monitoring.
/// </summary>
public static class CompensationServiceExtensions
{
    /// <summary>
    /// Executes all pending compensation transactions for a saga sequentially.
    /// </summary>
    /// <param name="service">The <see cref="CompensationService"/> instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sagaId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <returns>List of executed compensation transactions.</returns>
    public static async Task<List<CompensationTransaction>> ExecuteAllCompensationsAsync(
        this CompensationService service,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var executedCompensations = new List<CompensationTransaction>();
        CompensationTransaction? currentCompensation;

        do
        {
            currentCompensation = await service.ExecuteNextCompensationAsync(sagaId, cancellationToken);

            if (currentCompensation != null)
            {
                executedCompensations.Add(currentCompensation);
            }
        }
        while (currentCompensation != null);

        return executedCompensations;
    }

    /// <summary>
    /// Gets compensation transactions filtered by status.
    /// </summary>
    /// <param name="service">The <see cref="CompensationService"/> instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="status">Compensation status to filter by.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sagaId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <returns>Filtered list of compensation transactions.</returns>
    public static async Task<List<CompensationTransaction>> GetCompensationsByStatusAsync(
        this CompensationService service,
        string sagaId,
        CompensationStatus status)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var allCompensations = await service.GetCompensationsAsync(sagaId);
        return allCompensations.Where(c => c.Status == status).ToList();
    }

    /// <summary>
    /// Checks if a saga has any pending compensations.
    /// </summary>
    /// <param name="service">The <see cref="CompensationService"/> instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sagaId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <returns>True if saga has pending compensations; otherwise, false.</returns>
    public static async Task<bool> HasPendingCompensationsAsync(
        this CompensationService service,
        string sagaId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var pendingCompensations = await service.GetCompensationsByStatusAsync(sagaId, CompensationStatus.Pending);
        return pendingCompensations.Count > 0;
    }

    /// <summary>
    /// Gets the count of compensation transactions for a saga.
    /// </summary>
    /// <param name="service">The <see cref="CompensationService"/> instance.</param>
    /// <param name="sagaId">The saga identifier.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sagaId"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <returns>Count of compensation transactions.</returns>
    public static async Task<int> GetCompensationCountAsync(
        this CompensationService service,
        string sagaId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var compensations = await service.GetCompensationsAsync(sagaId);
        return compensations.Count;
    }
}