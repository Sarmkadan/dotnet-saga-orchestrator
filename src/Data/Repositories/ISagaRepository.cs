#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Repository interface for saga persistence operations. Provides CRUD access to saga
/// instances with filtering by status, correlation ID, and arbitrary search criteria.
/// Implementations must ensure that saga state transitions are persisted atomically.
/// </summary>
public interface ISagaRepository
{
    /// <summary>
    /// Retrieves a saga by its unique identifier.
    /// </summary>
    /// <param name="id">The saga instance identifier.</param>
    /// <returns>The matching <see cref="Saga"/>, or <c>null</c> if not found.</returns>
    Task<Saga?> GetByIdAsync(string id);

    /// <summary>
    /// Retrieves a saga by its business correlation identifier, which links the saga
    /// to the originating business transaction.
    /// </summary>
    /// <param name="correlationId">The business correlation identifier.</param>
    /// <returns>The matching <see cref="Saga"/>, or <c>null</c> if not found.</returns>
    Task<Saga?> GetByCorrelationIdAsync(string correlationId);

    /// <summary>
    /// Persists a new saga instance.
    /// </summary>
    /// <param name="saga">The saga to create.</param>
    /// <returns>The created <see cref="Saga"/> with any server-assigned fields populated.</returns>
    Task<Saga?> CreateAsync(Saga saga);

    /// <summary>
    /// Updates an existing saga instance, including step states and compensation data.
    /// </summary>
    /// <param name="saga">The saga with updated state to persist.</param>
    /// <returns>The updated <see cref="Saga"/>.</returns>
    Task<Saga?> UpdateAsync(Saga saga);

    /// <summary>
    /// Permanently deletes a saga instance by its identifier.
    /// </summary>
    /// <param name="id">The saga instance identifier to delete.</param>
    /// <returns><c>true</c> if the saga was found and deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Returns all saga instances in the repository.
    /// </summary>
    /// <returns>A list of all <see cref="Saga"/> instances.</returns>
    Task<List<Saga>> GetAllAsync();

    /// <summary>
    /// Returns all saga instances in the specified status (e.g., Running, Completed, Failed, Compensating).
    /// </summary>
    /// <param name="status">The saga status to filter by.</param>
    /// <returns>A list of sagas matching the status filter.</returns>
    Task<List<Saga>> GetByStatusAsync(Core.Domain.Enums.SagaStatus status);

    /// <summary>
    /// Searches for sagas matching the provided key-value criteria. Criteria keys correspond
    /// to saga property names and values are matched using equality comparison.
    /// </summary>
    /// <param name="criteria">A dictionary of property-name to value pairs to match against.</param>
    /// <returns>A list of sagas matching all specified criteria.</returns>
    Task<List<Saga>> SearchAsync(Dictionary<string, object> criteria);
}
