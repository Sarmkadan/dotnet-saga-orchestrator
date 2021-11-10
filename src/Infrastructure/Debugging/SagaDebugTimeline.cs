#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Infrastructure.Debugging;

/// <summary>
/// Classifies entries that appear on a saga's debug timeline.
/// </summary>
public enum TimelineEntryKind
{
    /// <summary>A point-in-time debug snapshot was captured.</summary>
    Snapshot = 0,

    /// <summary>A domain event was published to the event bus.</summary>
    EventPublished = 1,

    /// <summary>A saga-level or step-level state transition occurred.</summary>
    StateTransition = 2,

    /// <summary>Execution paused because a registered breakpoint was hit.</summary>
    BreakpointHit = 3,

    /// <summary>A previously captured snapshot was restored for time-travel inspection.</summary>
    SnapshotRestored = 4,
}

/// <summary>
/// A single entry on a saga's debug timeline, representing one observable event
/// in chronological order.
/// </summary>
public sealed record TimelineEntry
{
    /// <summary>Unique identifier for this timeline entry.</summary>
    [JsonPropertyName("entryId")]
    public required string EntryId { get; init; }

    /// <summary>Category of event this entry represents.</summary>
    [JsonPropertyName("kind")]
    public required TimelineEntryKind Kind { get; init; }

    /// <summary>UTC timestamp of the event.</summary>
    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }

    /// <summary>Short human-readable title for this entry.</summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>Detailed description of the event.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Identifier of the associated snapshot, if this entry is linked to one.
    /// </summary>
    [JsonPropertyName("snapshotId")]
    public string? SnapshotId { get; init; }

    /// <summary>
    /// Name of the step involved in this entry, if applicable.
    /// </summary>
    [JsonPropertyName("stepName")]
    public string? StepName { get; init; }

    /// <summary>
    /// Additional key/value metadata attached to this timeline entry.
    /// </summary>
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Constructs a timeline entry from a <see cref="SagaDebugSnapshot"/>.
    /// </summary>
    public static TimelineEntry FromSnapshot(SagaDebugSnapshot snapshot) => new()
    {
        EntryId     = Guid.NewGuid().ToString("N"),
        Kind        = snapshot.Trigger == SnapshotTrigger.Breakpoint
                          ? TimelineEntryKind.BreakpointHit
                          : TimelineEntryKind.Snapshot,
        Timestamp   = snapshot.CapturedAt,
        Title       = $"Snapshot #{snapshot.SequenceNumber} — {snapshot.Trigger}",
        Description = snapshot.Label
                      ?? $"Saga status: {snapshot.SagaStatus} | "
                       + $"{snapshot.CompletedStepCount}/{snapshot.Steps.Count} steps completed",
        SnapshotId  = snapshot.SnapshotId,
    };

    /// <summary>
    /// Constructs a timeline entry from a published <see cref="SagaEvent"/>.
    /// </summary>
    public static TimelineEntry FromSagaEvent(SagaEvent sagaEvent) => new()
    {
        EntryId     = Guid.NewGuid().ToString("N"),
        Kind        = TimelineEntryKind.EventPublished,
        Timestamp   = sagaEvent.Timestamp,
        Title       = sagaEvent.EventName,
        Description = sagaEvent.Description,
        StepName    = sagaEvent.StepName,
        Metadata    = new Dictionary<string, object>(sagaEvent.Data),
    };
}

/// <summary>
/// A registered execution breakpoint for a specific saga step.
/// When the step with <see cref="StepName"/> is about to execute within
/// <see cref="SagaId"/>, the debugger pauses execution and captures a snapshot.
/// </summary>
public sealed record SagaDebugBreakpoint
{
    /// <summary>Unique identifier for this breakpoint.</summary>
    [JsonPropertyName("breakpointId")]
    public required string BreakpointId { get; init; }

    /// <summary>The saga to which this breakpoint applies.</summary>
    [JsonPropertyName("sagaId")]
    public required string SagaId { get; init; }

    /// <summary>Step name that triggers this breakpoint when execution reaches it.</summary>
    [JsonPropertyName("stepName")]
    public required string StepName { get; init; }

    /// <summary>Whether the breakpoint is currently active.</summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; init; } = true;

    /// <summary>UTC timestamp when this breakpoint was registered.</summary>
    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }

    /// <summary>Number of times this breakpoint has been triggered.</summary>
    [JsonPropertyName("hitCount")]
    public required int HitCount { get; init; }

    /// <summary>Optional description or purpose note for this breakpoint.</summary>
    [JsonPropertyName("note")]
    public string? Note { get; init; }

    /// <summary>Returns a copy of this breakpoint with <see cref="HitCount"/> incremented by one.</summary>
    public SagaDebugBreakpoint WithIncrementedHitCount() => this with { HitCount = HitCount + 1 };

    /// <summary>Returns a copy of this breakpoint with <see cref="IsEnabled"/> set to <paramref name="enabled"/>.</summary>
    public SagaDebugBreakpoint WithEnabled(bool enabled) => this with { IsEnabled = enabled };
}

/// <summary>
/// Full chronological debug timeline for a single saga, combining domain events and
/// debug snapshots into one unified, ordered view. Used by the distributed debugger
/// for post-mortem analysis and live step-through inspection.
/// </summary>
public sealed record SagaDebugTimeline
{
    /// <summary>Saga this timeline belongs to.</summary>
    [JsonPropertyName("sagaId")]
    public required string SagaId { get; init; }

    /// <summary>Saga definition name for display purposes.</summary>
    [JsonPropertyName("sagaName")]
    public required string SagaName { get; init; }

    /// <summary>UTC timestamp of the earliest recorded entry.</summary>
    [JsonPropertyName("startedAt")]
    public required DateTime StartedAt { get; init; }

    /// <summary>UTC timestamp of the most recent entry, or <c>null</c> when the saga is still active.</summary>
    [JsonPropertyName("lastActivityAt")]
    public DateTime? LastActivityAt { get; init; }

    /// <summary>
    /// All timeline entries in ascending chronological order.
    /// Interleaves snapshots and domain events for a unified history.
    /// </summary>
    [JsonPropertyName("entries")]
    public required IReadOnlyList<TimelineEntry> Entries { get; init; }

    /// <summary>All breakpoints currently registered for this saga.</summary>
    [JsonPropertyName("breakpoints")]
    public required IReadOnlyList<SagaDebugBreakpoint> Breakpoints { get; init; }

    /// <summary>Total number of debug snapshots captured for this saga.</summary>
    [JsonPropertyName("totalSnapshots")]
    public int TotalSnapshots => Entries.Count(e =>
        e.Kind is TimelineEntryKind.Snapshot or TimelineEntryKind.BreakpointHit or TimelineEntryKind.SnapshotRestored);

    /// <summary>Total number of domain events recorded for this saga.</summary>
    [JsonPropertyName("totalEvents")]
    public int TotalEvents => Entries.Count(e => e.Kind == TimelineEntryKind.EventPublished);

    /// <summary>
    /// Assembles a <see cref="SagaDebugTimeline"/> from a set of snapshots, domain events, and breakpoints.
    /// Entries are sorted by ascending timestamp.
    /// </summary>
    public static SagaDebugTimeline Build(
        string sagaId,
        string sagaName,
        DateTime sagaStartedAt,
        IEnumerable<SagaDebugSnapshot> snapshots,
        IEnumerable<SagaEvent> sagaEvents,
        IEnumerable<SagaDebugBreakpoint> breakpoints)
    {
        var snapshotEntries = snapshots.Select(TimelineEntry.FromSnapshot);
        var eventEntries    = sagaEvents.Select(TimelineEntry.FromSagaEvent);

        var allEntries = snapshotEntries
            .Concat(eventEntries)
            .OrderBy(e => e.Timestamp)
            .ToList()
            .AsReadOnly();

        return new SagaDebugTimeline
        {
            SagaId         = sagaId,
            SagaName       = sagaName,
            StartedAt      = sagaStartedAt,
            LastActivityAt = allEntries.Count > 0 ? allEntries[^1].Timestamp : null,
            Entries        = allEntries,
            Breakpoints    = breakpoints.ToList().AsReadOnly(),
        };
    }
}
