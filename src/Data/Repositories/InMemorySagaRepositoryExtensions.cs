#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Extension methods for <see cref="InMemorySagaRepository"/> providing common query and management operations.
/// </summary>
public static class InMemorySagaRepositoryExtensions
{
    /// <summary>
    /// Gets a saga by its correlation ID, returning null if not found.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="correlationId">The correlation ID to search for.</param>
    /// <returns>The saga if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown if correlationId is null or empty.</exception>
    public static async Task<Saga?> GetByCorrelationIdAsync(this InMemorySagaRepository repository, string correlationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(correlationId);
        return await repository.GetByCorrelationIdAsync(correlationId).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all sagas with the specified status.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="status">The saga status to filter by.</param>
    /// <returns>A read-only list of sagas matching the status.</returns>
    public static async Task<IReadOnlyList<Saga>> GetByStatusAsync(this InMemorySagaRepository repository, SagaStatus status)
    {
        var result = await repository.GetByStatusAsync(status).ConfigureAwait(false);
        return result.AsReadOnly();
    }

    /// <summary>
    /// Searches for sagas by definition ID.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="definitionId">The definition ID to search for.</param>
    /// <returns>A read-only list of sagas matching the definition ID.</returns>
    /// <exception cref="ArgumentException">Thrown if definitionId is null or empty.</exception>
    public static async Task<IReadOnlyList<Saga>> SearchByDefinitionIdAsync(this InMemorySagaRepository repository, string definitionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(definitionId);

        var criteria = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["definitionId"] = definitionId
        };

        var result = await repository.SearchAsync(criteria).ConfigureAwait(false);
        return result.AsReadOnly();
    }

    /// <summary>
    /// Searches for sagas by name (using the saga's definition name).
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="name">The saga name to search for (case-insensitive).</param>
    /// <returns>A read-only list of sagas matching the name.</returns>
    /// <exception cref="ArgumentException">Thrown if name is null or empty.</exception>
    public static async Task<IReadOnlyList<Saga>> SearchByNameAsync(this InMemorySagaRepository repository, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var allSagas = await repository.GetAllAsync().ConfigureAwait(false);
        var result = allSagas.Where(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets all sagas that have timed out (status is Running and timeout exceeded).
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>A read-only list of timed out sagas.</returns>
    public static async Task<IReadOnlyList<Saga>> GetTimedOutSagasAsync(this InMemorySagaRepository repository)
    {
        var allSagas = await repository.GetAllAsync().ConfigureAwait(false);
        var result = allSagas.Where(s => s.IsTimedOut()).ToList();
        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets all sagas that can be retried (status is Failed and retry count below max).
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>A read-only list of retryable sagas.</returns>
    public static async Task<IReadOnlyList<Saga>> GetRetryableSagasAsync(this InMemorySagaRepository repository)
    {
        var allSagas = await repository.GetAllAsync().ConfigureAwait(false);
        var result = allSagas.Where(s => s.CanRetry()).ToList();
        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets all sagas that failed after a specific date.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="failedAfter">The date after which failures should be considered.</param>
    /// <returns>A read-only list of sagas that failed after the specified date.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if failedAfter is in the future.</exception>
    public static async Task<IReadOnlyList<Saga>> GetFailedSagasAfterAsync(this InMemorySagaRepository repository, DateTime failedAfter)
    {
        if (failedAfter > DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(failedAfter), "Cannot search for failures in the future.");
        }

        var criteria = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["status"] = SagaStatus.Failed,
            ["failedAfter"] = failedAfter
        };

        var result = await repository.SearchAsync(criteria).ConfigureAwait(false);
        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets the count of sagas by status.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="status">The saga status to count.</param>
    /// <returns>The count of sagas with the specified status.</returns>
    public static async Task<int> CountByStatusAsync(this InMemorySagaRepository repository, SagaStatus status)
    {
        var sagas = await repository.GetByStatusAsync(status).ConfigureAwait(false);
        return sagas.Count;
    }

    /// <summary>
    /// Gets the total count of all sagas in the repository.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>The total count of sagas.</returns>
    public static async Task<int> CountAllAsync(this InMemorySagaRepository repository)
    {
        var sagas = await repository.GetAllAsync().ConfigureAwait(false);
        return sagas.Count;
    }

    /// <summary>
    /// Checks if a saga with the given correlation ID exists.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="correlationId">The correlation ID to check.</param>
    /// <returns>True if a saga with the correlation ID exists, otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown if correlationId is null or empty.</exception>
    public static async Task<bool> ExistsByCorrelationIdAsync(this InMemorySagaRepository repository, string correlationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(correlationId);
        var saga = await repository.GetByCorrelationIdAsync(correlationId).ConfigureAwait(false);
        return saga != null;
    }

    /// <summary>
    /// Gets all completed sagas.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>A read-only list of completed sagas.</returns>
    public static async Task<IReadOnlyList<Saga>> GetCompletedSagasAsync(this InMemorySagaRepository repository)
    {
        return await repository.GetByStatusAsync(SagaStatus.Completed).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all failed sagas.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>A read-only list of failed sagas.</returns>
    public static async Task<IReadOnlyList<Saga>> GetFailedSagasAsync(this InMemorySagaRepository repository)
    {
        return await repository.GetByStatusAsync(SagaStatus.Failed).ConfigureAwait(false);
    }
}