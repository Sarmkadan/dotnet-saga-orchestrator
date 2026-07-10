#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Infrastructure.Debugging;

namespace SagaOrchestrator.Infrastructure.Debugging;

/// <summary>
/// Provides validation helpers for <see cref="SagaDebuggerService"/> instances.
/// </summary>
public static class SagaDebuggerServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="SagaDebuggerService"/> instance.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this SagaDebuggerService value)
    {
        if (value is null)
        {
            return new[] { "SagaDebuggerService instance is null." };
        }

        var problems = new List<string>();

        // Validate internal state consistency
        try
        {
            // These validations access private fields via reflection to check internal consistency
            var snapshotsField = typeof(SagaDebuggerService).GetField("_snapshots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var breakpointsField = typeof(SagaDebuggerService).GetField("_breakpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sequenceCountersField = typeof(SagaDebuggerService).GetField("_sequenceCounters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var snapshotIndexField = typeof(SagaDebuggerService).GetField("_snapshotIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var snapshots = (Dictionary<string, List<SagaDebugSnapshot>>?)snapshotsField?.GetValue(value);
            var breakpoints = (Dictionary<string, List<SagaDebugBreakpoint>>?)breakpointsField?.GetValue(value);
            var sequenceCounters = (Dictionary<string, int>?)sequenceCountersField?.GetValue(value);
            var snapshotIndex = (Dictionary<string, SagaDebugSnapshot>?)snapshotIndexField?.GetValue(value);

            // Validate snapshot index consistency
            if (snapshotIndex != null)
            {
                foreach (var snapshot in snapshotIndex.Values)
                {
                    if (snapshot == null)
                    {
                        problems.Add("Snapshot index contains null entry.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(snapshot.SnapshotId))
                    {
                        problems.Add("Snapshot in index has null or empty SnapshotId.");
                    }

                    if (snapshot.SequenceNumber <= 0)
                    {
                        problems.Add($"Snapshot '{snapshot.SnapshotId}' has invalid SequenceNumber {snapshot.SequenceNumber}.");
                    }
                }
            }

            // Validate snapshot lists consistency
            if (snapshots != null)
            {
                foreach (var sagaSnapshots in snapshots.Values)
                {
                    if (sagaSnapshots == null)
                    {
                        problems.Add("Null snapshot list found in _snapshots dictionary.");
                        continue;
                    }

                    for (int i = 0; i < sagaSnapshots.Count; i++)
                    {
                        var snapshot = sagaSnapshots[i];
                        if (snapshot == null)
                        {
                            problems.Add($"Snapshot at index {i} in saga snapshots list is null.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(snapshot.SnapshotId))
                        {
                            problems.Add($"Snapshot at index {i} has null or empty SnapshotId.");
                        }

                        if (snapshot.SequenceNumber <= 0)
                        {
                            problems.Add($"Snapshot '{snapshot.SnapshotId}' has invalid SequenceNumber {snapshot.SequenceNumber}.");
                        }

                        if (snapshot.CapturedAt == default)
                        {
                            problems.Add($"Snapshot '{snapshot.SnapshotId}' has default CapturedAt date.");
                        }

                        if (snapshot.SagaId != snapshot.SagaId?.Trim())
                        {
                            problems.Add($"Snapshot '{snapshot.SnapshotId}' has whitespace in SagaId.");
                        }
                    }
                }
            }

            // Validate breakpoints consistency
            if (breakpoints != null)
            {
                foreach (var sagaBreakpoints in breakpoints.Values)
                {
                    if (sagaBreakpoints == null)
                    {
                        problems.Add("Null breakpoint list found in _breakpoints dictionary.");
                        continue;
                    }

                    for (int i = 0; i < sagaBreakpoints.Count; i++)
                    {
                        var breakpoint = sagaBreakpoints[i];
                        if (breakpoint == null)
                        {
                            problems.Add($"Breakpoint at index {i} in saga breakpoints list is null.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(breakpoint.BreakpointId))
                        {
                            problems.Add($"Breakpoint at index {i} has null or empty BreakpointId.");
                        }

                        if (string.IsNullOrWhiteSpace(breakpoint.SagaId))
                        {
                            problems.Add($"Breakpoint '{breakpoint.BreakpointId}' has null or empty SagaId.");
                        }

                        if (string.IsNullOrWhiteSpace(breakpoint.StepName))
                        {
                            problems.Add($"Breakpoint '{breakpoint.BreakpointId}' has null or empty StepName.");
                        }

                        if (breakpoint.CreatedAt == default)
                        {
                            problems.Add($"Breakpoint '{breakpoint.BreakpointId}' has default CreatedAt date.");
                        }

                        if (breakpoint.HitCount < 0)
                        {
                            problems.Add($"Breakpoint '{breakpoint.BreakpointId}' has negative HitCount {breakpoint.HitCount}.");
                        }
                    }
                }
            }

            // Validate sequence counters
            if (sequenceCounters != null)
            {
                foreach (var kvp in sequenceCounters)
                {
                    if (kvp.Value <= 0)
                    {
                        problems.Add($"Sequence counter for saga '{kvp.Key}' has invalid value {kvp.Value}.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Validation failed with exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaDebuggerService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this SagaDebuggerService value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SagaDebuggerService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the service is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this SagaDebuggerService value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "SagaDebuggerService instance cannot be null.");
        }

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"SagaDebuggerService is invalid. Problems:\n{string.Join("\n", problems.Select((p, i) => $"  {i + 1}. {p}"))}",
            nameof(value));
    }
}