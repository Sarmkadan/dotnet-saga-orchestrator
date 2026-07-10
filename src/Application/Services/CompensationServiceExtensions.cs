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
/// Extension methods for CompensationService providing additional functionality
/// for compensation transaction management and monitoring.
/// </summary>
public static class CompensationServiceExtensions
{
    /// <summary>
    /// Executes all pending compensation transactions for a saga sequentially
    /// </summary>
    /// <param name="service">The CompensationService instance</param>
    /// <param name="sagaId">The saga identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of executed compensation transactions</returns>
    public static async Task<List<CompensationTransaction>> ExecuteAllCompensationsAsync(
        this CompensationService service,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

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
    /// Gets compensation transactions filtered by status
    /// </summary>
    /// <param name="service">The CompensationService instance</param>
    /// <param name="sagaId">The saga identifier</param>
    /// <param name="status">Compensation status to filter by</param>
    /// <returns>Filtered list of compensation transactions</returns>
    public static async Task<List<CompensationTransaction>> GetCompensationsByStatusAsync(
        this CompensationService service,
        string sagaId,
        CompensationStatus status)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

        var allCompensations = await service.GetCompensationsAsync(sagaId);
        return allCompensations.Where(c => c.Status == status).ToList();
    }

    /// <summary>
    /// Checks if a saga has any pending compensations
    /// </summary>
    /// <param name="service">The CompensationService instance</param>
    /// <param name="sagaId">The saga identifier</param>
    /// <returns>True if saga has pending compensations, false otherwise</returns>
    public static async Task<bool> HasPendingCompensationsAsync(
        this CompensationService service,
        string sagaId)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

        var pendingCompensations = await service.GetCompensationsByStatusAsync(sagaId, CompensationStatus.Pending);
        return pendingCompensations.Count > 0;
    }

    /// <summary>
    /// Gets the count of compensation transactions for a saga
    /// </summary>
    /// <param name="service">The CompensationService instance</param>
    /// <param name="sagaId">The saga identifier</param>
    /// <returns>Count of compensation transactions</returns>
    public static async Task<int> GetCompensationCountAsync(
        this CompensationService service,
        string sagaId)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

        var compensations = await service.GetCompensationsAsync(sagaId);
        return compensations.Count;
    }
}