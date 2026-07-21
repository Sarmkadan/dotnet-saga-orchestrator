#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Events;

namespace SagaOrchestrator.Infrastructure.BackgroundWorkers;

/// <summary>
/// Background worker that monitors and handles saga timeouts.
/// Periodically checks running sagas and applies timeout policies.
/// </summary>
public class SagaTimeoutWorker : BackgroundService
{
    private readonly ISagaRepository _sagaRepository;
    private readonly SagaOrchestrationService _orchestrationService;
    private readonly ILogger<SagaTimeoutWorker> _logger;
    private readonly IEventBus _eventBus;
    private readonly SagaOptions _sagaOptions;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public SagaTimeoutWorker(
        ISagaRepository sagaRepository,
        SagaOrchestrationService orchestrationService,
        ILogger<SagaTimeoutWorker> logger,
        IEventBus eventBus,
        SagaOptions sagaOptions)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _orchestrationService = orchestrationService ?? throw new ArgumentNullException(nameof(orchestrationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _sagaOptions = sagaOptions ?? throw new ArgumentNullException(nameof(sagaOptions));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Saga Timeout Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndHandleTimeoutsAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in timeout worker");
            }
        }

        _logger.LogInformation("Saga Timeout Worker stopped");
    }

    private async Task CheckAndHandleTimeoutsAsync(CancellationToken stoppingToken)
    {
        var sagas = await _sagaRepository.GetAllAsync();
        var runningOrCompensating = sagas.Where(s =>
            s.Status == SagaStatus.Running || s.Status == SagaStatus.Compensating).ToList();

        foreach (var saga in runningOrCompensating)
        {
            try
            {
                var elapsedTime = DateTime.UtcNow - saga.CreatedAt;
                if (elapsedTime > TimeSpan.FromSeconds(saga.TimeoutSeconds))
                {
                    _logger.LogWarning("Saga {SagaId} has exceeded timeout limit. Aborting.", saga.Id);
                    await _orchestrationService.AbortSagaAsync(saga.Id);
                }


            // Check for stale saga detection
            if (saga.Status == SagaStatus.Running)
            {
                var staleTimeout = TimeSpan.FromSeconds(_sagaOptions.TimeoutPolicies.StaleSagaTimeoutSeconds);
                var staleDuration = DateTime.UtcNow - saga.StartedAt;

                if (staleDuration > staleTimeout)
                {
                    _logger.LogWarning("Saga {SagaId} has been running for {StaleDurationSeconds} seconds and is considered stale. Publishing stale saga event.",
                        saga.Id, staleDuration.TotalSeconds);

                    var staleEvent = new SagaStaleEvent
                    {
                        SagaId = saga.Id,
                        SagaName = saga.Name,
                        StaleDurationSeconds = (long)staleDuration.TotalSeconds,
                        StaleAt = DateTime.UtcNow
                    };

                    await _eventBus.PublishAsync(staleEvent);
                }
            }

                // Check individual step timeouts
                var executingSteps = saga.Steps.Where(s => s.Status == SagaStepStatus.Executing).ToList();
                foreach (var step in executingSteps)
                {
                    if (step.IsTimedOut())
                    {
                        _logger.LogWarning("Step {StepId} in saga {SagaId} has timed out. Triggering compensation.", step.Id, saga.Id);
                        await _orchestrationService.HandleTimeoutAsync(saga.Id, step.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling timeout for saga {SagaId}", saga.Id);
            }
        }
    }
}
