# DebuggerOptions

The `DebuggerOptions` class provides configurable settings for the saga debugger in the `dotnet-saga-orchestrator` project. It controls whether debugging is enabled, how snapshots and breakpoints are managed, which data is captured automatically, and whether time-travel debugging is available. The class exposes both direct property access and a fluent builder API (via `DebuggerOptionsBuilder`) for constructing instances with a consistent set of options.

## API

### Properties

- **`IsEnabled`** (`bool`)  
  Gets or sets whether the debugger is active. When `false`, all debugging features are disabled regardless of other settings.

- **`MaxSnapshotsPerSaga`** (`int`)  
  Gets or sets the maximum number of snapshots retained per saga instance.  
  *Throws*: `ArgumentOutOfRangeException` if set to a negative value.

- **`AutoCaptureOnStepTransition`** (`bool`)  
  Gets or sets whether a snapshot is automatically captured each time a saga transitions between steps.

- **`AutoCaptureOnCompensation`** (`bool`)  
  Gets or sets whether a snapshot is automatically captured when a saga compensates a step.

- **`AutoCaptureOnTerminalState`** (`bool`)  
  Gets or sets whether a snapshot is automatically captured when a saga reaches a terminal state (completed, faulted, or suspended).

- **`MaxBreakpointsPerSaga`** (`int`)  
  Gets or sets the maximum number of breakpoints that can be active per saga instance.  
  *Throws*: `ArgumentOutOfRangeException` if set to a negative value.

- **`IncludeStepPayloads`** (`bool`)  
  Gets or sets whether the payload of each step is included in debug snapshots and breakpoint data.

- **`IncludeSagaMetadata`** (`bool`)  
  Gets or sets whether saga-level metadata (e.g., saga ID, state machine type) is included in debug data.

- **`EnableTimeTravel`** (`bool`)  
  Gets or sets whether time-travel debugging is enabled, allowing replay of saga execution from captured snapshots.

### Builder Methods

Each of the following methods returns a `DebuggerOptionsBuilder` instance that can be used to fluently configure a new `DebuggerOptions` object. The builder methods do not modify the current `DebuggerOptions` instance; they create a fresh builder pre‑populated with the current instance’s values (or defaults) and then apply the specified configuration.

- **`Enable()`**  
  Returns a builder with `IsEnabled` set to `true`. All other settings retain their current values (or defaults if called on a newly created `DebuggerOptions`).

- **`WithMaxSnapshotsPerSaga(int maxSnapshots)`**  
  Returns a builder with `MaxSnapshotsPerSaga` set to the specified value.  
  *Throws*: `ArgumentOutOfRangeException` if `maxSnapshots` is negative.

- **`WithAutoCapture()`**  
  Returns a builder with auto‑capture settings configured. The exact behavior (which of the three `AutoCapture*` properties are enabled) is determined by the implementation; typically all three are set to `true`.

- **`WithMaxBreakpointsPerSaga(int maxBreakpoints)`**  
  Returns a builder with `MaxBreakpointsPerSaga` set to the specified value.  
  *Throws*: `ArgumentOutOfRangeException` if `maxBreakpoints` is negative.

- **`WithDataInclusion()`**  
  Returns a builder with data inclusion settings configured. The exact behavior (which of `IncludeStepPayloads` and `IncludeSagaMetadata` are enabled) is determined by the implementation; typically both are set to `true`.

- **`WithTimeTravel()`**  
  Returns a builder with `EnableTimeTravel` set to `true`.

- **`Build()`**  
  Creates and returns a new `DebuggerOptions` instance with the settings accumulated in the builder. The builder is not affected and can be reused.  
  *Throws*: No exceptions under normal circumstances.

## Usage

### Example 1: Fluent builder pattern

```csharp
using SagaOrchestrator.Debugging;

var options = new DebuggerOptions()
    .Enable()
    .WithMaxSnapshotsPerSaga(50)
    .WithAutoCapture()
    .WithMaxBreakpointsPerSaga(10)
    .WithDataInclusion()
    .WithTimeTravel()
    .Build();

// options.IsEnabled == true
// options.MaxSnapshotsPerSaga == 50
// options.AutoCaptureOnStepTransition == true
// options.AutoCaptureOnCompensation == true
// options.AutoCaptureOnTerminalState == true
// options.MaxBreakpointsPerSaga == 10
// options.IncludeStepPayloads == true
// options.IncludeSagaMetadata == true
// options.EnableTimeTravel == true
```

### Example 2: Direct property assignment (without builder)

```csharp
using SagaOrchestrator.Debugging;

var options = new DebuggerOptions
{
    IsEnabled = true,
    MaxSnapshotsPerSaga = 20,
    AutoCaptureOnStepTransition = false,
    AutoCaptureOnCompensation = true,
    AutoCaptureOnTerminalState = true,
    MaxBreakpointsPerSaga = 5,
    IncludeStepPayloads = false,
    IncludeSagaMetadata = true,
    EnableTimeTravel = false
};
```

## Notes

- **Thread safety**: `DebuggerOptions` is not thread‑safe. Concurrent reads and writes to its properties can produce inconsistent state. If the same instance is accessed from multiple threads, external synchronization (e.g., a lock) is required. The builder methods (`Enable`, `With*`, `Build`) are also not thread‑safe; they should be used from a single thread or protected by synchronization.
- **Negative values**: Setting `MaxSnapshotsPerSaga` or `MaxBreakpointsPerSaga` to a negative value throws `ArgumentOutOfRangeException`. A value of zero is allowed and disables the corresponding feature (no snapshots or no breakpoints).
- **Builder immutability**: The builder methods do not modify the original `DebuggerOptions` instance. Each call returns a new builder that is independent of the source. The `Build()` method creates a new `DebuggerOptions` object; the builder can be reused to produce multiple distinct configurations.
- **Default values**: When a `DebuggerOptions` is created with the parameterless constructor, all boolean properties default to `false` and integer properties default to `0`. The builder methods that accept no parameters (e.g., `WithAutoCapture`) typically enable all related options, but the exact defaults may vary by implementation.
- **Time‑travel dependency**: Enabling `EnableTimeTravel` has no effect unless `IsEnabled` is also `true` and snapshots are being captured (either manually or via auto‑capture).
