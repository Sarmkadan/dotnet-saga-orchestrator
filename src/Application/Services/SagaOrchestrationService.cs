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

    /// <summary>
/// Initializes a new instance of the <see cref="SagaOrchestrationService"/> class.
/// </summary>
/// <param name="sagaRepository">The saga repository.</param>
/// <param name="stepRepository">The saga step repository.</param>
/// <param name="compensationService">The compensation service.</param>
/// <param name="sagaLogger">The saga logger (optional).</param>
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
    /// Creates and initializes a new saga instance.
    /// </summary>
    /// <param name="definition">The saga definition.</param>
    /// <param name="maxRetries">The maximum number of retries for steps (optional).</param>
    /// <param name="timeoutSeconds">The timeout in seconds for the saga (optional).</param>
    /// <returns>The created saga instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    /// <exception cref="InvalidSagaDefinitionException">Thrown when the saga definition validation fails.</exception>
    /// <exception cref="DotnetSagaOrchestratorException">Thrown when an error occurs during saga creation.</exception>
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

        try
        {
            var created = await _sagaRepository.CreateAsync(saga);
            return created ?? throw new SagaException("Failed to create saga", saga.Id);
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error creating saga", ex);
        }
    }

    /// <summary>
    /// Starts saga execution by executing the first step
    /// </summary>
    /// <param name="sagaId">The saga ID.</param>
    /// <returns>The started saga instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> is null or empty.</exception>
    /// <exception cref="SagaNotFoundException">Thrown when the saga is not found.</exception>
    /// <exception cref="SagaException">Thrown when the saga cannot be started.</exception>
    /// <exception cref="DotnetSagaOrchestratorException">Thrown when an error occurs during saga start.</exception>
    public async Task<Saga> StartSagaAsync(string sagaId)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must be provided", nameof(sagaId));

        Saga saga;
        try
        {
            saga = await _sagaRepository.GetByIdAsync(sagaId)
                ?? throw new SagaNotFoundException(sagaId);
        }
        catch (Exception ex) when (!(ex is SagaNotFoundException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga for start", ex);
        }

        if (saga.Status != SagaStatus.Initialized)
            throw new SagaException($"Cannot start saga in {saga.Status} status", sagaId);

        using var activity = SagaActivitySource.StartSaga(saga.Id, saga.Definition.Id, saga.CorrelationId);

        saga.Start();

        // Create step instances from definition
        InitializeStepsFromDefinition(saga);

        try
        {
            var updated = await _sagaRepository.UpdateAsync(saga);
            return updated ?? throw new SagaException("Failed to update saga", sagaId);
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error updating saga after start", ex);
        }
    }

    /// <summary>
    /// Executes the next step in the saga.
    /// </summary>
    /// <param name="sagaId">The saga ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The executed saga step, or null if all steps are completed.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> is null or empty.</exception>
    /// <exception cref="SagaNotFoundException">Thrown when the saga is not found.</exception>
    /// <exception cref="SagaException">Thrown when the saga is not in running status.</exception>
    /// <exception cref="DotnetSagaOrchestratorException">Thrown when an error occurs during step execution.</exception>
    public async Task<SagaStep> ExecuteNextStepAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must be provided", nameof(sagaId));

        Saga saga;
        try
        {
            saga = await _sagaRepository.GetByIdAsync(sagaId)
                ?? throw new SagaNotFoundException(sagaId);
        }
        catch (Exception ex) when (!(ex is SagaNotFoundException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga for step execution", ex);
        }

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
    nextStep.AttemptCount++;
        nextStep.Start();
        await _stepRepository.UpdateAsync(nextStep);

        using var stepActivity = SagaActivitySource.StartStep(
            saga.Id, nextStep.Id, nextStep.Name, nextStep.Order, nextStep.RetryCount + 1);

        try
        {
            // Simulate step execution (would call actual service endpoint)
            var result = await SimulateStepExecutionAsync(nextStep, cancellationToken);

            // Persist checkpoint atomically: update step first, then saga.
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
                try
                {
                    await _compensationService.BeginCompensationAsync(saga);
                }
                catch (Exception compEx)
                {
                    throw new DotnetSagaOrchestratorException("Error during compensation start", compEx);
                }
            }
        }

        return nextStep;
    }

    /// <summary>
    /// Handles step timeout.
    /// </summary>
    /// <param name="sagaId">The saga ID.</param>
    /// <param name="stepId">The step ID.</param>
    /// <returns>True if the step timed out, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> or <paramref name="stepId"/> is null or empty.</exception>
    /// <exception cref="SagaNotFoundException">Thrown when the saga is not found.</exception>
    /// <exception cref="SagaException">Thrown when the step is not found in the saga.</exception>
    /// <exception cref="DotnetSagaOrchestratorException">Thrown when an error occurs during timeout handling.</exception>
    public async Task<bool> HandleTimeoutAsync(string sagaId, string stepId)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must be provided", nameof(sagaId));
        if (string.IsNullOrWhiteSpace(stepId))
            throw new ArgumentException("Step ID must be provided", nameof(stepId));

        Saga saga;
        try
        {
            saga = await _sagaRepository.GetByIdAsync(sagaId)
                ?? throw new SagaNotFoundException(sagaId);
        }
        catch (Exception ex) when (!(ex is SagaNotFoundException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga for timeout handling", ex);
        }

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
            try
            {
                await _compensationService.BeginCompensationAsync(saga);
            }
            catch (Exception compEx)
            {
                throw new DotnetSagaOrchestratorException("Error during compensation after timeout", compEx);
            }
        }

        return true;
    }

    /// <summary>
    /// Compensates a failed saga, running all pending compensation transactions
    /// to completion using the strategy configured on the saga's definition.
    /// </summary>
    /// <param name="sagaId">The saga ID.</param>
    /// <returns>The compensated saga instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> is null or empty.</exception>
    /// <exception cref="SagaNotFoundException">Thrown when the saga is not found.</exception>
    /// <exception cref="DotnetSagaOrchestratorException">Thrown when an error occurs during compensation.</exception>
    public async Task<Saga> CompensateSagaAsync(string sagaId)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must be provided", nameof(sagaId));

        Saga saga;
        try
        {
            saga = await _sagaRepository.GetByIdAsync(sagaId)
                ?? throw new SagaNotFoundException(sagaId);
        }
        catch (Exception ex) when (!(ex is SagaNotFoundException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga for compensation", ex);
        }

        if (saga.Status != SagaStatus.Compensating)
        {
            try
            {
                await _compensationService.BeginCompensationAsync(saga);
            }
            catch (Exception compEx)
            {
                throw new DotnetSagaOrchestratorException("Error starting compensation", compEx);
            }
        }

        while (await _compensationService.ExecuteNextCompensationAsync(sagaId) != null)
        {
            // Keep executing compensation transactions until none remain.
        }

        return await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new SagaNotFoundException(sagaId);
    }

    /// <summary>
    /// Compensates a failed saga using an explicit compensation strategy override.
    /// </summary>
    /// <param name="sagaId">The saga ID.</param>
    /// <param name="strategy">The compensation strategy.</param>
    /// <returns>The compensated saga instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> is null or empty.</exception>
    /// <exception cref="SagaNotFoundException">Thrown when the saga is not found.</exception>
    /// <exception cref="DotnetSagaOrchestratorException">Thrown when an error occurs during compensation.</exception>
    public async Task<Saga> CompensateSagaAsync(string sagaId, CompensationStrategy strategy)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must be provided", nameof(sagaId));

        Saga saga;
        try
        {
            saga = await _sagaRepository.GetByIdAsync(sagaId)
                ?? throw new SagaNotFoundException(sagaId);
        }
        catch (Exception ex) when (!(ex is SagaNotFoundException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga for strategy override", ex);
        }

        saga.Definition.CompensationStrategy = strategy;
        await _sagaRepository.UpdateAsync(saga);

        return await CompensateSagaAsync(sagaId);
    }

    /// <summary>
    /// Aborts a running saga.
    /// </summary>
    /// <param name="sagaId">The saga ID.</param>
    /// <param name="reason">The reason for aborting the saga (optional).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> is null or empty.</exception>
    /// <exception cref="SagaNotFoundException">Thrown when the saga is not found.</exception>
    /// <exception cref="SagaException">Thrown when the saga is not in running or initialized status.</exception>
    /// <exception cref="DotnetSagaOrchestratorException">Thrown when an error occurs during saga abort.</exception>
    public async Task AbortSagaAsync(string sagaId, string reason = "User abort")
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must be provided", nameof(sagaId));

        Saga saga;
        try
        {
            saga = await _sagaRepository.GetByIdAsync(sagaId)
                ?? throw new SagaNotFoundException(sagaId);
        }
        catch (Exception ex) when (!(ex is SagaNotFoundException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga for abort", ex);
        }

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
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must be provided", nameof(sagaId));

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
