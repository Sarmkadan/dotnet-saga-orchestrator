# SagaOrchestrationService

`SagaOrchestrationService` is the central coordinator for long-running distributed transactions implemented as sagas. It manages the full lifecycle of saga instances—creation, step-by-step execution, timeout handling, compensation on failure, and explicit abort—while maintaining durable state so that progress can survive process restarts. The service is designed for scenarios where a business process spans multiple services and must guarantee eventual consistency through forward execution and backward compensation.

## API

### CreateSagaAsync
```csharp
public async Task<Saga> CreateSagaAsync(/* saga definition and initial data */)
```
Creates a new saga instance in a `Pending` or equivalent initial state. It persists the saga record along with its ordered step definitions and initial payload, but does not begin execution. Returns the fully materialized `Saga` object with its generated identifier. Throws if the saga definition is invalid, if required initial data is missing, or if the underlying persistence layer is unavailable.

### StartSagaAsync
```csharp
public async Task<Saga> StartSagaAsync(/* saga identifier */)
```
Transitions a previously created saga from its initial state into the `Running` state and immediately triggers execution of the first step. Returns the updated `Saga` reflecting the outcome of that first step. Throws if the saga does not exist, is not in a startable state, or if the first step fails in a non-retryable manner before compensation has been recorded.

### ExecuteNextStepAsync
```csharp
public async Task<SagaStep> ExecuteNextStepAsync(/* saga identifier */)
```
Evaluates the current state of the specified saga and executes the next pending step in the defined sequence. Each step invocation is recorded durably before the step’s action is dispatched. Returns the `SagaStep` result containing the step’s outcome, any produced data, and whether the saga has completed all forward steps. Throws if the saga is not in `Running` state, if the step definition references an unknown handler, or if persistence of step state fails.

### HandleTimeoutAsync
```csharp
public async Task<bool> HandleTimeoutAsync(/* saga identifier and timeout metadata */)
```
Called when a previously scheduled timeout for a saga step or the saga itself has elapsed. Evaluates whether the step is still in a state where the timeout is relevant and, if so, marks the step as timed out and initiates compensation or abort logic. Returns `true` if the timeout was actionable and processed; returns `false` if the saga or step had already progressed past the point where the timeout applies. Throws if the saga identifier is invalid or if persistence operations fail.

### CompensateSagaAsync
```csharp
public async Task<Saga> CompensateSagaAsync(/* saga identifier */)
public async Task<Saga> CompensateSagaAsync(/* saga identifier and optional error context */)
```
Initiates backward recovery for a saga that has failed or been explicitly marked for compensation. Walks through previously completed steps in reverse order, invoking each step’s compensation action. The overload without explicit error context derives failure information from the saga’s current state. Both overloads return the `Saga` after compensation completes, with its status set to `Compensated` or `Failed` depending on whether all compensations succeeded. Throws if the saga is not in a compensable state, if a compensation handler is missing for a completed step, or if the persistence layer cannot record compensation progress.

### AbortSagaAsync
```csharp
public async Task AbortSagaAsync(/* saga identifier */)
```
Forcefully terminates a saga without running compensation logic. The saga is marked with an `Aborted` status and all pending steps are discarded. This is intended for scenarios where the saga is irrecoverable and compensation is either impossible or unnecessary. Does not return a value. Throws if the saga does not exist or is already in a terminal state that cannot be transitioned to `Aborted`.

### GetSagaAsync
```csharp
public async Task<Saga> GetSagaAsync(/* saga identifier */)
```
Retrieves the current state, step history, and payload of a single saga by its unique identifier. Returns the `Saga` object or `null` if no saga with that identifier exists. This method does not throw under normal circumstances; failures in the persistence layer surface as exceptions from the underlying data access components.

### ListSagasAsync
```csharp
public async Task<List<Saga>> ListSagasAsync(/* optional filter criteria */)
```
Queries the saga store for instances matching optional filter criteria such as status, creation date range, or saga type. Returns a `List<Saga>` containing zero or more matching sagas. The list is a snapshot of current state at the time of the query. Throws if the query parameters are malformed or if the persistence layer is unavailable.

## Usage

### Example 1: Creating and running a simple order fulfillment saga
```csharp
var sagaService = serviceProvider.GetRequiredService<SagaOrchestrationService>();

// Define the saga with steps: reserve inventory, charge payment, ship order
var orderSaga = await sagaService.CreateSagaAsync(new CreateSagaRequest
{
    SagaType = "OrderFulfillment",
    InitialData = new { OrderId = "ORD-12345", Amount = 150.00m }
});

// Start execution; the first step (reserve inventory) runs immediately
orderSaga = await sagaService.StartSagaAsync(orderSaga.Id);

// If the first step succeeded, subsequent steps are triggered
if (orderSaga.Status == SagaStatus.Running)
{
    var nextStep = await sagaService.ExecuteNextStepAsync(orderSaga.Id);
    // Continue until saga completes or fails
    while (nextStep.Status == StepStatus.Completed && !nextStep.IsFinal)
    {
        nextStep = await sagaService.ExecuteNextStepAsync(orderSaga.Id);
    }
}

// On failure, compensate
if (orderSaga.Status == SagaStatus.Failed)
{
    orderSaga = await sagaService.CompensateSagaAsync(orderSaga.Id);
}
```

### Example 2: Timeout-aware saga with explicit abort path
```csharp
var sagaService = serviceProvider.GetRequiredService<SagaOrchestrationService>();

var paymentSaga = await sagaService.CreateSagaAsync(new CreateSagaRequest
{
    SagaType = "PaymentAuthorization",
    InitialData = new { TransactionId = "TXN-98765" },
    StepTimeout = TimeSpan.FromSeconds(30)
});

paymentSaga = await sagaService.StartSagaAsync(paymentSaga.Id);

// A background process monitors timeouts and calls HandleTimeoutAsync
bool timeoutHandled = await sagaService.HandleTimeoutAsync(
    paymentSaga.Id,
    new TimeoutContext { StepIndex = 0, Elapsed = TimeSpan.FromSeconds(31) }
);

if (timeoutHandled)
{
    // Timeout triggered compensation automatically; verify final state
    paymentSaga = await sagaService.GetSagaAsync(paymentSaga.Id);
}
else
{
    // Timeout was stale; saga may have progressed, so check and possibly abort
    paymentSaga = await sagaService.GetSagaAsync(paymentSaga.Id);
    if (paymentSaga.Status == SagaStatus.Running)
    {
        await sagaService.AbortSagaAsync(paymentSaga.Id);
    }
}

// List all sagas of this type for reporting
var allPaymentSagas = await sagaService.ListSagasAsync(
    new SagaFilter { SagaType = "PaymentAuthorization" }
);
```

## Notes

- **State transitions are durable**: Every method that mutates saga state persists the change before returning. If the process crashes after a call returns, the saga’s recorded state reflects the operation.
- **Idempotency expectations**: `StartSagaAsync`, `ExecuteNextStepAsync`, and `CompensateSagaAsync` rely on the underlying step handlers being idempotent. The service itself does not deduplicate repeated calls for the same logical transition; callers must guard against duplicate triggers.
- **Timeout staleness**: `HandleTimeoutAsync` may return `false` if the saga or step has already progressed. Callers should treat a `false` return as a signal to re-read the saga state rather than assuming the timeout is still pending.
- **Compensation ordering**: `CompensateSagaAsync` compensates completed steps in strict reverse order. If a compensation handler itself fails, the saga is typically marked `Failed` and remaining compensations are not attempted, leaving the saga in a partially compensated state that requires manual intervention.
- **Abort vs. compensate**: `AbortSagaAsync` skips all compensation logic. Use it only when compensation is known to be unnecessary or impossible; otherwise prefer `CompensateSagaAsync` to maintain consistency across participating services.
- **Thread safety**: The service does not impose internal locks across calls for the same saga. Concurrent calls to `ExecuteNextStepAsync`, `HandleTimeoutAsync`, or `CompensateSagaAsync` for the same saga identifier can produce race conditions. Callers must serialize operations on a given saga instance externally, for example by using a distributed lock or single-threaded dispatcher per saga.
- **Null return from GetSagaAsync**: A `null` return indicates the saga does not exist. Callers should check for `null` before accessing members to avoid null-reference exceptions.
- **ListSagasAsync filtering**: The optional filter criteria are applied at the persistence layer. Passing `null` or an empty filter returns all sagas, which may be a large result set depending on store size. Consider always providing a bounded filter in production scenarios.
