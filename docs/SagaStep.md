# SagaStep

`SagaStep` represents a single step within a distributed saga orchestration. It encapsulates the metadata, execution state, payload, retry configuration, and compensation details required to reliably invoke a remote service operation and, if necessary, roll back that operation through a compensating call.

## API

### Properties

- **`public string Id`**  
  Unique identifier for this step instance. Set during construction and remains immutable for the lifetime of the step.

- **`public string SagaId`**  
  Identifier of the parent saga that owns this step. Used to correlate steps across a distributed transaction.

- **`public string Name`**  
  Human-readable name describing the operation this step performs (e.g., `"ReserveInventory"`).

- **`public int Order`**  
  Zero-based execution order within the saga. Steps are typically processed sequentially in ascending `Order`.

- **`public SagaStepStatus Status`**  
  Current lifecycle status of the step. Values follow the `SagaStepStatus` enumeration (e.g., `Pending`, `Running`, `Completed`, `Compensating`, `Compensated`, `Failed`).

- **`public string ServiceUrl`**  
  The endpoint URL invoked when the step executes its forward action.

- **`public string CompensationUrl`**  
  The endpoint URL invoked to undo the forward action during compensation. May be empty if the step is not compensatable.

- **`public Dictionary<string, object> Payload`**  
  Arbitrary key-value data sent as the request body to `ServiceUrl` when the step starts. Populated before calling `Start()`.

- **`public Dictionary<string, object> Response`**  
  Deserialized response data received from a successful invocation of `ServiceUrl`. Remains `null` until the step completes successfully.

- **`public DateTime? StartedAt`**  
  UTC timestamp recorded when the step transitions to `Running`. `null` if the step has not yet started.

- **`public DateTime? CompletedAt`**  
  UTC timestamp recorded when the step transitions to `Completed`. `null` if the step has not finished successfully.

- **`public DateTime? CompensatedAt`**  
  UTC timestamp recorded when the step transitions to `Compensated`. `null` if compensation has not occurred.

- **`public string? ErrorMessage`**  
  Holds the last error message if the step fails or compensation fails. Cleared or `null` when the step is in a healthy state.

- **`public int RetryCount`**  
  Number of retry attempts performed so far for the current invocation. Reset when the step transitions out of a retryable failure state.

- **`public int MaxRetries`**  
  Maximum number of retry attempts allowed before the step is marked as permanently failed.

- **`public int TimeoutSeconds`**  
  HTTP call timeout in seconds applied to both forward and compensation requests.

- **`public RetryPolicy? RetryPolicy`**  
  Optional retry strategy defining backoff intervals and transient error detection. When `null`, a default linear backoff may be applied internally.

### Constructors

- **`public SagaStep`**  
  Default parameterless constructor. Creates an empty step with `Status` set to `Pending`. All collection properties are initialized to empty dictionaries; timestamps and `ErrorMessage` are `null`.

### Methods

- **`public void Initialize`**  
  Validates and prepares the step for execution. Throws `InvalidOperationException` if required fields (`Id`, `SagaId`, `ServiceUrl`) are not set. Must be called before `Start()`.

- **`public void Start`**  
  Begins execution of the step by sending the `Payload` to `ServiceUrl`. Transitions `Status` to `Running` and records `StartedAt`. Throws `InvalidOperationException` if `Initialize` has not been called or the step is not in a startable state (`Pending` or a retryable failure state). Execution honors `RetryPolicy`, `MaxRetries`, and `TimeoutSeconds`.

## Usage

### Example 1: Basic step with compensation

```csharp
var step = new SagaStep
{
    Id = Guid.NewGuid().ToString(),
    SagaId = "saga-12345",
    Name = "ChargeCreditCard",
    Order = 2,
    ServiceUrl = "https://payments.example.com/api/charge",
    CompensationUrl = "https://payments.example.com/api/refund",
    MaxRetries = 3,
    TimeoutSeconds = 30,
    Payload = new Dictionary<string, object>
    {
        ["cardToken"] = "tok_abc123",
        ["amount"] = 99.95m,
        ["currency"] = "USD"
    }
};

step.Initialize();
step.Start();
```

### Example 2: Step with custom retry policy and no compensation

```csharp
var step = new SagaStep
{
    Id = "step-notify",
    SagaId = "saga-67890",
    Name = "SendEmail",
    Order = 4,
    ServiceUrl = "https://notifications.example.com/send",
    CompensationUrl = null, // non-compensatable action
    MaxRetries = 5,
    TimeoutSeconds = 10,
    RetryPolicy = new RetryPolicy
    {
        Intervals = new[] { 1, 5, 15, 60 },
        RetryOn = ex => ex is HttpRequestException or TaskCanceledException
    },
    Payload = new Dictionary<string, object>
    {
        ["templateId"] = "order-confirmation",
        ["recipient"] = "user@example.com"
    }
};

step.Initialize();
step.Start();
```

## Notes

- **Initialization requirement**: Calling `Start()` without prior `Initialize()` throws `InvalidOperationException`. `Initialize` performs validation synchronously and does not make network calls.
- **State machine constraints**: `Start()` can only be called when `Status` is `Pending` or when a previous attempt failed but retries remain. Calling it on an already `Completed` or `Compensated` step throws.
- **Thread safety**: Instance members are not synchronized. Concurrent calls to `Start()` or concurrent mutations of `Payload`/`Response` from multiple threads may cause race conditions. External locking is required if a step is shared across threads.
- **Compensation triggering**: This type does not autonomously decide to compensate. An external orchestrator reads `Status` and invokes the compensation endpoint using `CompensationUrl` and the original `Payload`. The step records compensation outcome via `CompensatedAt` and `ErrorMessage`.
- **Retry and timeout interaction**: If a request exceeds `TimeoutSeconds`, it is treated as a failure and counts toward `RetryCount`. The `RetryPolicy` determines whether the failure is retryable and the delay before the next attempt.
- **Payload immutability during execution**: Modifying `Payload` after `Start()` is called but before the step completes leads to undefined behaviour; the in-flight request may use either the original or mutated data depending on internal implementation details.
