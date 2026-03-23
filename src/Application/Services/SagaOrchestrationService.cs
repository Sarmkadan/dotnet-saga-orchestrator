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
using SagaOrchestrator.Core.Constants;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;
using SagaOrchestrator.Core.Utilities;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Logging;
using SagaOrchestrator.Infrastructure.Telemetry;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Main service responsible for orchestrating saga execution.
/// Manages step execution, retries, and compensation workflows.
/// </summary>
public class SagaOrchestrationService
{
    private readonly ISagaRepository _sagaRepository;
    private readonly ISagaStepRepository _stepRepository;
    private readonly CompensationService _compensationService;
    private readonly ISagaLogger? _sagaLogger;

    public SagaOrchestrationService(
        ISagaRepository sagaRepository,
        ISagaStepRepository stepRepository,
        CompensationService compensationService,
        ISagaLogger? sagaLogger = null)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _compensationService = compensationService ?? throw new ArgumentNullException(nameof(compensationService));
        _sagaLogger = sagaLogger;
    }

    /// <summary>
    /// Creates and initializes a new saga instance
    /// </summary>
    public async Task<Saga> CreateSagaAsync(SagaDefinition definition, int? maxRetries = null, int? timeoutSeconds = null)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        if (!definition.Validate())
            throw new InvalidSagaDefinitionException(definition.Id, "Saga definition validation failed");

        var saga = new Saga();
        saga.Initialize(definition,
            maxRetries ?? SagaConstants.DefaultMaxRetries,
            timeoutSeconds ?? SagaConstants.DefaultSagaTimeoutSeconds);

        var created = await _sagaRepository.CreateAsync(saga);
        return created ?? throw new SagaException("Failed to create saga", saga.Id);
    }

    /// <summary>
    /// Starts saga execution by executing the first step
    /// </summary>
    public async Task<Saga> StartSagaAsync(string sagaId)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new SagaNotFoundException(sagaId);

        if (saga.Status != SagaStatus.Initialized)
            throw new SagaException($"Cannot start saga in {saga.Status} status", sagaId);

        using var activity = SagaActivitySource.StartSaga(saga.Id, saga.Definition.Id, saga.CorrelationId);

        saga.Start();

        // Create step instances from definition
        InitializeStepsFromDefinition(saga);

        var updated = await _sagaRepository.UpdateAsync(saga);
        return updated ?? throw new SagaException("Failed to update saga", sagaId);
    }

    /// <summary>
    /// Executes the next step in the saga
    /// </summary>
    public async Task<SagaStep> ExecuteNextStepAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new SagaNotFoundException(sagaId);

        if (saga.Status != SagaStatus.Running)
            throw new SagaException($"Cannot execute step for saga in {saga.Status} status", sagaId);

        // Find next pending step — skip already-completed steps for idempotency
        var nextStep = saga.Steps.FirstOrDefault(s =>
            s.Status == SagaStepStatus.Pending || s.Status == SagaStepStatus.WaitingForRetry);
        if (nextStep == null)
        {
            // All steps completed
            saga.Complete();
            await _sagaRepository.UpdateAsync(saga);
            return null!;
        }

        // Idempotency guard: if the step was already completed in a previous attempt,
        // treat it as completed without re-executing to prevent duplicate side-effects.
        if (nextStep.Status == SagaStepStatus.Completed)
            return nextStep;

        // Execute the step
        nextStep.Start();
        await _stepRepository.UpdateAsync(nextStep);

        using var stepActivity = SagaActivitySource.StartStep(
            saga.Id, nextStep.Id, nextStep.Name, nextStep.Order, nextStep.RetryCount + 1);

        try
        {
            // Simulate step execution (would call actual service endpoint)
            var result = await SimulateStepExecutionAsync(nextStep, cancellationToken);

            // Persist checkpoint atomically: update step first, then saga.
            // If the saga update (outbox/checkpoint publish) throws, the step record
            // already reflects completion so a restart will detect it via the
            // idempotency guard above and skip re-execution.
            nextStep.Complete(result);
            await _stepRepository.UpdateAsync(nextStep);

            // Check if saga is complete
            if (saga.Steps.All(s => s.Status == SagaStepStatus.Completed))
            {
                saga.Complete();
                using var completeActivity = SagaActivitySource.RecordSagaComplete(
                    saga.Id, saga.Status.ToString(), saga.Steps.Count);
                _sagaLogger?.LogExecutionTimeline(saga);
            }

            await _sagaRepository.UpdateAsync(saga);
        }
        catch (Exception ex)
        {
            SagaActivitySource.RecordStepFailure(stepActivity, ex.Message);
            nextStep.Fail(ex.Message);

            // Check if we can retry using per-step policy when available
            if (nextStep.CanRetry() && nextStep.RetryCount < nextStep.MaxRetries)
            {
                nextStep.PrepareForRetry();

                // Apply backoff delay from per-step RetryPolicy when present
                if (nextStep.RetryPolicy != null && nextStep.RetryPolicy.CanRetry(nextStep.RetryCount - 1))
                {
                    var delayMs = nextStep.RetryPolicy.CalculateDelay(nextStep.RetryCount);
                    if (delayMs > 0)
                        await Task.Delay(delayMs, cancellationToken);
                }
            }
            else
            {
                // Step failed permanently, begin compensation
                saga.Fail($"Step '{nextStep.Name}' failed after {nextStep.RetryCount} retries: {ex.Message}");
                _sagaLogger?.LogExecutionTimeline(saga);
            }

            await _stepRepository.UpdateAsync(nextStep);
            await _sagaRepository.UpdateAsync(saga);

            if (saga.Status == SagaStatus.Failed)
            {
                await _compensationService.BeginCompensationAsync(saga);
            }
        }

        return nextStep;
    }

    /// <summary>
    /// Handles step timeout
    /// </summary>
    public async Task<bool> HandleTimeoutAsync(string sagaId, string stepId)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new SagaNotFoundException(sagaId);

        var step = saga.Steps.FirstOrDefault(s => s.Id == stepId);
        if (step == null)
            throw new SagaException($"Step '{stepId}' not found in saga", sagaId);

        if (!step.IsTimedOut())
            return false;

        step.Fail($"Step execution timed out after {step.TimeoutSeconds} seconds");

        if (step.CanRetry())
        {
            step.PrepareForRetry();
            await _stepRepository.UpdateAsync(step);
        }
        else
        {
            saga.Fail($"Step '{step.Name}' timed out and cannot be retried");
            await _stepRepository.UpdateAsync(step);
            await _sagaRepository.UpdateAsync(saga);
            await _compensationService.BeginCompensationAsync(saga);
        }

        return true;
    }

    /// <summary>
    /// Aborts a running saga
    /// </summary>
    public async Task AbortSagaAsync(string sagaId, string reason = "User abort")
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new SagaNotFoundException(sagaId);

        if (saga.Status != SagaStatus.Running && saga.Status != SagaStatus.Initialized)
            throw new SagaException($"Cannot abort saga in {saga.Status} status", sagaId);

        saga.Status = SagaStatus.Aborted;
        saga.FailureReason = reason;
        saga.FailedAt = DateTime.UtcNow;

        await _sagaRepository.UpdateAsync(saga);
    }

    /// <summary>
    /// Retrieves saga by ID
    /// </summary>
    public async Task<Saga> GetSagaAsync(string sagaId)
    {
        return await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new SagaNotFoundException(sagaId);
    }

    /// <summary>
    /// Lists sagas with optional filtering
    /// </summary>
    public async Task<List<Saga>> ListSagasAsync(SagaStatus? status = null, int pageSize = 100, int pageNumber = 1)
    {
        var sagas = await _sagaRepository.GetAllAsync();

        if (status.HasValue)
            sagas = sagas.Where(s => s.Status == status.Value).ToList();

        return sagas
            .OrderByDescending(s => s.StartedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    // Private helper methods

    private void InitializeStepsFromDefinition(Saga saga)
    {
        saga.Steps.Clear();

        foreach (var stepDef in saga.Definition.Steps)
        {
            var step = new SagaStep();
            step.Initialize(stepDef.Name, stepDef.Order, stepDef.ServiceUrl, stepDef.CompensationUrl);
            step.SagaId = saga.Id;
            step.MaxRetries = stepDef.MaxRetries;
            step.TimeoutSeconds = stepDef.TimeoutSeconds;
            step.RetryPolicy = stepDef.RetryPolicy;

            saga.Steps.Add(step);
        }
    }

    private async Task<Dictionary<string, object>> SimulateStepExecutionAsync(SagaStep step, CancellationToken cancellationToken)
    {
        // Simulate service call delay
        await Task.Delay(100, cancellationToken);

        // In production, this would make an HTTP call to the service
        var response = new Dictionary<string, object>
        {
            { "status", "success" },
            { "timestamp", DateTime.UtcNow },
            { "step", step.Name }
        };

        return response;
    }
}
