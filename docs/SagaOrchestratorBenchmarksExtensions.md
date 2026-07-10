# SagaOrchestratorBenchmarksExtensions

Provides static utility methods for executing and measuring saga orchestrator benchmark scenarios, including warm-up runs, timed action execution, full cycle execution, and safe entry-point invocation for BenchmarkDotNet harnesses.

## API

### RunFullCycle
```csharp
public static void RunFullCycle(ISagaOrchestrator orchestrator, SagaContext context, CancellationToken cancellationToken = default)
```
Executes a complete saga lifecycle (start, process steps, compensate on failure, or complete) using the provided orchestrator and context. Intended for benchmark iterations where a full end-to-end run is measured.

**Parameters**
- `orchestrator`: The saga orchestrator instance to drive the cycle.
- `context`: The saga context containing payload, state, and step definitions.
- `cancellationToken`: Optional token to abort the cycle early.

**Throws**
- `OperationCanceledException` if `cancellationToken` is triggered.
- `SagaExecutionException` if the orchestrator reports a fatal error during the cycle.
- `ArgumentNullException` if `orchestrator` or `context` is null.

---

### TimeAction
```csharp
public static TimeSpan TimeAction(Action action)
```
Executes `action` and returns the elapsed wall-clock time as a `TimeSpan`. Uses `Stopwatch.GetTimestamp()` for high-resolution measurement.

**Parameters**
- `action`: The delegate to execute and measure.

**Returns**
- `TimeSpan` representing the elapsed time of `action`.

**Throws**
- `ArgumentNullException` if `action` is null.
- Any exception thrown by `action` propagates unchanged.

---

### WarmUp
```csharp
public static void WarmUp(ISagaOrchestrator orchestrator, SagaContext context, int iterations = 3, CancellationToken cancellationToken = default)
```
Runs `iterations` full saga cycles without timing to allow JIT compilation, tiered compilation, and runtime optimizations to stabilize before measured benchmark runs.

**Parameters**
- `orchestrator`: The saga orchestrator instance to warm up.
- `context`: The saga context used for each warm-up cycle.
- `iterations`: Number of warm-up cycles to execute (default 3).
- `cancellationToken`: Optional token to abort warm-up early.

**Throws**
- `ArgumentNullException` if `orchestrator` or `context` is null.
- `ArgumentOutOfRangeException` if `iterations` is less than 1.
- `OperationCanceledException` if `cancellationToken` is triggered.
- `SagaExecutionException` if any warm-up cycle fails fatally.

---

### TryRunMain
```csharp
public static bool TryRunMain(string[] args, Action<BenchmarkSwitcher> configure)
```
Attempts to run the BenchmarkDotNet entry point with the provided command-line arguments and a configuration action for the `BenchmarkSwitcher`. Returns `false` if arguments indicate help/version requests or if parsing fails; otherwise runs benchmarks and returns `true`.

**Parameters**
- `args`: Command-line arguments passed to the process.
- `configure`: Action that receives the `BenchmarkSwitcher` to add benchmarks, filters, or custom configuration.

**Returns**
- `true` if benchmarks were executed; `false` if the run was skipped (e.g., `--help`, `--version`, or invalid args).

**Throws**
- `ArgumentNullException` if `args` or `configure` is null.
- Exceptions from BenchmarkDotNet internals (e.g., `BenchmarkException`) propagate if not handled by the switcher.

## Usage

### Example 1: Benchmark harness entry point
```csharp
public static class Program
{
    public static int Main(string[] args)
    {
        return SagaOrchestratorBenchmarksExtensions.TryRunMain(args, switcher =>
        {
            switcher.AddBenchmark<OrderSagaBenchmarks>();
            switcher.AddBenchmark<PaymentSagaBenchmarks>();
        }) ? 0 : -1;
    }
}
```

### Example 2: Custom benchmark iteration with warm-up and timing
```csharp
[Benchmark]
public TimeSpan MeasureFullOrderCycle()
{
    var orchestrator = _serviceProvider.GetRequiredService<ISagaOrchestrator>();
    var context = CreateOrderContext();

    SagaOrchestratorBenchmarksExtensions.WarmUp(orchestrator, context, iterations: 5);

    return SagaOrchestratorBenchmarksExtensions.TimeAction(() =>
    {
        SagaOrchestratorBenchmarksExtensions.RunFullCycle(orchestrator, context, CancellationToken.None);
    });
}
```

## Notes

- **Thread safety**: All methods are stateless and operate solely on passed arguments. They are safe for concurrent use provided the `ISagaOrchestrator` and `SagaContext` implementations are themselves thread-safe or confined to a single benchmark thread.
- **Cancellation**: `RunFullCycle` and `WarmUp` respect `CancellationToken`; cancellation aborts the current cycle cleanly but does not roll back already-completed steps unless the orchestrator implements compensation on cancellation.
- **Warm-up iterations**: The default of 3 iterations suits most JIT scenarios; increase for complex sagas with heavy first-run initialization (e.g., expression tree compilation, dynamic proxy generation).
- **TimeAction resolution**: Uses `Stopwatch.GetTimestamp()` / `Stopwatch.Frequency` for nanosecond-resolution timing on supported platforms; falls back to `DateTime.UtcNow` ticks on older runtimes.
- **TryRunMain exit codes**: Returns `false` for `--help`, `--version`, `--list`, or unrecognized arguments; the caller should map `false` to a non-zero exit code if desired.
- **Exception propagation**: `TimeAction` does not swallow exceptions; benchmark harnesses should handle or log them. `RunFullCycle` wraps orchestrator failures in `SagaExecutionException` to distinguish infrastructure errors from business logic compensation.
