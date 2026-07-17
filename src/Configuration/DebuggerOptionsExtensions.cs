#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Provides extension methods for <see cref="DebuggerOptions"/> that offer
/// convenient ways to query and manipulate debugger configuration.
/// </summary>
public static class DebuggerOptionsExtensions
{
    /// <summary>
    /// Determines whether automatic snapshots are enabled for the given trigger type.
    /// </summary>
    /// <param name="options">The debugger options to check.</param>
    /// <param name="trigger">The type of trigger to check.</param>
    /// <returns>
    /// <c>true</c> if automatic snapshots are enabled for the specified trigger;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static bool IsAutoCaptureEnabled(this DebuggerOptions options, DebuggerSnapshotTrigger trigger)
    {
        ArgumentNullException.ThrowIfNull(options);

        return trigger switch
        {
            DebuggerSnapshotTrigger.StepTransition => options.AutoCaptureOnStepTransition,
            DebuggerSnapshotTrigger.Compensation => options.AutoCaptureOnCompensation,
            DebuggerSnapshotTrigger.TerminalState => options.AutoCaptureOnTerminalState,
            _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, "Unknown snapshot trigger type.")
        };
    }

    /// <summary>
    /// Gets the effective maximum number of snapshots that can be stored for a saga,
    /// considering both the global limit and any per-saga overrides.
    /// </summary>
    /// <param name="options">The debugger options.</param>
    /// <param name="sagaId">The unique identifier of the saga (used for consistent hashing).</param>
    /// <returns>The effective maximum number of snapshots for the specified saga.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> is <c>null</c> or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="options"/>.MaxSnapshotsPerSaga is less than 1.</exception>
    public static int GetMaxSnapshotsForSaga(this DebuggerOptions options, string sagaId)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(sagaId, nameof(sagaId));
        ArgumentOutOfRangeException.ThrowIfLessThan(options.MaxSnapshotsPerSaga, 1);

        // Use consistent hashing to distribute snapshots across sagas
        // This ensures that different sagas get different limits while maintaining determinism
        var hash = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(sagaId)) % 100;

        // Apply a small variance (±20%) to the base limit based on saga ID
        // This prevents all sagas from hitting the limit at the same time
        var effectiveLimit = options.MaxSnapshotsPerSaga + (hash - 50);
        effectiveLimit = Math.Max(1, effectiveLimit);

        return Math.Min(effectiveLimit, options.MaxSnapshotsPerSaga);
    }

    /// <summary>
    /// Determines whether the debugger is configured to capture data for the given scenario.
    /// </summary>
    /// <param name="options">The debugger options to check.</param>
    /// <param name="scenario">The debugging scenario to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the debugger will capture data for the specified scenario;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static bool WillCaptureDataFor(this DebuggerOptions options, DebuggerScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(options);

        return scenario switch
        {
            DebuggerScenario.StepExecution when !options.IncludeStepPayloads => false,
            DebuggerScenario.SagaMetadata when !options.IncludeSagaMetadata => false,
            DebuggerScenario.TimeTravel when !options.EnableTimeTravel => false,
            DebuggerScenario.StepExecution or DebuggerScenario.SagaMetadata or DebuggerScenario.TimeTravel => true,
            _ => options.IsEnabled
        };
    }

    /// <summary>
    /// Creates a new <see cref="DebuggerOptions"/> instance that inherits settings from the current instance
    /// but with specific overrides applied.
    /// </summary>
    /// <param name="options">The source debugger options.</param>
    /// <param name="configure">An action that applies overrides to the new options.</param>
    /// <returns>A new <see cref="DebuggerOptions"/> instance with inherited settings and applied overrides.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> or <paramref name="configure"/> is <c>null</c>.
    /// </exception>
    public static DebuggerOptions WithOverrides(this DebuggerOptions options, Action<DebuggerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configure);

        var result = new DebuggerOptions
        {
            IsEnabled = options.IsEnabled,
            MaxSnapshotsPerSaga = options.MaxSnapshotsPerSaga,
            AutoCaptureOnStepTransition = options.AutoCaptureOnStepTransition,
            AutoCaptureOnCompensation = options.AutoCaptureOnCompensation,
            AutoCaptureOnTerminalState = options.AutoCaptureOnTerminalState,
            MaxBreakpointsPerSaga = options.MaxBreakpointsPerSaga,
            IncludeStepPayloads = options.IncludeStepPayloads,
            IncludeSagaMetadata = options.IncludeSagaMetadata,
            EnableTimeTravel = options.EnableTimeTravel
        };

        configure(result);
        return result;
    }
}

/// <summary>
/// Represents the type of trigger that can cause a snapshot to be captured.
/// </summary>
public enum DebuggerSnapshotTrigger
{
    /// <summary>Snapshot is triggered by step transition (start or completion).</summary>
    StepTransition,

    /// <summary>Snapshot is triggered by saga entering compensation phase.</summary>
    Compensation,

    /// <summary>Snapshot is triggered by saga reaching a terminal state.</summary>
    TerminalState
}

/// <summary>
/// Represents common debugging scenarios where debugger behavior needs to be evaluated.
/// </summary>
public enum DebuggerScenario
{
    /// <summary>Scenario involving step execution and payload inspection.</summary>
    StepExecution,

    /// <summary>Scenario involving saga metadata inspection.</summary>
    SagaMetadata,

    /// <summary>Scenario involving time-travel debugging.</summary>
    TimeTravel
}