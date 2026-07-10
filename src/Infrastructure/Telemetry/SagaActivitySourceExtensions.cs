#nullable enable

using System.Diagnostics;

namespace SagaOrchestrator.Infrastructure.Telemetry;

/// <summary>
/// Extension methods for <see cref="SagaActivitySource"/> to provide fluent APIs and additional convenience methods
/// for saga telemetry operations.
/// </summary>
public static class SagaActivitySourceExtensions
{
    /// <summary>
    /// Starts a saga execution span with additional context tags.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="definitionId">The saga definition identifier.</param>
    /// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
    /// <param name="sagaType">Optional saga type/category for filtering and grouping.</param>
    /// <param name="tenantId">Optional tenant identifier for multi-tenancy scenarios.</param>
    /// <returns>The started activity or null if activity creation failed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sagaId"/> or <paramref name="definitionId"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="sagaId"/> or <paramref name="definitionId"/> is empty or whitespace.</exception>
    public static Activity? StartSaga(
        string sagaId,
        string definitionId,
        string? correlationId = null,
        string? sagaType = null,
        string? tenantId = null)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        ArgumentNullException.ThrowIfNull(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

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
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="finalStatus">The final saga status (e.g., "Completed", "Compensated", "Failed").</param>
    /// <param name="totalSteps">Total number of steps in the saga.</param>
    /// <param name="duration">Total execution duration for performance tracking.</param>
    /// <param name="completedSteps">Number of successfully completed steps.</param>
    /// <param name="failedSteps">Number of failed steps.</param>
    /// <returns>The completed activity or null if activity creation failed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sagaId"/> or <paramref name="finalStatus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="sagaId"/> or <paramref name="finalStatus"/> is empty or whitespace.</exception>
    public static Activity? RecordSagaComplete(
        string sagaId,
        string finalStatus,
        int totalSteps,
        TimeSpan duration,
        int completedSteps = 0,
        int failedSteps = 0)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        ArgumentNullException.ThrowIfNull(finalStatus);
        ArgumentException.ThrowIfNullOrWhiteSpace(finalStatus);

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
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="stepName">Human-readable step name.</param>
    /// <param name="order">Execution order position in the saga.</param>
    /// <param name="attempt">Retry attempt number (default: 1).</param>
    /// <param name="stepType">Optional step type/category for filtering.</param>
    /// <param name="serviceName">Optional service name where the step executes.</param>
    /// <returns>The started step activity or null if activity creation failed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sagaId"/>, <paramref name="stepId"/>, <paramref name="stepName"/>, or <paramref name="order"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="sagaId"/>, <paramref name="stepId"/>, or <paramref name="stepName"/> is empty or whitespace.</exception>
    public static Activity? StartStep(
        string sagaId,
        string stepId,
        string stepName,
        int order,
        int attempt = 1,
        string? stepType = null,
        string? serviceName = null)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        ArgumentNullException.ThrowIfNull(stepId);
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        ArgumentNullException.ThrowIfNull(stepName);
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);

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
    /// <param name="activity">The activity to record the failure on.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="exception">Optional exception containing detailed error information.</param>
    /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is empty or whitespace.</exception>
    public static void RecordStepFailure(
        Activity? activity,
        string errorMessage,
        Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(errorMessage);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

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
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="compensationId">The compensation transaction identifier.</param>
    /// <param name="stepName">Name of the step being compensated.</param>
    /// <param name="stepOrder">Execution order of the step being compensated.</param>
    /// <param name="compensationType">Optional compensation type/category.</param>
    /// <param name="compensatingService">Optional service performing the compensation.</param>
    /// <returns>The started compensation activity or null if activity creation failed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sagaId"/>, <paramref name="compensationId"/>, or <paramref name="stepName"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="sagaId"/>, <paramref name="compensationId"/>, or <paramref name="stepName"/> is empty or whitespace.</exception>
    public static Activity? StartCompensation(
        string sagaId,
        string compensationId,
        string stepName,
        int stepOrder,
        string? compensationType = null,
        string? compensatingService = null)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        ArgumentNullException.ThrowIfNull(compensationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(compensationId);
        ArgumentNullException.ThrowIfNull(stepName);
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);

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
    /// <param name="activity">The activity to record the failure on.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="exception">Optional exception containing detailed error information.</param>
    /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is empty or whitespace.</exception>
    public static void RecordCompensationFailure(
        Activity? activity,
        string errorMessage,
        Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(errorMessage);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        SagaActivitySource.RecordCompensationFailure(activity, errorMessage);

        if (exception != null)
        {
            activity?.SetTag("saga.compensation.exception_type", exception.GetType().FullName);
            activity?.SetTag("saga.compensation.exception_message", exception.Message);
        }
    }
}