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
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Infrastructure.Debugging;

/// <summary>
/// Core engine for the distributed saga debugger.
/// Provides time-travel inspection by capturing immutable <see cref="SagaDebugSnapshot"/>
/// objects at key execution moments, and allows restoring any prior state back into the
/// repository layer for re-inspection or replay.  Breakpoints augment the live execution
/// loop with pause-and-inspect semantics without modifying the orchestration service.
/// </summary>
public sealed class SagaDebuggerService : ISagaDebugger
{
    private readonly ISagaRepository _sagaRepository;
    private readonly ISagaStepRepository _sagaStepRepository;
    private readonly SagaEventPublisher _eventPublisher;
    private readonly DebuggerOptions _options;

    // snapshot store: sagaId → ordered list of snapshots
    private readonly Dictionary<string, List<SagaDebugSnapshot>> _snapshots = new();

    // snapshot lookup by snapshotId for O(1) retrieval
    private readonly Dictionary<string, SagaDebugSnapshot> _snapshotIndex = new();

    // breakpoint store: sagaId → list of breakpoints
    private readonly Dictionary<string, List<SagaDebugBreakpoint>> _breakpoints = new();

    // per-saga sequence counters
    private readonly Dictionary<string, int> _sequenceCounters = new();

    private readonly object _lock = new();

    /// <summary>
    /// Initialises the debugger service with its required dependencies.
    /// </summary>
    /// <param name="sagaRepository">Repository used to read and update saga state.</param>
    /// <param name="sagaStepRepository">Repository used to read and update individual step state.</param>
    /// <param name="eventPublisher">Publisher whose event history feeds the debug timeline.</param>
    /// <param name="options">Debugger behaviour configuration.</param>
    public SagaDebuggerService(
        ISagaRepository sagaRepository,
        ISagaStepRepository sagaStepRepository,
        SagaEventPublisher eventPublisher,
        DebuggerOptions options)
    {
        _sagaRepository     = sagaRepository     ?? throw new ArgumentNullException(nameof(sagaRepository));
        _sagaStepRepository = sagaStepRepository ?? throw new ArgumentNullException(nameof(sagaStepRepository));
        _eventPublisher     = eventPublisher     ?? throw new ArgumentNullException(nameof(eventPublisher));
        _options            = options            ?? throw new ArgumentNullException(nameof(options));
    }

    // -------------------------------------------------------------------------
    // ISagaDebugger – snapshot management
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<SagaDebugSnapshot> CaptureSnapshotAsync(
        string sagaId,
        SnapshotTrigger trigger = SnapshotTrigger.Manual,
        string? label = null,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsEnabled)
            throw new InvalidOperationException(
                "The saga debugger is disabled. Set DebuggerOptions.IsEnabled = true to use this feature.");

        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must not be empty.", nameof(sagaId));

        var saga = await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new KeyNotFoundException($"Saga '{sagaId}' not found.");

        // Ensure the saga's Steps list is up to date from the step repository
        saga.Steps = (await _sagaStepRepository.GetBySagaIdAsync(sagaId))
            .OrderBy(s => s.Order)
            .ToList();

        SagaDebugSnapshot snapshot;
        lock (_lock)
        {
            var seq = NextSequenceNumber(sagaId);
            snapshot = SagaDebugSnapshot.Capture(saga, trigger, seq, label);

            if (!_snapshots.TryGetValue(sagaId, out var list))
            {
                list = new List<SagaDebugSnapshot>();
                _snapshots[sagaId] = list;
            }

            list.Add(snapshot);
            _snapshotIndex[snapshot.SnapshotId] = snapshot;

            EnforceSnapshotLimit(sagaId, list);
        }

        return snapshot;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<SagaDebugSnapshot>> GetSnapshotsAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_snapshots.TryGetValue(sagaId, out var list))
                return Task.FromResult<IReadOnlyList<SagaDebugSnapshot>>([]);

            IReadOnlyList<SagaDebugSnapshot> result = list
                .OrderBy(s => s.SequenceNumber)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(result);
        }
    }

    /// <inheritdoc/>
    public Task<SagaDebugSnapshot?> GetSnapshotAsync(
        string snapshotId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _snapshotIndex.TryGetValue(snapshotId, out var snapshot);
            return Task.FromResult(snapshot);
        }
    }

    /// <inheritdoc/>
    public Task PurgeSnapshotsAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_snapshots.TryGetValue(sagaId, out var list))
            {
                foreach (var s in list)
                    _snapshotIndex.Remove(s.SnapshotId);

                _snapshots.Remove(sagaId);
                _sequenceCounters.Remove(sagaId);
            }
        }

        return Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // ISagaDebugger – time-travel
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<SagaDebugSnapshot> TravelToAsync(
        string sagaId,
        string snapshotId,
        CancellationToken cancellationToken = default)
    {
        var target = await GetSnapshotAsync(snapshotId, cancellationToken)
            ?? throw new KeyNotFoundException($"Snapshot '{snapshotId}' not found.");

        if (target.SagaId != sagaId)
            throw new InvalidOperationException(
                $"Snapshot '{snapshotId}' belongs to saga '{target.SagaId}', not '{sagaId}'.");

        var saga = await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new KeyNotFoundException($"Saga '{sagaId}' not found.");

        // Restore saga-level mutable state from the snapshot
        saga.Status        = target.SagaStatus;
        saga.StartedAt     = target.SagaStartedAt;
        saga.CompletedAt   = target.SagaCompletedAt;
        saga.FailureReason = target.FailureReason;
        saga.RetryCount    = target.RetryCount;

        await _sagaRepository.UpdateAsync(saga).ConfigureAwait(false);

        // Restore step-level mutable state from the snapshot
        var existingSteps = await _sagaStepRepository.GetBySagaIdAsync(sagaId).ConfigureAwait(false);
        var stepById      = existingSteps.ToDictionary(s => s.Id);

        foreach (var stepState in target.Steps)
        {
            if (!stepById.TryGetValue(stepState.StepId, out var step))
                continue;

            step.Status        = stepState.Status;
            step.RetryCount    = stepState.RetryCount;
            step.StartedAt     = stepState.StartedAt;
            step.CompletedAt   = stepState.CompletedAt;
            step.CompensatedAt = stepState.CompensatedAt;
            step.ErrorMessage  = stepState.ErrorMessage;
            step.Response      = stepState.OutputData.ToDictionary(kv => kv.Key, kv => kv.Value);

            await _sagaStepRepository.UpdateAsync(step).ConfigureAwait(false);
        }

        var restorationLabel = $"Restored from snapshot #{target.SequenceNumber} captured {target.CapturedAt:u}";
        return await CaptureSnapshotAsync(sagaId, SnapshotTrigger.Manual, restorationLabel, cancellationToken).ConfigureAwait(false);
    }

    // -------------------------------------------------------------------------
    // ISagaDebugger – timeline
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<SagaDebugTimeline> GetTimelineAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        var saga = await _sagaRepository.GetByIdAsync(sagaId)
            ?? throw new KeyNotFoundException($"Saga '{sagaId}' not found.");

        IReadOnlyList<SagaDebugSnapshot> snapshots;
        IReadOnlyList<SagaDebugBreakpoint> breakpoints;

        lock (_lock)
        {
            snapshots   = _snapshots.TryGetValue(sagaId, out var sl)
                ? sl.OrderBy(s => s.SequenceNumber).ToList().AsReadOnly()
                : [];

            breakpoints = _breakpoints.TryGetValue(sagaId, out var bl)
                ? bl.ToList().AsReadOnly()
                : [];
        }

        var sagaEvents = _eventPublisher.GetSagaEvents(sagaId);
        var sagaName   = saga.Definition?.Name ?? sagaId;

        return SagaDebugTimeline.Build(
            sagaId,
            sagaName,
            saga.StartedAt,
            snapshots,
            sagaEvents,
            breakpoints);
    }

    // -------------------------------------------------------------------------
    // ISagaDebugger – breakpoints
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public Task<SagaDebugBreakpoint> SetBreakpointAsync(
        string sagaId,
        string stepName,
        string? note = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sagaId))   throw new ArgumentException("Saga ID must not be empty.",  nameof(sagaId));
        if (string.IsNullOrWhiteSpace(stepName)) throw new ArgumentException("Step name must not be empty.", nameof(stepName));

        var breakpoint = new SagaDebugBreakpoint
        {
            BreakpointId = Guid.NewGuid().ToString("N"),
            SagaId       = sagaId,
            StepName     = stepName,
            IsEnabled    = true,
            CreatedAt    = DateTime.UtcNow,
            HitCount     = 0,
            Note         = note,
        };

        lock (_lock)
        {
            if (!_breakpoints.TryGetValue(sagaId, out var list))
            {
                list = new List<SagaDebugBreakpoint>();
                _breakpoints[sagaId] = list;
            }

            EnforceBreakpointLimit(sagaId, list);
            list.Add(breakpoint);
        }

        return Task.FromResult(breakpoint);
    }

    /// <inheritdoc/>
    public Task<bool> RemoveBreakpointAsync(
        string breakpointId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            foreach (var (_, list) in _breakpoints)
            {
                var idx = list.FindIndex(b => b.BreakpointId == breakpointId);
                if (idx < 0) continue;
                list.RemoveAt(idx);
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<SagaDebugBreakpoint>> GetBreakpointsAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<SagaDebugBreakpoint> result = _breakpoints.TryGetValue(sagaId, out var list)
                ? list.ToList().AsReadOnly()
                : [];

            return Task.FromResult(result);
        }
    }

    /// <inheritdoc/>
    public Task ClearBreakpointsAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _breakpoints.Remove(sagaId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> CheckBreakpointAsync(
        string sagaId,
        string stepName,
        CancellationToken cancellationToken = default)
    {
        SagaDebugBreakpoint? hit = null;
        int hitIndex = -1;

        lock (_lock)
        {
            if (_breakpoints.TryGetValue(sagaId, out var list))
            {
                hitIndex = list.FindIndex(b =>
                    b.IsEnabled &&
                    string.Equals(b.StepName, stepName, StringComparison.OrdinalIgnoreCase));

                if (hitIndex >= 0)
                {
                    hit = list[hitIndex].WithIncrementedHitCount();
                    list[hitIndex] = hit;
                }
            }
        }

        if (hit is null)
            return false;

        await CaptureSnapshotAsync(
            sagaId,
            SnapshotTrigger.Breakpoint,
            $"Breakpoint hit: step '{stepName}' (total hits: {hit.HitCount})",
            cancellationToken);

        return true;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private int NextSequenceNumber(string sagaId)
    {
        _sequenceCounters.TryGetValue(sagaId, out var current);
        var next = current + 1;
        _sequenceCounters[sagaId] = next;
        return next;
    }

    private void EnforceSnapshotLimit(string sagaId, List<SagaDebugSnapshot> list)
    {
        while (list.Count > _options.MaxSnapshotsPerSaga)
        {
            _snapshotIndex.Remove(list[0].SnapshotId);
            list.RemoveAt(0);
        }
    }

    private void EnforceBreakpointLimit(string sagaId, List<SagaDebugBreakpoint> list)
    {
        while (list.Count >= _options.MaxBreakpointsPerSaga)
            list.RemoveAt(0);
    }
}
