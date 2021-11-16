#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Extensions;

namespace SagaOrchestrator.Infrastructure.Logging;

/// <summary>
/// Comprehensive logging pipeline for saga operations.
/// Logs saga creation, execution, completion, and compensation events with structured data.
/// </summary>
public interface ISagaLogger
{
    void LogSagaCreated(Saga saga);
    void LogStepStarted(Saga saga, SagaStep step);
    void LogStepCompleted(Saga saga, SagaStep step, TimeSpan duration);
    void LogStepFailed(Saga saga, SagaStep step, Exception ex);
    void LogCompensationStarted(Saga saga);
    void LogCompensationCompleted(Saga saga);
    void LogSagaCompleted(Saga saga, TimeSpan duration);
    void LogSagaFailed(Saga saga, Exception ex);

    /// <summary>Logs the full step-by-step execution timeline for a saga.</summary>
    void LogExecutionTimeline(Saga saga);
}

public class SagaLogger : ISagaLogger
{
    private readonly ILogger<SagaLogger> _logger;

    public SagaLogger(ILogger<SagaLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogSagaCreated(Saga saga)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"] = saga.Id,
            ["SagaName"] = saga.Name,
            ["DefinitionId"] = saga.DefinitionId
        });

        _logger.LogInformation(
            "Saga created | Id: {SagaId}, Name: {SagaName}, Steps: {StepCount}",
            saga.Id, saga.Name, saga.Steps.Count);
    }

    public void LogStepStarted(Saga saga, SagaStep step)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"] = saga.Id,
            ["StepId"] = step.Id,
            ["StepName"] = step.Name,
            ["StepNumber"] = step.Order
        });

        _logger.LogInformation(
            "Step execution started | Saga: {SagaId}, Step: {StepName}, Order: {Order}",
            saga.Id, step.Name, step.Order);
    }

    public void LogStepCompleted(Saga saga, SagaStep step, TimeSpan duration)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"] = saga.Id,
            ["StepId"] = step.Id,
            ["StepName"] = step.Name,
            ["Duration"] = duration.TotalMilliseconds
        });

        _logger.LogInformation(
            "Step completed successfully | Saga: {SagaId}, Step: {StepName}, Duration: {Duration}ms",
            saga.Id, step.Name, duration.TotalMilliseconds);
    }

    public void LogStepFailed(Saga saga, SagaStep step, Exception ex)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"] = saga.Id,
            ["StepId"] = step.Id,
            ["StepName"] = step.Name,
            ["Attempt"] = step.RetryCount
        });

        _logger.LogError(ex,
            "Step execution failed | Saga: {SagaId}, Step: {StepName}, Attempt: {Attempt}, Error: {Error}",
            saga.Id, step.Name, step.RetryCount, ex.Message);
    }

    public void LogCompensationStarted(Saga saga)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"] = saga.Id,
            ["SagaName"] = saga.Name,
            ["Strategy"] = saga.CompensationStrategy
        });

        _logger.LogWarning(
            "Compensation started | Saga: {SagaId}, Strategy: {Strategy}",
            saga.Id, saga.CompensationStrategy);
    }

    public void LogCompensationCompleted(Saga saga)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"] = saga.Id,
            ["SagaName"] = saga.Name
        });

        _logger.LogInformation(
            "Compensation completed | Saga: {SagaId}",
            saga.Id);
    }

    public void LogSagaCompleted(Saga saga, TimeSpan duration)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"] = saga.Id,
            ["SagaName"] = saga.Name,
            ["Status"] = saga.Status,
            ["Duration"] = duration.TotalSeconds,
            ["CompletedSteps"] = saga.Steps.Count(s => s.Status.ToString() == "Completed")
        });

        _logger.LogInformation(
            "Saga completed successfully | Id: {SagaId}, Name: {SagaName}, Duration: {Duration}s, Steps: {CompletedSteps}/{TotalSteps}",
            saga.Id, saga.Name, duration.TotalSeconds, saga.Steps.Count(s => s.Status.ToString() == "Completed"), saga.Steps.Count);
    }

    public void LogSagaFailed(Saga saga, Exception ex)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"] = saga.Id,
            ["SagaName"] = saga.Name,
            ["Status"] = saga.Status
        });

        _logger.LogError(ex,
            "Saga execution failed | Id: {SagaId}, Name: {SagaName}, Error: {Error}",
            saga.Id, saga.Name, ex.Message);
    }

    /// <inheritdoc />
    public void LogExecutionTimeline(Saga saga)
    {
        var steps = saga.Steps.OrderBy(s => s.Order).ToList();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["SagaId"]    = saga.Id,
            ["SagaName"]  = saga.Name,
            ["Status"]    = saga.Status,
            ["StepCount"] = steps.Count
        });

        _logger.LogInformation(
            "Saga execution timeline | Id: {SagaId}, Name: {SagaName}, Status: {Status}, Steps: {StepCount}",
            saga.Id, saga.Name, saga.Status, steps.Count);

        foreach (var step in steps)
        {
            var duration = step.StartedAt.HasValue && step.CompletedAt.HasValue
                ? (step.CompletedAt.Value - step.StartedAt.Value)
                : (TimeSpan?)null;

            var durationStr = duration.HasValue ? $"{duration.Value.TotalMilliseconds:F0}ms" : "N/A";

            _logger.LogInformation(
                "  [{Order}] {StepName}: {Status} | Duration: {Duration} | StartedAt: {StartedAt} | Retries: {RetryCount}",
                step.Order,
                step.Name,
                step.Status,
                durationStr,
                step.StartedAt?.ToString("O") ?? "N/A",
                step.RetryCount);
        }
    }
}
