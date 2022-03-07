#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Infrastructure.Debugging;

namespace SagaOrchestrator.Infrastructure.Debugging;

/// <summary>
/// Extension methods for <see cref="SagaDebuggerService"/> that provide additional debugging utilities
/// for saga inspection, analysis, and time-travel operations.
/// </summary>
public static class SagaDebuggerServiceExtensions
{
    /// <summary>
    /// Captures a snapshot at the current state of the saga and returns it along with
    /// a human-readable summary of the current execution state.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga to capture.</param>
    /// <param name="label">Optional label for the snapshot.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the captured snapshot and a formatted state summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the debugger is disabled.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the saga is not found.</exception>
    public static async Task<(SagaDebugSnapshot Snapshot, string StateSummary)> CaptureSnapshotWithSummaryAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        string? label = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var snapshot = await debugger.CaptureSnapshotAsync(sagaId, SnapshotTrigger.Manual, label, cancellationToken);

        var summary = FormatSnapshotSummary(snapshot);
        return (snapshot, summary);
    }

    /// <summary>
    /// Gets the most recent snapshot for the specified saga, or null if no snapshots exist.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The most recent snapshot, or null if none exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId is null or whitespace.</exception>
    public static async Task<SagaDebugSnapshot?> GetLatestSnapshotAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var snapshots = await debugger.GetSnapshotsAsync(sagaId, cancellationToken);
        return snapshots.Count > 0 ? snapshots[^1] : null;
    }

    /// <summary>
    /// Gets the execution progress percentage for the specified saga based on its latest snapshot.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A value between 0 and 100 representing completion percentage, or null if no data is available.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId is null or whitespace.</exception>
    public static async Task<double?> GetProgressPercentageAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var latest = await debugger.GetLatestSnapshotAsync(sagaId, cancellationToken);
        return latest?.ProgressPercent;
    }

    /// <summary>
    /// Checks if the specified saga has any breakpoints registered.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if breakpoints exist, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId is null or whitespace.</exception>
    public static async Task<bool> HasBreakpointsAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var breakpoints = await debugger.GetBreakpointsAsync(sagaId, cancellationToken);
        return breakpoints.Count > 0;
    }

    /// <summary>
    /// Gets a formatted timeline summary for the specified saga, including snapshot count,
    /// event count, and progress information.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A formatted timeline summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId is null or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the saga is not found.</exception>
    public static async Task<string> GetTimelineSummaryAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var timeline = await debugger.GetTimelineAsync(sagaId, cancellationToken);
        var latest = await debugger.GetLatestSnapshotAsync(sagaId, cancellationToken);

        var parts = new List<string>();
        parts.Add($"Timeline for saga '{timeline.SagaName}' (ID: {timeline.SagaId})");
        parts.Add($"Started: {timeline.StartedAt:u}");
        parts.Add($"Total snapshots: {timeline.TotalSnapshots}");
        parts.Add($"Total events: {timeline.TotalEvents}");
        parts.Add($"Breakpoints: {timeline.Breakpoints.Count}");

        if (latest is not null)
        {
            parts.Add(string.Empty);
            parts.Add("Latest snapshot:");
            parts.Add($"  Status: {latest.SagaStatus}");
            parts.Add($"  Progress: {latest.ProgressPercent:F2}% ({latest.CompletedStepCount}/{latest.Steps.Count} steps)");
            parts.Add($"  Trigger: {latest.Trigger}");
            parts.Add($"  Captured: {latest.CapturedAt:u}");

            if (latest.FailureReason is not null)
            {
                parts.Add($"  Failure: {latest.FailureReason}");
            }
        }

        return string.Join(Environment.NewLine, parts);
    }

    /// <summary>
    /// Finds the first snapshot in the saga's history that matches the specified predicate.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="predicate">Predicate to match against snapshots.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching snapshot, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId or predicate is null.</exception>
    public static async Task<SagaDebugSnapshot?> FindSnapshotAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        Func<SagaDebugSnapshot, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        ArgumentNullException.ThrowIfNull(predicate);

        var snapshots = await debugger.GetSnapshotsAsync(sagaId, cancellationToken);
        return snapshots.FirstOrDefault(predicate);
    }

    /// <summary>
    /// Gets all snapshots filtered by a specific trigger type.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="trigger">The trigger type to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered list of snapshots.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId is null.</exception>
    public static async Task<IReadOnlyList<SagaDebugSnapshot>> GetSnapshotsByTriggerAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        SnapshotTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var snapshots = await debugger.GetSnapshotsAsync(sagaId, cancellationToken);
        return snapshots.Where(s => s.Trigger == trigger).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all breakpoints that have been hit at least once.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of hit breakpoints.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId is null or whitespace.</exception>
    public static async Task<IReadOnlyList<SagaDebugBreakpoint>> GetHitBreakpointsAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var breakpoints = await debugger.GetBreakpointsAsync(sagaId, cancellationToken);
        return breakpoints.Where(b => b.HitCount > 0).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the breakpoint with the highest hit count.
    /// </summary>
    /// <param name="debugger">The debugger service instance.</param>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The most frequently hit breakpoint, or null if none exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if sagaId is null or whitespace.</exception>
    public static async Task<SagaDebugBreakpoint?> GetMostHitBreakpointAsync(
        this SagaDebuggerService debugger,
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(debugger);
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var hitBreakpoints = await debugger.GetHitBreakpointsAsync(sagaId, cancellationToken);
        return hitBreakpoints.Count > 0 ? hitBreakpoints.MaxBy(b => b.HitCount) : null;
    }

    /// <summary>
    /// Formats a snapshot into a concise human-readable summary.
    /// </summary>
    private static string FormatSnapshotSummary(SagaDebugSnapshot snapshot)
    {
        var parts = new List<string>();
        parts.Add($"Snapshot #{snapshot.SequenceNumber} - {snapshot.Trigger}");
        parts.Add($"Status: {snapshot.SagaStatus} | Progress: {snapshot.ProgressPercent:F2}%");
        parts.Add($"Steps: {snapshot.CompletedStepCount}/{snapshot.Steps.Count} completed, {snapshot.FailedStepCount} failed");
        parts.Add($"Captured: {snapshot.CapturedAt:u}");

        if (snapshot.Label is not null)
        {
            parts.Add($"Label: {snapshot.Label}");
        }

        if (snapshot.FailureReason is not null)
        {
            parts.Add($"Failure: {snapshot.FailureReason}");
        }

        return string.Join(Environment.NewLine, parts);
    }
}
