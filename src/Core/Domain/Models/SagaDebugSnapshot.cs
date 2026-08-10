#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Identifies the event that triggered the creation of a debug snapshot.
/// </summary>
public enum SnapshotTrigger
{
    /// <summary>Snapshot created by an explicit API or user request.</summary>
    Manual = 0,

    /// <summary>Auto-captured the moment a step began executing.</summary>
    StepStarted = 1,

    /// <summary>Auto-captured when a step successfully completed.</summary>
    StepCompleted = 2,

    /// <summary>Auto-captured when a step entered a failed state.</summary>
    StepFailed = 3,

    /// <summary>Auto-captured when the saga initiated the compensation phase.</summary>
    CompensationStarted = 4,

    /// <summary>Auto-captured when the saga reached <see cref="SagaStatus.Completed"/>.</summary>
    SagaCompleted = 5,

    /// <summary>Auto-captured when the saga entered <see cref="SagaStatus.Failed"/>.</summary>
    SagaFailed = 6,

    /// <summary>Execution paused at a registered breakpoint; snapshot captures the paused state.</summary>
    Breakpoint = 7,

    /// <summary>Auto-captured when the saga exceeded its configured timeout.</summary>
    TimedOut = 8,

    /// <summary>Auto-captured when all compensation transactions finished.</summary>
    Compensated = 9,
}

/// <summary>
/// Immutable record of a single saga step's execution state at a specific instant.
/// Used by the distributed debugger to represent the step within a <see cref="SagaDebugSnapshot"/>.
/// </summary>
public sealed record SagaStepDebugState
{
    /// <summary>Unique identifier of the step instance.</summary>
    [JsonPropertyName("stepId")]
    public required string StepId { get; init; }

    /// <summary>Human-readable name of the step.</summary>
    [JsonPropertyName("stepName")]
    public required string StepName { get; init; }

    /// <summary>Execution order within the saga (one-based).</summary>
    [JsonPropertyName("stepOrder")]
    public required int StepOrder { get; init; }

    /// <summary>Step execution status at the time the snapshot was captured.</summary>
    [JsonPropertyName("status")]
    public required SagaStepStatus Status { get; init; }

    /// <summary>Number of retries attempted at snapshot time.</summary>
    [JsonPropertyName("retryCount")]
    public required int RetryCount { get; init; }

    /// <summary>Maximum retries allowed for this step.</summary>
    [JsonPropertyName("maxRetries")]
    public required int MaxRetries { get; init; }

    /// <summary>When this step began executing, or <c>null</c> if it had not started yet.</summary>
    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; init; }

    /// <summary>When this step finished successfully, or <c>null</c> if not yet completed.</summary>
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; init; }

    /// <summary>When this step was rolled back, or <c>null</c> if compensation has not run.</summary>
    [JsonPropertyName("compensatedAt")]
    public DateTime? CompensatedAt { get; init; }

    /// <summary>Error message if the step was in a failed or timed-out state at snapshot time.</summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }

    /// <summary>Service endpoint invoked by this step.</summary>
    [JsonPropertyName("serviceUrl")]
    public required string ServiceUrl { get; init; }

    /// <summary>Key/value output data produced by this step at snapshot time.</summary>
    [JsonPropertyName("outputData")]
    public required IReadOnlyDictionary<string, object> OutputData { get; init; }

    /// <summary>
    /// Captures the current live state of a <see cref="SagaStep"/> into an immutable record.
    /// </summary>
    /// <param name="step">The step to snapshot.</param>
    public static SagaStepDebugState FromStep(SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        return new()
        {
            StepId        = step.Id,
            StepName      = step.Name,
            StepOrder     = step.Order,
            Status        = step.Status,
            RetryCount    = step.RetryCount,
            MaxRetries    = step.MaxRetries,
            StartedAt     = step.StartedAt,
            CompletedAt   = step.CompletedAt,
            CompensatedAt = step.CompensatedAt,
            ErrorMessage  = step.ErrorMessage,
            ServiceUrl    = step.ServiceUrl ?? string.Empty,
            OutputData    = new Dictionary<string, object>(step.Response),
        };
    }
}

/// <summary>
/// Complete, immutable point-in-time snapshot of an entire saga's execution state.
/// Snapshots form the basis of the time-travel inspection feature, enabling a saga's
/// history to be traversed, replayed, or diffed at any recorded instant.
/// </summary>
public sealed record SagaDebugSnapshot
{
    /// <summary>Unique identifier for this snapshot instance.</summary>
    [JsonPropertyName("snapshotId")]
    public required string SnapshotId { get; init; }

    /// <summary>Identifier of the saga this snapshot was taken from.</summary>
    [JsonPropertyName("sagaId")]
    public required string SagaId { get; init; }

    /// <summary>Saga definition name at capture time.</summary>
    [JsonPropertyName("sagaName")]
    public required string SagaName { get; init; }

    /// <summary>Saga definition identifier at capture time.</summary>
    [JsonPropertyName("definitionId")]
    public required string DefinitionId { get; init; }

    /// <summary>Saga correlation identifier, linking it to an outer business transaction.</summary>
    [JsonPropertyName("correlationId")]
    public required string CorrelationId { get; init; }

    /// <summary>Saga execution status at the moment this snapshot was taken.</summary>
    [JsonPropertyName("sagaStatus")]
    public required SagaStatus SagaStatus { get; init; }

    /// <summary>What event triggered the creation of this snapshot.</summary>
    [JsonPropertyName("trigger")]
    public required SnapshotTrigger Trigger { get; init; }

    /// <summary>UTC timestamp when this snapshot was captured.</summary>
    [JsonPropertyName("capturedAt")]
    public required DateTime CapturedAt { get; init; }

    /// <summary>When the saga started executing.</summary>
    [JsonPropertyName("sagaStartedAt")]
    public required DateTime SagaStartedAt { get; init; }

    /// <summary>When the saga finished, if it has reached a terminal state.</summary>
    [JsonPropertyName("sagaCompletedAt")]
    public DateTime? SagaCompletedAt { get; init; }

    /// <summary>Failure description if the saga was in a failed state at capture time.</summary>
    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; init; }

    /// <summary>Number of top-level saga retries recorded at capture time.</summary>
    [JsonPropertyName("retryCount")]
    public required int RetryCount { get; init; }

    /// <summary>Maximum retries allowed for the saga.</summary>
    [JsonPropertyName("maxRetries")]
    public required int MaxRetries { get; init; }

    /// <summary>Ordered, immutable list of all step states included in this snapshot.</summary>
    [JsonPropertyName("steps")]
    public required IReadOnlyList<SagaStepDebugState> Steps { get; init; }

    /// <summary>Saga metadata key/value pairs at capture time.</summary>
    [JsonPropertyName("metadata")]
    public required IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <summary>Optional annotation attached to the snapshot (e.g. breakpoint name or replay label).</summary>
    [JsonPropertyName("label")]
    public string? Label { get; init; }

    /// <summary>
    /// Monotonically increasing sequence number within this saga's snapshot history (1-based).
    /// Enables ordered traversal without depending solely on wall-clock timestamps.
    /// </summary>
    [JsonPropertyName("sequenceNumber")]
    public required int SequenceNumber { get; init; }

    // -------------------------------------------------------------------------
    // Computed convenience properties
    // -------------------------------------------------------------------------

    /// <summary>Number of steps that had completed successfully at snapshot time.</summary>
    [JsonIgnore]
    public int CompletedStepCount => Steps.Count(s => s.Status == SagaStepStatus.Completed);

    /// <summary>Number of steps in a failed or timed-out state at snapshot time.</summary>
    [JsonIgnore]
    public int FailedStepCount => Steps.Count(s => s.Status is SagaStepStatus.Failed or SagaStepStatus.TimedOut);

    /// <summary>Saga execution progress as a 0–100 percentage based on completed steps.</summary>
    [JsonIgnore]
    public double ProgressPercent =>
        Steps.Count == 0 ? 0 : Math.Round((double)CompletedStepCount / Steps.Count * 100, 2);

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Captures the live state of a <see cref="Saga"/> and all its steps into a new immutable snapshot.
    /// </summary>
    /// <param name="saga">The saga whose state should be captured.</param>
    /// <param name="trigger">The event that triggered this snapshot.</param>
    /// <param name="sequenceNumber">Monotonic sequence number within this saga's history.</param>
    /// <param name="label">Optional annotation for the snapshot.</param>
    public static SagaDebugSnapshot Capture(
        Saga saga,
        SnapshotTrigger trigger,
        int sequenceNumber,
        string? label = null)
    {
        ArgumentNullException.ThrowIfNull(saga);
        return new()
        {
            SnapshotId      = Guid.NewGuid().ToString("N"),
            SagaId          = saga.Id,
            SagaName        = saga.Definition?.Name ?? string.Empty,
            DefinitionId    = saga.Definition?.Id   ?? string.Empty,
            CorrelationId   = saga.CorrelationId,
            SagaStatus      = saga.Status,
            Trigger         = trigger,
            CapturedAt      = DateTime.UtcNow,
            SagaStartedAt   = saga.StartedAt,
            SagaCompletedAt = saga.CompletedAt,
            FailureReason   = saga.FailureReason,
            RetryCount      = saga.RetryCount,
            MaxRetries      = saga.MaxRetries,
            Steps           = saga.Steps.Select(SagaStepDebugState.FromStep).ToList().AsReadOnly(),
            Metadata        = new Dictionary<string, object>(saga.Metadata),
            Label           = label,
            SequenceNumber  = sequenceNumber,
        };
    }
}
