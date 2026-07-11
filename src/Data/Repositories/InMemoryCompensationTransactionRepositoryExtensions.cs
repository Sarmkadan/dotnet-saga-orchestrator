#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Extension methods for <see cref="ICompensationTransactionRepository"/> providing additional compensation transaction query functionality.
/// </summary>
public static class InMemoryCompensationTransactionRepositoryExtensions
{
    /// <summary>
    /// Gets the first compensation transaction by saga ID and status, ordered by order descending.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga ID to filter by.</param>
    /// <param name="status">The compensation status to filter by.</param>
    /// <returns>The first matching compensation transaction or null if not found.</returns>
/// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<CompensationTransaction?> GetFirstBySagaIdAndStatusAsync(
        this ICompensationTransactionRepository repository,
        string sagaId,
        CompensationStatus status)
    {
    ArgumentNullException.ThrowIfNull(repository);

        var transactions = await repository.GetBySagaIdAsync(sagaId);
        return transactions.FirstOrDefault(t => t.Status == status);
    }

    /// <summary>
    /// Gets all compensation transactions by saga ID and status, ordered by order descending.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sagaId">The saga ID to filter by.</param>
    /// <param name="status">The compensation status to filter by.</param>
    /// <returns>List of matching compensation transactions.</returns>

/// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
/// <exception cref="ArgumentException"><paramref name="sagaId"/> is null or empty.</exception>
    public static async Task<List<CompensationTransaction>> GetBySagaIdAndStatusAsync(
        this ICompensationTransactionRepository repository,
        string sagaId,
        CompensationStatus status)
    {
    ArgumentNullException.ThrowIfNull(repository);

    ArgumentNullException.ThrowIfNullOrEmpty(sagaId);

        var transactions = await repository.GetBySagaIdAsync(sagaId);
        return transactions.Where(t => t.Status == status).ToList();
    }

    /// <summary>
    /// Gets all compensation transactions with the specified status across all sagas.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="status">The compensation status to filter by.</param>
    /// <returns>List of matching compensation transactions.</returns>

/// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<List<CompensationTransaction>> GetByStatusAsync(
        this ICompensationTransactionRepository repository,
        CompensationStatus status)
    {
    ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetByStatusAsync(status);
    }

    /// <summary>
    /// Gets the count of compensation transactions with the specified status.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="status">The compensation status to filter by.</param>
    /// <returns>The count of matching compensation transactions.</returns>

/// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<int> CountByStatusAsync(
        this ICompensationTransactionRepository repository,
        CompensationStatus status)
    {
    ArgumentNullException.ThrowIfNull(repository);

        var transactions = await repository.GetByStatusAsync(status);
        return transactions.Count;
    }

    /// <summary>
    /// Gets all compensation transactions that are in a terminal state (completed, failed, or timed out).
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>List of compensation transactions in terminal states.</returns>

/// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<List<CompensationTransaction>> GetTerminalTransactionsAsync(
        this ICompensationTransactionRepository repository)
    {
    ArgumentNullException.ThrowIfNull(repository);

        var all = await repository.GetAllAsync();
        return all.Where(t => t.Status == CompensationStatus.Completed
                          || t.Status == CompensationStatus.Failed
                          || t.Status == CompensationStatus.TimedOut).ToList();
    }

    /// <summary>
    /// Gets all compensation transactions that are still active (pending or in progress).
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>List of active compensation transactions.</returns>

/// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<List<CompensationTransaction>> GetActiveTransactionsAsync(
        this ICompensationTransactionRepository repository)
    {
    ArgumentNullException.ThrowIfNull(repository);

        var all = await repository.GetAllAsync();
        return all.Where(t => t.Status == CompensationStatus.Pending
                          || t.Status == CompensationStatus.InProgress).ToList();
    }
}