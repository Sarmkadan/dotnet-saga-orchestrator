#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Infrastructure.Debugging;

/// <summary>
/// Contract for the distributed saga debugger with time-travel inspection.
/// Provides snapshot management, breakpoint control, and chronological timeline
/// reconstruction for any saga managed by the orchestrator.
/// </summary>
/// <remarks>
/// <para>
/// <b>Time-travel inspection</b> – Call <see cref="CaptureSnapshotAsync"/> at strategic points
/// (or rely on auto-capture via <see cref="CheckBreakpointAsync"/>) to record immutable
/// <see cref="SagaDebugSnapshot"/> objects. Call <see cref="TravelToAsync"/> to restore the
/// saga and its steps in the repository back to the state recorded in any past snapshot,
/// enabling re-inspection or incremental replay.
/// </para>
/// <para>
/// <b>Breakpoints</b> – Register a step name with <see cref="SetBreakpointAsync"/>, then call
/// <see cref="CheckBreakpointAsync"/> from within the execution loop. When the step name matches
/// an enabled breakpoint, execution should be paused and a snapshot is automatically captured
/// with <see cref="SnapshotTrigger.Breakpoint"/>.
/// </para>
/// </remarks>
public interface ISagaDebugger
{
    // -------------------------------------------------------------------------
    // Snapshot management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Captures the current live state of a saga and all its steps as an immutable snapshot.
    /// </summary>
    /// <param name="sagaId">Identifier of the saga to snapshot.</param>
    /// <param name="trigger">
    /// The reason this snapshot is being created.
    /// Defaults to <see cref="SnapshotTrigger.Manual"/>.
    /// </param>
    /// <param name="label">Optional human-readable annotation stored with the snapshot.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>The newly created, stored snapshot.</returns>
    Task<SagaDebugSnapshot> CaptureSnapshotAsync(
        string sagaId,
        SnapshotTrigger trigger = SnapshotTrigger.Manual,
        string? label = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all snapshots for a saga, ordered by ascending sequence number.
    /// </summary>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task<IReadOnlyList<SagaDebugSnapshot>> GetSnapshotsAsync(
        string sagaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single snapshot by its unique identifier.
    /// </summary>
    /// <param name="snapshotId">Identifier of the snapshot.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>The snapshot, or <c>null</c> if it does not exist.</returns>
    Task<SagaDebugSnapshot?> GetSnapshotAsync(
        string snapshotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all snapshots stored for a saga, freeing memory.
    /// </summary>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task PurgeSnapshotsAsync(
        string sagaId,
        CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Time-travel
    // -------------------------------------------------------------------------

    /// <summary>
    /// Restores a saga and all its steps in the underlying repositories to the state recorded
    /// in the specified snapshot, enabling re-inspection or replay from that point.
    /// After restoration a new snapshot is automatically captured and returned.
    /// </summary>
    /// <param name="sagaId">Identifier of the saga to restore.</param>
    /// <param name="snapshotId">Identifier of the target snapshot.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>
    /// A new snapshot taken immediately after restoration, labelled with the source snapshot reference.
    /// </returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">
    /// Thrown when the saga or snapshot cannot be found.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the snapshot does not belong to the specified saga.
    /// </exception>
    Task<SagaDebugSnapshot> TravelToAsync(
        string sagaId,
        string snapshotId,
        CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Timeline
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a unified chronological timeline for a saga, interleaving debug snapshots
    /// and published domain events into a single ordered view.
    /// </summary>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task<SagaDebugTimeline> GetTimelineAsync(
        string sagaId,
        CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Breakpoints
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a breakpoint that pauses execution and auto-captures a snapshot when
    /// the step named <paramref name="stepName"/> is reached within <paramref name="sagaId"/>.
    /// </summary>
    /// <param name="sagaId">Saga this breakpoint applies to.</param>
    /// <param name="stepName">Name of the step that should trigger the pause.</param>
    /// <param name="note">Optional description for the breakpoint.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>The newly registered <see cref="SagaDebugBreakpoint"/>.</returns>
    Task<SagaDebugBreakpoint> SetBreakpointAsync(
        string sagaId,
        string stepName,
        string? note = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a breakpoint by its unique identifier.
    /// </summary>
    /// <param name="breakpointId">Identifier of the breakpoint to remove.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns><c>true</c> if the breakpoint was found and removed; <c>false</c> otherwise.</returns>
    Task<bool> RemoveBreakpointAsync(
        string breakpointId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all breakpoints currently registered for a saga.
    /// </summary>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task<IReadOnlyList<SagaDebugBreakpoint>> GetBreakpointsAsync(
        string sagaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all breakpoints registered for a saga.
    /// </summary>
    /// <param name="sagaId">Identifier of the saga.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task ClearBreakpointsAsync(
        string sagaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a breakpoint is registered for the given saga and step.
    /// When a matching enabled breakpoint is found, its hit counter is incremented and a
    /// <see cref="SnapshotTrigger.Breakpoint"/> snapshot is automatically captured.
    /// </summary>
    /// <param name="sagaId">Identifier of the executing saga.</param>
    /// <param name="stepName">Name of the step that is about to execute.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>
    /// <c>true</c> if a matching enabled breakpoint was found (execution should pause);
    /// <c>false</c> otherwise.
    /// </returns>
    Task<bool> CheckBreakpointAsync(
        string sagaId,
        string stepName,
        CancellationToken cancellationToken = default);
}
