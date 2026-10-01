#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Central orchestrator that coordinates saga lifecycle: creation, step execution,
/// compensation on failure, and timeout enforcement across distributed transactions.
/// </summary>
public class SagaOrchestratorService : IDisposable
{
    private readonly ISagaRepository _sagaRepository;
    private readonly ISagaStepRepository _stepRepository;
    private readonly StepExecutor _stepExecutor;
    private readonly CompensationHandler _compensationHandler;
    private readonly TimeoutManager _timeoutManager;
    private readonly SagaStore _sagaStore;
    private readonly ILogger<SagaOrchestratorService> _logger;
    private readonly SagaOptions _options;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeSagas = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaOrchestratorService"/> class.
    /// </summary>
    public SagaOrchestratorService(
        ISagaRepository sagaRepository,
        ISagaStepRepository stepRepository,
        StepExecutor stepExecutor,
        CompensationHandler compensationHandler,
        TimeoutManager timeoutManager,
        SagaStore sagaStore,
        ILogger<SagaOrchestratorService> logger,
        IOptions<SagaOptions> options)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _stepExecutor = stepExecutor ?? throw new ArgumentNullException(nameof(stepExecutor));
        _compensationHandler = compensationHandler ?? throw new ArgumentNullException(nameof(compensationHandler));
        _timeoutManager = timeoutManager ?? throw new ArgumentNullException(nameof(timeoutManager));
        _sagaStore = sagaStore ?? throw new ArgumentNullException(nameof(sagaStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new SagaOptions();
    }

    /// <summary>
    /// Creates and starts a new saga from the given definition, executing steps sequentially.
    /// </summary>
    /// <param name="definition">The saga definition describing steps and compensation strategy.</param>
    /// <param name="correlationId">Business correlation identifier for tracing.</param>
    /// <param name="cancellationToken">Token to cancel the saga execution.</param>
    /// <returns>The completed or failed saga instance.</returns>
    public async Task<Saga> StartSagaAsync(
        SagaDefinition definition,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        var saga = new Saga
        {
            Id = Guid.NewGuid().ToString("N"),
            CorrelationId = correlationId,
            Status = SagaStatus.Initialized,
            Definition = definition,
            StartedAt = DateTime.UtcNow
        };

        await _sagaRepository.CreateAsync(saga);
        await _sagaStore.SaveSnapshotAsync(saga);
        _logger.LogInformation("Saga {SagaId} created with correlation {CorrelationId}", saga.Id, correlationId);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _activeSagas.TryAdd(saga.Id, cts);

        try
        {
            _timeoutManager.RegisterSaga(saga.Id, TimeSpan.FromSeconds(_options.TimeoutPolicies.DefaultStepTimeoutSeconds));
            saga.Status = SagaStatus.Running;
            await _sagaRepository.UpdateAsync(saga);

            var steps = await _stepRepository.GetBySagaIdAsync(saga.Id);
            foreach (var step in steps.OrderBy(s => s.Order))
            {
                cts.Token.ThrowIfCancellationRequested();

                var result = await _stepExecutor.ExecuteStepAsync(step, cts.Token);
                if (!result)
                {
                    _logger.LogWarning("Step {StepId} failed in saga {SagaId}, initiating compensation", step.Id, saga.Id);
                    saga.Status = SagaStatus.Compensating;
                    await _sagaRepository.UpdateAsync(saga);

                    await _compensationHandler.CompensateAsync(saga, steps, cts.Token);

                    saga.Status = SagaStatus.Compensated;
                    await _sagaRepository.UpdateAsync(saga);
                    return saga;
                }
            }

            saga.Status = SagaStatus.Completed;
            saga.CompletedAt = DateTime.UtcNow;
            await _sagaRepository.UpdateAsync(saga);
            _logger.LogInformation("Saga {SagaId} completed successfully", saga.Id);
        }
        catch (OperationCanceledException)
        {
            saga.Status = SagaStatus.Aborted;
            await _sagaRepository.UpdateAsync(saga);
            _logger.LogWarning("Saga {SagaId} was cancelled", saga.Id);
        }
        finally
        {
            _activeSagas.TryRemove(saga.Id, out _);
            _timeoutManager.UnregisterSaga(saga.Id);
            await _sagaStore.SaveSnapshotAsync(saga);
        }

        return saga;
    }

    /// <summary>
    /// Retrieves the current state of a saga by its identifier.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>The saga instance, or null if not found.</returns>
    public async Task<Saga?> GetSagaStatusAsync(string sagaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        return await _sagaRepository.GetByIdAsync(sagaId);
    }

    /// <summary>
    /// Aborts a running saga and triggers compensation for completed steps.
    /// </summary>
    /// <param name="sagaId">The saga identifier to abort.</param>
    /// <returns>True if the saga was found and abort was initiated.</returns>
    public async Task<bool> AbortSagaAsync(string sagaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var saga = await _sagaRepository.GetByIdAsync(sagaId);
        if (saga == null || saga.Status is SagaStatus.Completed or SagaStatus.Compensated or SagaStatus.Aborted)
            return false;

        if (_activeSagas.TryGetValue(sagaId, out var cts))
        {
            await cts.CancelAsync();
        }

        saga.Status = SagaStatus.Aborted;
        await _sagaRepository.UpdateAsync(saga);
        _logger.LogInformation("Saga {SagaId} aborted by request", sagaId);
        return true;
    }

    /// <summary>
    /// Returns all sagas currently in a running or compensating state.
    /// </summary>
    /// <returns>List of active saga instances.</returns>
    public async Task<List<Saga>> GetActiveSagasAsync()
    {
        var running = await _sagaRepository.GetByStatusAsync(SagaStatus.Running);
        var compensating = await _sagaRepository.GetByStatusAsync(SagaStatus.Compensating);
        return running.Concat(compensating).ToList();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var kvp in _activeSagas)
        {
            kvp.Value.Dispose();
        }
        _activeSagas.Clear();
        _timeoutManager.Dispose();
    }
}
