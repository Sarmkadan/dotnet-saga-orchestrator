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
/// Background worker that processes pending compensation transactions.
/// Monitors failed sagas and initiates compensation chains automatically.
/// </summary>
public class CompensationWorker : BackgroundService
{
    private readonly ISagaRepository _sagaRepository;
    private readonly CompensationService _compensationService;
    private readonly ILogger<CompensationWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15);

    public CompensationWorker(
        ISagaRepository sagaRepository,
        CompensationService compensationService,
        ILogger<CompensationWorker> logger)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _compensationService = compensationService ?? throw new ArgumentNullException(nameof(compensationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Compensation Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessCompensationsAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in compensation worker");
            }
        }

        _logger.LogInformation("Compensation Worker stopped");
    }

    private async Task ProcessCompensationsAsync(CancellationToken stoppingToken)
    {
        var allSagas = await _sagaRepository.GetAllAsync();
        var failedSagas = allSagas.Where(s => s.Status == SagaStatus.Failed).ToList();
        var compensatingSagas = allSagas.Where(s => s.Status == SagaStatus.Compensating).ToList();

        // Process failed sagas - initiate compensation if not already done
        foreach (var saga in failedSagas)
        {
            try
            {
                // Check if compensation has been initiated
                if (saga.Status != SagaStatus.Compensating)
                {
                    _logger.LogWarning("Initiating compensation for failed saga {SagaId}", saga.Id);

                    // Initiate compensation using the strategy configured on the saga definition
                    await _compensationService.BeginCompensationAsync(saga);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initiate compensation for saga {SagaId}", saga.Id);
            }
        }

        // Monitor compensating sagas
        foreach (var saga in compensatingSagas)
        {
            try
            {
                var compensationTxns = await _compensationService.GetCompensationsAsync(saga.Id);
                var allCompleted = compensationTxns.All(t =>
                    t.Status == CompensationStatus.Completed ||
                    t.Status == CompensationStatus.Failed);

                if (allCompleted)
                {
                    _logger.LogInformation("Compensation completed for saga {SagaId}", saga.Id);
                    saga.Status = SagaStatus.Compensated;
                    await _sagaRepository.UpdateAsync(saga);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring compensation for saga {SagaId}", saga.Id);
            }
        }
    }
}
