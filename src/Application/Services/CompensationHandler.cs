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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Handles compensation (rollback) logic for failed sagas. Supports multiple
/// compensation strategies: reverse-order, forward-order, from-failure-point, and parallel.
/// </summary>
public class CompensationHandler
{
    private readonly ICompensationTransactionRepository _compensationRepository;
    private readonly ISagaStepRepository _stepRepository;
    private readonly ILogger<CompensationHandler> _logger;
    private readonly SagaOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompensationHandler"/> class.
    /// </summary>
    public CompensationHandler(
        ICompensationTransactionRepository compensationRepository,
        ISagaStepRepository stepRepository,
        ILogger<CompensationHandler> logger,
        IOptions<SagaOptions> options)
    {
        _compensationRepository = compensationRepository ?? throw new ArgumentNullException(nameof(compensationRepository));
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new SagaOptions();
    }

    /// <summary>
    /// Executes compensation for a failed saga using the strategy defined in its definition.
    /// </summary>
    /// <param name="saga">The saga that requires compensation.</param>
    /// <param name="steps">All steps of the saga.</param>
    /// <param name="cancellationToken">Token to cancel compensation.</param>
    public async Task CompensateAsync(Saga saga, List<SagaStep> steps, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(saga);
        ArgumentNullException.ThrowIfNull(steps);

        var strategy = saga.Definition?.CompensationStrategy ?? CompensationStrategy.ReverseOrder;
        var completedSteps = steps.Where(s => s.Status == SagaStepStatus.Completed).ToList();

        _logger.LogInformation(
            "Starting compensation for saga {SagaId} with strategy {Strategy}, {Count} steps to compensate",
            saga.Id, strategy, completedSteps.Count);

        var ordered = OrderStepsForCompensation(completedSteps, strategy);

        if (strategy == CompensationStrategy.Parallel)
        {
            await CompensateParallelAsync(saga.Id, ordered, cancellationToken);
        }
        else
        {
            await CompensateSequentialAsync(saga.Id, ordered, cancellationToken);
        }
    }

    /// <summary>
    /// Determines the order in which steps should be compensated based on the chosen strategy.
    /// </summary>
    /// <param name="completedSteps">Steps that were completed and need rollback.</param>
    /// <param name="strategy">The compensation ordering strategy.</param>
    /// <returns>Ordered list of steps for compensation.</returns>
    public List<SagaStep> OrderStepsForCompensation(List<SagaStep> completedSteps, CompensationStrategy strategy)
    {
        return strategy switch
        {
            CompensationStrategy.ReverseOrder => completedSteps.OrderByDescending(s => s.Order).ToList(),
            CompensationStrategy.ForwardOrder => completedSteps.OrderBy(s => s.Order).ToList(),
            CompensationStrategy.FromFailurePoint => completedSteps.OrderByDescending(s => s.Order).ToList(),
            CompensationStrategy.Parallel => completedSteps,
            CompensationStrategy.Manual => completedSteps,
            _ => completedSteps.OrderByDescending(s => s.Order).ToList()
        };
    }

    /// <summary>
    /// Records a compensation transaction in the repository for audit purposes.
    /// </summary>
    /// <param name="sagaId">The saga being compensated.</param>
    /// <param name="step">The step being rolled back.</param>
    /// <param name="success">Whether the compensation succeeded.</param>
    /// <param name="errorMessage">Error details if compensation failed.</param>
    public async Task RecordCompensationAsync(string sagaId, SagaStep step, bool success, string? errorMessage = null)
    {
        var transaction = new CompensationTransaction
        {
            Id = Guid.NewGuid().ToString("N"),
            SagaId = sagaId,
            StepId = step.Id,
            Status = success ? CompensationStatus.Completed : CompensationStatus.Failed,
            InitiatedAt = DateTime.UtcNow,
            ErrorMessage = errorMessage
        };

        await _compensationRepository.CreateAsync(transaction);
        _logger.LogDebug("Compensation recorded for step {StepId}: success={Success}", step.Id, success);
    }

    private async Task CompensateSequentialAsync(string sagaId, List<SagaStep> steps, CancellationToken ct)
    {
        foreach (var step in steps)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                step.Status = SagaStepStatus.Compensated;
                await _stepRepository.UpdateAsync(step);
                await RecordCompensationAsync(sagaId, step, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compensation failed for step {StepId} in saga {SagaId}", step.Id, sagaId);
                await RecordCompensationAsync(sagaId, step, false, ex.Message);
            }
        }
    }

    private async Task CompensateParallelAsync(string sagaId, List<SagaStep> steps, CancellationToken ct)
    {
        var tasks = steps.Select(async step =>
        {
            try
            {
                step.Status = SagaStepStatus.Compensated;
                await _stepRepository.UpdateAsync(step);
                await RecordCompensationAsync(sagaId, step, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parallel compensation failed for step {StepId}", step.Id);
                await RecordCompensationAsync(sagaId, step, false, ex.Message);
            }
        });

        await Task.WhenAll(tasks);
    }
}
