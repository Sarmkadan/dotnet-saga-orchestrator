#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Data.Repositories;

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
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public SagaTimeoutWorker(
        ISagaRepository sagaRepository,
        SagaOrchestrationService orchestrationService,
        ILogger<SagaTimeoutWorker> logger)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _orchestrationService = orchestrationService ?? throw new ArgumentNullException(nameof(orchestrationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                // Check individual step timeouts
                var executingSteps = saga.Steps.Where(s => s.Status.ToString() == "Executing").ToList();
                foreach (var step in executingSteps)
                {
                    var stepElapsed = DateTime.UtcNow - step.StartedAt;
                    if (stepElapsed > TimeSpan.FromSeconds(step.TimeoutSeconds))
                    {
                        _logger.LogWarning("Step {StepId} in saga {SagaId} has timed out.", step.Id, saga.Id);
                        // Mark step as failed, which triggers compensation
                        step.Status = SagaStepStatus.TimedOut;
                        await _sagaRepository.UpdateAsync(saga);
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
