#nullable enable

using System.Diagnostics;

namespace SagaOrchestrator.Infrastructure.Telemetry;

/// <summary>
/// Extension methods for SagaActivitySource to provide fluent APIs and additional convenience methods
/// for saga telemetry operations.
/// </summary>
public static class SagaActivitySourceExtensions
{
    /// <summary>
    /// Starts a saga execution span with additional context tags.
    /// </summary>
    public static Activity? StartSaga(
        string sagaId,
        string definitionId,
        string? correlationId = null,
        string? sagaType = null,
        string? tenantId = null)
    {
        var activity = SagaActivitySource.StartSaga(sagaId, definitionId, correlationId);
        if (activity == null) return null;

        if (!string.IsNullOrWhiteSpace(sagaType))
        {
            activity.SetTag("saga.type", sagaType);
        }

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            activity.SetTag("saga.tenant_id", tenantId);
        }

        return activity;
    }

    /// <summary>
    /// Records saga completion with additional performance metrics.
    /// </summary>
    public static Activity? RecordSagaComplete(
        string sagaId,
        string finalStatus,
        int totalSteps,
        TimeSpan duration,
        int completedSteps = 0,
        int failedSteps = 0)
    {
        var activity = SagaActivitySource.RecordSagaComplete(sagaId, finalStatus, totalSteps);
        if (activity == null) return null;

        activity.SetTag("saga.duration_ms", (long)duration.TotalMilliseconds);
        activity.SetTag("saga.completed_steps", completedSteps);
        activity.SetTag("saga.failed_steps", failedSteps);

        return activity;
    }

    /// <summary>
    /// Starts a saga step execution span with retry context.
    /// </summary>
    public static Activity? StartStep(
        string sagaId,
        string stepId,
        string stepName,
        int order,
        int attempt = 1,
        string? stepType = null,
        string? serviceName = null)
    {
        var activity = SagaActivitySource.StartStep(sagaId, stepId, stepName, order, attempt);
        if (activity == null) return null;

        if (!string.IsNullOrWhiteSpace(stepType))
        {
            activity.SetTag("saga.step.type", stepType);
        }

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            activity.SetTag("saga.step.service", serviceName);
        }

        return activity;
    }

    /// <summary>
    /// Records a step failure with exception details.
    /// </summary>
    public static void RecordStepFailure(
        Activity? activity,
        string errorMessage,
        Exception? exception = null)
    {
        SagaActivitySource.RecordStepFailure(activity, errorMessage);

        if (exception != null)
        {
            activity?.SetTag("saga.step.exception_type", exception.GetType().FullName);
            activity?.SetTag("saga.step.exception_message", exception.Message);
        }
    }

    /// <summary>
    /// Starts a compensation transaction span with additional context.
    /// </summary>
    public static Activity? StartCompensation(
        string sagaId,
        string compensationId,
        string stepName,
        int stepOrder,
        string? compensationType = null,
        string? compensatingService = null)
    {
        var activity = SagaActivitySource.StartCompensation(sagaId, compensationId, stepName, stepOrder);
        if (activity == null) return null;

        if (!string.IsNullOrWhiteSpace(compensationType))
        {
            activity.SetTag("saga.compensation.type", compensationType);
        }

        if (!string.IsNullOrWhiteSpace(compensatingService))
        {
            activity.SetTag("saga.compensation.service", compensatingService);
        }

        return activity;
    }

    /// <summary>
    /// Records a compensation failure with exception details.
    /// </summary>
    public static void RecordCompensationFailure(
        Activity? activity,
        string errorMessage,
        Exception? exception = null)
    {
        SagaActivitySource.RecordCompensationFailure(activity, errorMessage);

        if (exception != null)
        {
            activity?.SetTag("saga.compensation.exception_type", exception.GetType().FullName);
            activity?.SetTag("saga.compensation.exception_message", exception.Message);
        }
    }
}