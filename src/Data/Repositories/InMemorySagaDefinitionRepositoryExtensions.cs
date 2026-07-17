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
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Provides extension methods for <see cref="InMemorySagaDefinitionRepository"/> to simplify common operations.
/// </summary>
public static class InMemorySagaDefinitionRepositoryExtensions
{
    /// <summary>
    /// Gets a saga definition by its name, returning null if not found.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="name">The name of the saga definition to retrieve.</param>
    /// <returns>The saga definition if found, null otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public static async Task<SagaDefinition?> GetByNameAsync(this InMemorySagaDefinitionRepository repository, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return await repository.GetByNameAsync(name).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all saga definitions that match the specified criteria.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>A list of matching saga definitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
    public static async Task<IReadOnlyList<SagaDefinition>> GetAllAsync(this InMemorySagaDefinitionRepository repository, Func<SagaDefinition, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var allDefinitions = await repository.GetAllAsync().ConfigureAwait(false);
        return allDefinitions.Where(predicate).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all active saga definitions.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>A list of active saga definitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    public static async Task<IReadOnlyList<SagaDefinition>> GetActiveAsync(this InMemorySagaDefinitionRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        return await repository.GetActiveAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Searches for saga definitions by name using a partial match.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="nameFragment">The fragment to search for in saga definition names.</param>
    /// <returns>A list of matching saga definitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="nameFragment"/> is null.</exception>
    public static async Task<IReadOnlyList<SagaDefinition>> SearchByNameAsync(this InMemorySagaDefinitionRepository repository, string nameFragment)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(nameFragment);

        var allDefinitions = await repository.GetAllAsync().ConfigureAwait(false);
        return allDefinitions
            .Where(d => d.Name.Contains(nameFragment, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets all saga definitions with a specific version number.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="version">The version number to filter by.</param>
    /// <returns>A list of saga definitions with the specified version.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    public static async Task<IReadOnlyList<SagaDefinition>> GetByVersionAsync(this InMemorySagaDefinitionRepository repository, int version)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var allDefinitions = await repository.GetAllAsync().ConfigureAwait(false);
        return allDefinitions
            .Where(d => d.Version == version)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the latest version of a saga definition by name.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="name">The name of the saga definition.</param>
    /// <returns>The latest version of the saga definition, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public static async Task<SagaDefinition?> GetLatestVersionAsync(this InMemorySagaDefinitionRepository repository, string name)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(name);

        var allDefinitions = await repository.GetAllAsync().ConfigureAwait(false);
        return allDefinitions
            .Where(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(d => d.Version)
            .FirstOrDefault();
    }

    /// <summary>
    /// Determines whether a saga definition with the specified name exists.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="name">The name of the saga definition to check.</param>
    /// <returns>True if a saga definition with the name exists, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public static async Task<bool> ExistsByNameAsync(this InMemorySagaDefinitionRepository repository, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return await repository.GetByNameAsync(name) is not null;
    }

    /// <summary>
    /// Gets all saga definitions created after a specific date.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="cutoffDate">The date threshold for filtering saga definitions.</param>
    /// <returns>A list of saga definitions created after the cutoff date.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    public static async Task<IReadOnlyList<SagaDefinition>> GetCreatedAfterAsync(this InMemorySagaDefinitionRepository repository, DateTime cutoffDate)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var allDefinitions = await repository.GetAllAsync().ConfigureAwait(false);
        return allDefinitions
            .Where(d => d.CreatedAt > cutoffDate)
            .OrderBy(d => d.CreatedAt)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the total count of saga definitions in the repository.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>The total number of saga definitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    public static async Task<int> CountAsync(this InMemorySagaDefinitionRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var allDefinitions = await repository.GetAllAsync().ConfigureAwait(false);
        return allDefinitions.Count;
    }

    /// <summary>
    /// Gets the count of active saga definitions in the repository.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <returns>The number of active saga definitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    public static async Task<int> CountActiveAsync(this InMemorySagaDefinitionRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var activeDefinitions = await repository.GetActiveAsync().ConfigureAwait(false);
        return activeDefinitions.Count;
    }
}