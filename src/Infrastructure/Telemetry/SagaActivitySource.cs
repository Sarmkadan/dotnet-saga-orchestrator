#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace SagaOrchestrator.Infrastructure.Telemetry;

/// <summary>
/// Central ActivitySource for saga orchestration telemetry.
/// </summary>
/// <remarks>
/// Register with OpenTelemetry by adding the source name to your tracer provider:
/// <code>
/// services.AddOpenTelemetry()
///     .WithTracing(b => b.AddSource(SagaActivitySource.Name));
/// </code>
/// The source emits spans for:
/// <list type="bullet">
/// <item><description>saga.start – when a saga transitions to Running</description></item>
/// <item><description>saga.step – each individual step execution attempt</description></item>
/// <item><description>saga.compensate – each compensation transaction execution</description></item>
/// <item><description>saga.complete – when a saga reaches Completed or Compensated</description></item>
/// </list>
/// </remarks>
public static class SagaActivitySource
{
    /// <summary>
    /// The name used to identify this ActivitySource with OpenTelemetry.
    /// </summary>
    public const string Name = "SagaOrchestrator";

    // Activity (span) names
    private const string StartActivityName = "saga.start";
    private const string CompleteActivityName = "saga.complete";
    private const string StepActivityName = "saga.step";
    private const string CompensateActivityName = "saga.compensate";

    // Tag names
    private const string SagaIdTag = "saga.id";
    private const string SagaDefinitionIdTag = "saga.definition_id";
    private const string SagaCorrelationIdTag = "saga.correlation_id";
    private const string SagaStatusTag = "saga.status";
    private const string SagaTotalStepsTag = "saga.total_steps";
    private const string SagaStepIdTag = "saga.step.id";
    private const string SagaStepNameTag = "saga.step.name";
    private const string SagaStepOrderTag = "saga.step.order";
    private const string SagaStepAttemptTag = "saga.step.attempt";
    private const string SagaStepErrorTag = "saga.step.error";
    private const string SagaCompensationIdTag = "saga.compensation.id";
    private const string SagaCompensationStepNameTag = "saga.compensation.step_name";
    private const string SagaCompensationStepOrderTag = "saga.compensation.step_order";
    private const string SagaCompensationErrorTag = "saga.compensation.error";

    // Baggage keys
    private const string CorrelationIdBaggageKey = "saga.correlation_id";

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

        var activity = Source.StartActivity(StartActivityName, ActivityKind.Internal);
        if (activity == null) return null;

        activity.SetTag(SagaIdTag, sagaId);
        activity.SetTag(SagaDefinitionIdTag, definitionId);

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            activity.SetTag(SagaCorrelationIdTag, correlationId);
            // Propagate correlation as baggage so downstream spans can reference it
            activity.SetBaggage(CorrelationIdBaggageKey, correlationId);
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

        var activity = Source.StartActivity(CompleteActivityName, ActivityKind.Internal);
        if (activity == null) return null;

        activity.SetTag(SagaIdTag, sagaId);
        activity.SetTag(SagaStatusTag, finalStatus);
        activity.SetTag(SagaTotalStepsTag, totalSteps);

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

        var activity = Source.StartActivity(StepActivityName, ActivityKind.Client);
        if (activity == null) return null;

        activity.SetTag(SagaIdTag, sagaId);
        activity.SetTag(SagaStepIdTag, stepId);
        activity.SetTag(SagaStepNameTag, stepName);
        activity.SetTag(SagaStepOrderTag, order);
        activity.SetTag(SagaStepAttemptTag, attempt);

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
        activity.SetTag(SagaStepErrorTag, errorMessage);
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

        var activity = Source.StartActivity(CompensateActivityName, ActivityKind.Client);
        if (activity == null) return null;

        activity.SetTag(SagaIdTag, sagaId);
        activity.SetTag(SagaCompensationIdTag, compensationId);
        activity.SetTag(SagaCompensationStepNameTag, stepName);
        activity.SetTag(SagaCompensationStepOrderTag, stepOrder);

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
        activity.SetTag(SagaCompensationErrorTag, errorMessage);
    }
}
