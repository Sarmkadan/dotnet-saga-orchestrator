#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace SagaOrchestrator.Infrastructure.Telemetry;

/// <summary>
/// Central ActivitySource for saga orchestration telemetry.
///
// Register with OpenTelemetry by adding the source name to your tracer provider:
// <code>
// services.AddOpenTelemetry()
//     .WithTracing(b => b.AddSource(SagaActivitySource.Name));
// </code>
///
/// The source emits spans for:
//   - saga.start       – when a saga transitions to Running
//   - saga.step        – each individual step execution attempt
//   - saga.compensate  – each compensation transaction execution
//   - saga.complete    – when a saga reaches Completed or Compensated
/// </summary>
public static class SagaActivitySource
{
    /// <summary>
    /// The name used to identify this ActivitySource with OpenTelemetry.
    /// </summary>
    public const string Name = "SagaOrchestrator";

    internal static readonly ActivitySource Source = new(Name);

    // -------------------------------------------------------------------------
    // Saga-level spans
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts a span representing the start of saga execution.
    /// The returned Activity should be disposed when the saga completes or fails.
    /// </summary>
    public static Activity? StartSaga(string sagaId, string definitionId, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("SagaId is required", nameof(sagaId));
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("DefinitionId is required", nameof(definitionId));

        var activity = Source.StartActivity("saga.start", ActivityKind.Internal);
        if (activity == null) return null;

        activity.SetTag("saga.id", sagaId);
        activity.SetTag("saga.definition_id", definitionId);

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            activity.SetTag("saga.correlation_id", correlationId);
            // Propagate correlation as baggage so downstream spans can reference it
            activity.SetBaggage("saga.correlation_id", correlationId);
        }

        return activity;
    }

    /// <summary>
    /// Records a span for saga completion (success or compensated).
    /// </summary>
    public static Activity? RecordSagaComplete(string sagaId, string finalStatus, int totalSteps)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("SagaId is required", nameof(sagaId));
        if (string.IsNullOrWhiteSpace(finalStatus))
            throw new ArgumentException("FinalStatus is required", nameof(finalStatus));

        var activity = Source.StartActivity("saga.complete", ActivityKind.Internal);
        if (activity == null) return null;

        activity.SetTag("saga.id", sagaId);
        activity.SetTag("saga.status", finalStatus);
        activity.SetTag("saga.total_steps", totalSteps);

        return activity;
    }

    // -------------------------------------------------------------------------
    // Step-level spans
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts a span representing execution of a single saga step.
    /// </summary>
    public static Activity? StartStep(string sagaId, string stepId, string stepName, int order, int attempt = 1)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("SagaId is required", nameof(sagaId));
        if (string.IsNullOrWhiteSpace(stepId))
            throw new ArgumentException("StepId is required", nameof(stepId));
        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentException("StepName is required", nameof(stepName));

        var activity = Source.StartActivity("saga.step", ActivityKind.Client);
        if (activity == null) return null;

        activity.SetTag("saga.id", sagaId);
        activity.SetTag("saga.step.id", stepId);
        activity.SetTag("saga.step.name", stepName);
        activity.SetTag("saga.step.order", order);
        activity.SetTag("saga.step.attempt", attempt);

        return activity;
    }

    /// <summary>
    /// Records a step failure on the provided activity.
    /// </summary>
    public static void RecordStepFailure(Activity? activity, string errorMessage)
    {
        if (activity == null) return;
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("ErrorMessage is required", nameof(errorMessage));

        activity.SetStatus(ActivityStatusCode.Error, errorMessage);
        activity.SetTag("saga.step.error", errorMessage);
    }

    // -------------------------------------------------------------------------
    // Compensation spans
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts a span representing execution of a compensation transaction.
    /// </summary>
    public static Activity? StartCompensation(string sagaId, string compensationId, string stepName, int stepOrder)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("SagaId is required", nameof(sagaId));
        if (string.IsNullOrWhiteSpace(compensationId))
            throw new ArgumentException("CompensationId is required", nameof(compensationId));
        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentException("StepName is required", nameof(stepName));

        var activity = Source.StartActivity("saga.compensate", ActivityKind.Client);
        if (activity == null) return null;

        activity.SetTag("saga.id", sagaId);
        activity.SetTag("saga.compensation.id", compensationId);
        activity.SetTag("saga.compensation.step_name", stepName);
        activity.SetTag("saga.compensation.step_order", stepOrder);

        return activity;
    }

    /// <summary>
    /// Records a compensation failure on the provided activity.
    /// </summary>
    public static void RecordCompensationFailure(Activity? activity, string errorMessage)
    {
        if (activity == null) return;
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("ErrorMessage is required", nameof(errorMessage));

        activity.SetStatus(ActivityStatusCode.Error, errorMessage);
        activity.SetTag("saga.compensation.error", errorMessage);
    }
}