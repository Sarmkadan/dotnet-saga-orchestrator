#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Configuration options for the distributed saga debugger.
/// Can be loaded from <c>appsettings.json</c> under the
/// <see cref="SectionName"/> key or configured programmatically via
/// <see cref="DebuggerOptionsBuilder"/>.
/// </summary>
public sealed class DebuggerOptions
{
    /// <summary>Configuration section key used when binding from <c>appsettings.json</c>.</summary>
    public const string SectionName = "SagaDebugger";

    /// <summary>
    /// Whether the debugger is active.  When <c>false</c> all operations become no-ops,
    /// adding zero overhead to production paths.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Maximum number of snapshots retained per saga in memory.
    /// Oldest snapshots are evicted when the limit is reached.
    /// Defaults to <c>50</c>.
    /// </summary>
    public int MaxSnapshotsPerSaga { get; set; } = 50;

    /// <summary>
    /// When <c>true</c>, a snapshot is automatically captured each time a step
    /// transitions to <c>Executing</c> or <c>Completed</c>.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool AutoCaptureOnStepTransition { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, a snapshot is automatically captured when the saga
    /// enters the compensation phase.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool AutoCaptureOnCompensation { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, a snapshot is automatically captured when the saga
    /// reaches any terminal state (Completed, Failed, Compensated, TimedOut).
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool AutoCaptureOnTerminalState { get; set; } = true;

    /// <summary>
    /// Maximum number of breakpoints that can be registered per saga simultaneously.
    /// Defaults to <c>20</c>.
    /// </summary>
    public int MaxBreakpointsPerSaga { get; set; } = 20;

    /// <summary>
    /// When <c>true</c>, step-level payload data is included in snapshots.
    /// Set to <c>false</c> in environments with strict data-residency requirements.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool IncludeStepPayloads { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, saga metadata is included in snapshots.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool IncludeSagaMetadata { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, the time-travel (<c>TravelTo</c>) feature is available.
    /// Disabling it prevents accidental state restoration in production.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool EnableTimeTravel { get; set; } = false;

    /// <summary>
    /// Returns a string representation of the debugger options.
    /// </summary>
    /// <returns>A formatted string with key property values.</returns>
    public override string ToString()
    {
        return $"DebuggerOptions {{ IsEnabled = {IsEnabled}, MaxSnapshotsPerSaga = {MaxSnapshotsPerSaga}, AutoCaptureOnStepTransition = {AutoCaptureOnStepTransition}, AutoCaptureOnCompensation = {AutoCaptureOnCompensation}, AutoCaptureOnTerminalState = {AutoCaptureOnTerminalState}, MaxBreakpointsPerSaga = {MaxBreakpointsPerSaga} }}";
    }
}

/// <summary>
/// Fluent builder for constructing a validated <see cref="DebuggerOptions"/> instance.
/// </summary>
/// <example>
/// <code>
/// var opts = new DebuggerOptionsBuilder()
///     .Enable()
///     .WithMaxSnapshotsPerSaga(100)
///     .WithAutoCapture(onStepTransition: true, onCompensation: true)
///     .WithTimeTravel(enabled: true)
///     .Build();
/// </code>
/// </example>
public sealed class DebuggerOptionsBuilder
{
    private readonly DebuggerOptions _options = new();

    /// <summary>
    /// Enables or disables the debugger.
    /// </summary>
    public DebuggerOptionsBuilder Enable(bool enabled = true)
    {
        _options.IsEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of snapshots retained per saga.
    /// </summary>
    /// <param name="max">Must be between 1 and 10 000.</param>
    public DebuggerOptionsBuilder WithMaxSnapshotsPerSaga(int max)
    {
        if (max < 1 || max > 10_000)
            throw new ArgumentOutOfRangeException(nameof(max), "Value must be between 1 and 10 000.");

        _options.MaxSnapshotsPerSaga = max;
        return this;
    }

    /// <summary>
    /// Configures the automatic snapshot capture triggers.
    /// </summary>
    /// <param name="onStepTransition">Capture on each step start or completion.</param>
    /// <param name="onCompensation">Capture when the saga enters compensation.</param>
    /// <param name="onTerminalState">Capture when the saga reaches a terminal state.</param>
    public DebuggerOptionsBuilder WithAutoCapture(
        bool onStepTransition = true,
        bool onCompensation   = true,
        bool onTerminalState  = true)
    {
        _options.AutoCaptureOnStepTransition = onStepTransition;
        _options.AutoCaptureOnCompensation   = onCompensation;
        _options.AutoCaptureOnTerminalState  = onTerminalState;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of simultaneous breakpoints per saga.
    /// </summary>
    /// <param name="max">Must be between 1 and 100.</param>
    public DebuggerOptionsBuilder WithMaxBreakpointsPerSaga(int max)
    {
        if (max < 1 || max > 100)
            throw new ArgumentOutOfRangeException(nameof(max), "Value must be between 1 and 100.");

        _options.MaxBreakpointsPerSaga = max;
        return this;
    }

    /// <summary>
    /// Controls whether step payload and saga metadata are included in snapshots.
    /// </summary>
    /// <param name="includePayloads">Include step output data.</param>
    /// <param name="includeMetadata">Include saga metadata dictionary.</param>
    public DebuggerOptionsBuilder WithDataInclusion(
        bool includePayloads = true,
        bool includeMetadata = true)
    {
        _options.IncludeStepPayloads  = includePayloads;
        _options.IncludeSagaMetadata  = includeMetadata;
        return this;
    }

    /// <summary>
    /// Enables or disables the time-travel (state restoration) feature.
    /// </summary>
    public DebuggerOptionsBuilder WithTimeTravel(bool enabled = true)
    {
        _options.EnableTimeTravel = enabled;
        return this;
    }

    /// <summary>
    /// Validates and returns the configured <see cref="DebuggerOptions"/> instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when time-travel is enabled but the debugger itself is disabled, or when
    /// <see cref="DebuggerOptions.MaxSnapshotsPerSaga"/> is zero.
    /// </exception>
    public DebuggerOptions Build()
    {
        Validate();
        return _options;
    }

    private void Validate()
    {
        if (_options.EnableTimeTravel && !_options.IsEnabled)
            throw new InvalidOperationException(
                "Time-travel cannot be enabled while the debugger itself is disabled.");

        if (_options.MaxSnapshotsPerSaga < 1)
            throw new InvalidOperationException(
                $"{nameof(DebuggerOptions.MaxSnapshotsPerSaga)} must be at least 1.");

        if (_options.MaxBreakpointsPerSaga < 1)
            throw new InvalidOperationException(
                $"{nameof(DebuggerOptions.MaxBreakpointsPerSaga)} must be at least 1.");
    }
}
