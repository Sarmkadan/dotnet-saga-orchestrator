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
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Telemetry;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Service responsible for executing compensating transactions.
/// Implements various compensation strategies for rollback of failed sagas.
/// </summary>
public class CompensationService
{
    private readonly ICompensationTransactionRepository _compensationRepository;
    private readonly ISagaRepository _sagaRepository;
    private readonly ISagaStepRepository _stepRepository;

    public CompensationService(
        ICompensationTransactionRepository compensationRepository,
        ISagaRepository sagaRepository,
        ISagaStepRepository stepRepository)
    {
        _compensationRepository = compensationRepository ?? throw new ArgumentNullException(nameof(compensationRepository));
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
    }

    /// <summary>
    /// Initiates compensation for a failed saga
    /// </summary>
    public async Task BeginCompensationAsync(Saga saga)
    {
        if (saga == null)
            throw new ArgumentNullException(nameof(saga));

        if (saga.Status != SagaStatus.Failed)
            throw new SagaException($"Cannot compensate saga in {saga.Status} status", saga.Id);

        saga.BeginCompensation();
        await _sagaRepository.UpdateAsync(saga);

        // Create compensation transactions for completed steps
        var completedSteps = saga.Steps
            .Where(s => s.Status == SagaStepStatus.Completed)
            .ToList();

        foreach (var step in completedSteps)
        {
            var compensation = new CompensationTransaction();
            compensation.Initialize(saga.Id, step.Id, step.Name, step.Order, step.CompensationUrl);
            compensation.SetRequestPayload(step.Response);

            await _compensationRepository.CreateAsync(compensation);
        }
    }

    /// <summary>
    /// Executes the next compensation transaction
    /// </summary>
    public async Task<CompensationTransaction?> ExecuteNextCompensationAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new SagaNotFoundException(sagaId);

        if (saga.Status != SagaStatus.Compensating)
            throw new SagaException($"Cannot execute compensation for saga in {saga.Status} status", sagaId);

        var compensations = await _compensationRepository.GetBySagaIdAsync(sagaId);

        // Get next pending compensation based on strategy
        var nextCompensation = GetNextCompensationByStrategy(saga.Definition.CompensationStrategy, compensations);

        if (nextCompensation == null)
        {
            // All compensations complete
            saga.CompleteCompensation();
            await _sagaRepository.UpdateAsync(saga);

            // Mark all steps as compensated
            var steps = saga.Steps.Where(s => s.Status == SagaStepStatus.Completed).ToList();
            foreach (var step in steps)
            {
                step.Compensate();
                await _stepRepository.UpdateAsync(step);
            }

            return null;
        }

        // Execute compensation
        nextCompensation.Start();
        await _compensationRepository.UpdateAsync(nextCompensation);

        using var compActivity = SagaActivitySource.StartCompensation(
            sagaId, nextCompensation.Id, nextCompensation.StepName, nextCompensation.Order);

        try
        {
            var result = await SimulateCompensationCallAsync(nextCompensation, cancellationToken);
            nextCompensation.Complete(result);

            await _compensationRepository.UpdateAsync(nextCompensation);
        }
        catch (Exception ex)
        {
            SagaActivitySource.RecordCompensationFailure(compActivity, ex.Message);
            nextCompensation.Fail(ex.Message);

            if (nextCompensation.CanRetry())
            {
                nextCompensation.PrepareForRetry();
            }

            await _compensationRepository.UpdateAsync(nextCompensation);
        }

        return nextCompensation;
    }

    /// <summary>
    /// Retries a failed compensation transaction
    /// </summary>
    public async Task<bool> RetryCompensationAsync(string compensationId)
    {
        var compensation = await _compensationRepository.GetByIdAsync(compensationId);
        if (compensation == null)
            throw new SagaException($"Compensation transaction '{compensationId}' not found");

        if (compensation.Status != CompensationStatus.Failed)
            return false;

        if (!compensation.CanRetry())
            throw new SagaException($"Compensation cannot be retried: exceeded max retries", compensation.SagaId);

        compensation.PrepareForRetry();
        await _compensationRepository.UpdateAsync(compensation);

        return true;
    }

    /// <summary>
    /// Gets compensation transactions for a saga
    /// </summary>
    public async Task<List<CompensationTransaction>> GetCompensationsAsync(string sagaId)
    {
        return await _compensationRepository.GetBySagaIdAsync(sagaId);
    }

    /// <summary>
    /// Checks for compensation timeouts
    /// </summary>
    public async Task<List<CompensationTransaction>> CheckTimeoutsAsync(string sagaId)
    {
        var compensations = await _compensationRepository.GetBySagaIdAsync(sagaId);
        var timedOut = new List<CompensationTransaction>();

        foreach (var compensation in compensations)
        {
            if (compensation.IsTimedOut() && compensation.Status == CompensationStatus.InProgress)
            {
                compensation.Fail($"Compensation timed out after {compensation.TimeoutSeconds} seconds");

                if (compensation.CanRetry())
                {
                    compensation.PrepareForRetry();
                }

                await _compensationRepository.UpdateAsync(compensation);
                timedOut.Add(compensation);
            }
        }

        return timedOut;
    }

    // Private helper methods

    private CompensationTransaction? GetNextCompensationByStrategy(
        CompensationStrategy strategy,
        List<CompensationTransaction> compensations)
    {
        var pending = compensations.Where(c => c.Status == CompensationStatus.Pending).ToList();

        if (pending.Count == 0)
            return null;

        return strategy switch
        {
            CompensationStrategy.ReverseOrder => pending.OrderByDescending(c => c.Order).FirstOrDefault(),
            CompensationStrategy.ForwardOrder => pending.OrderBy(c => c.Order).FirstOrDefault(),
            CompensationStrategy.Parallel => pending.FirstOrDefault(),
            _ => pending.FirstOrDefault()
        };
    }

    private async Task<Dictionary<string, object>> SimulateCompensationCallAsync(
        CompensationTransaction compensation,
        CancellationToken cancellationToken)
    {
        // Simulate HTTP call delay
        await Task.Delay(100, cancellationToken);

        // In production, this would make an HTTP call to the compensation endpoint
        var response = new Dictionary<string, object>
        {
            { "compensated", true },
            { "timestamp", DateTime.UtcNow },
            { "step", compensation.StepName }
        };

        return response;
    }
}
