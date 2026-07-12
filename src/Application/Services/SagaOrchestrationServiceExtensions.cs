using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides extension methods for <see cref="SagaOrchestrationService"/>.
/// </summary>
public static class SagaOrchestrationServiceExtensions
{
    /// <summary>
    /// Retrieves a saga by ID safely, returning null if the saga does not exist, instead of throwing an exception.
    /// </summary>
    /// <param name="service">The saga orchestration service instance.</param>
    /// <param name="sagaId">The unique identifier of the saga.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="Saga"/> object if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> is null or empty.</exception>
    public static async Task<Saga?> GetSagaSafeAsync(
        this SagaOrchestrationService service,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));
        ArgumentException.ThrowIfNullOrEmpty(sagaId, nameof(sagaId));

        try
        {
            return await service.GetSagaAsync(sagaId);
        }
        catch (SagaNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Lists all sagas currently in the 'Running' status.
    /// </summary>
    /// <param name="service">The saga orchestration service instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of sagas currently running.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<IReadOnlyList<Saga>> ListRunningSagasAsync(
        this SagaOrchestrationService service,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));

        var sagas = await service.ListSagasAsync(status: SagaStatus.Running);
        return sagas.ToList().AsReadOnly();
    }

    /// <summary>
    /// Lists all sagas currently in the 'Failed' status.
    /// </summary>
    /// <param name="service">The saga orchestration service instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of sagas currently in a failed state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<IReadOnlyList<Saga>> ListFailedSagasAsync(
        this SagaOrchestrationService service,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));

        var sagas = await service.ListSagasAsync(status: SagaStatus.Failed);
        return sagas.ToList().AsReadOnly();
    }
}
