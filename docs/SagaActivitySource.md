# SagaActivitySource

Provides predefined `Activity` instances for tracing the lifecycle of sagas, steps, and compensations in the dotnet‑saga‑orchestrator library. These static members allow consistent correlation of diagnostic information across asynchronous workflow boundaries without requiring callers to create their own `ActivitySource`.

## API

### StartSaga
- **Purpose**: Begins a new saga activity that represents the overall workflow execution.
- **Parameters**: None.
- **Return value**: An `Activity?` representing the started saga, or `null` if tracing is disabled.
- **Exceptions**: Throws `InvalidOperationException` if the underlying `ActivitySource` has been disposed.

### RecordSagaComplete
- **Purpose**: Creates an activity that marks the successful completion of a saga.
- **Parameters**: None.
- **Return value**: An `Activity?` representing the completion event, or `null` if tracing is disabled.
- **Exceptions**: Throws `InvalidOperationException` if the underlying `ActivitySource` has been disposed.

### StartStep
- **Purpose**: Starts an activity for an individual step within a saga.
- **Parameters**: None.
- **Return value**: An `Activity?` representing the step execution, or `null` if tracing is disabled.
- **Exceptions**: Throws `InvalidOperationException` if the underlying `ActivitySource` has been disposed.

### RecordStepFailure
- **Purpose**: Records a failure for the currently active step activity.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws `InvalidOperationException` if no step activity is currently active or if the underlying `ActivitySource` has been disposed.

### StartCompensation
- **Purpose**: Begins an activity for a compensation action triggered by a step failure.
- **Parameters**: None.
- **Return value**: An `Activity?` representing the compensation execution, or `null` if tracing is disabled.
- **Exceptions**: Throws `InvalidOperationException` if the underlying `ActivitySource` has been disposed.

### RecordCompensationFailure
- **Purpose**: Records a failure for the currently active compensation activity.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws `InvalidOperationException` if no compensation activity is currently active or if the underlying `ActivitySource` has been disposed.

## Usage

```csharp
using System.Diagnostics;
using DotNetSagaOrchestrator.Tracing; // namespace containing SagaActivitySource

public class OrderSaga
{
    public async Task ExecuteAsync()
    {
        using var sagaActivity = SagaActivitySource.StartSaga?.Start();
        if (sagaActivity == null) { /* tracing disabled – continue without tracing */ }

        try
        {
            await ProcessPaymentAsync();
            await ReserveInventoryAsync();
            SagaActivitySource.RecordSagaComplete?.Start()?.Dispose();
        }
        catch (Exception ex)
        {
            SagaActivitySource.RecordStepFailure?.Invoke();
            await CompensateAsync(ex);
        }
    }

    private async Task ProcessPaymentAsync()
    {
        using var step = SagaActivitySource.StartStep?.Start();
        // ... (stepActivitySource.StartStep?.Start()?.Dispose();
    }

    private async Task ReserveInventoryAsync()
    {
        using var step = SagaActivitySource.StartStep?.Start();
        // inventory reservation logic
    }

    private async Task CompensateAsync(Exception original)
    {
        using var comp = SagaActivitySource.StartCompensation?.Start();
        // compensation logic (e.g., refund, release inventory)
        if (compensationFailed)
        {
            SagaActivitySource.RecordCompensationFailure?.Invoke();
        }
    }
}
```

```csharp
// Example showing manual activity management when more control is needed
public class ManualSagaRunner
{
    public void Run()
    {
        var saga = SagaActivitySource.StartSaga;
        if (saga != null)
        {
            saga.Start();
            try
            {
                DoWork();
                var complete = SagaActivitySource.RecordSagaComplete;
                complete?.Start()?.Dispose();
            }
            catch
            {
                SagaActivitySource.RecordStepFailure?.Invoke();
                throw;
            }
            finally
            {
                saga.Dispose();
            }
        }
        else
        {
            DoWork(); // tracing disabled
        }
    }

    private void DoWork()
    {
        var step = SagaActivitySource.StartStep;
        if (step != null)
        {
            using (step.Start())
            {
                // step body
            }
        }
        else
        {
            // step body without tracing
        }
    }
}
```

## Notes

- All members are **static** and safe to call from multiple threads concurrently; the underlying `ActivitySource` is initialized once during type construction.
- Returned `Activity?` instances must be disposed (typically via a `using` statement or explicit `Dispose`) to end the associated timing segment. Failure to dispose may cause incomplete or overlapping traces.
- If tracing is disabled (e.g., via configuration of the `ActivitySource`), the members return `null`; calling `Start()` on a null reference will throw a `NullReferenceException`. Guard against this by checking for null before use.
- `RecordStepFailure` and `RecordCompensationFailure` rely on the existence of a currently active step or compensation activity, respectively. Invoking them when no such activity is active results in an `InvalidOperationException`.
- The members do **not** accept parameters; they implicitly associate with the ambient activity context (`Activity.Current`) at the moment of invocation. Ensure the correct activity is current before calling the recording methods.
