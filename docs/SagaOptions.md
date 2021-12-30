# SagaOptions

`SagaOptions` is a configuration container used by the saga orchestrator to define global behavior for timeouts, retries, caching, worker execution, and webhook handling. Instances of this type are typically passed to the orchestrator at startup or when registering a saga definition, allowing centralized tuning of performance and reliability characteristics without modifying individual saga steps.

## API

| Member | Type | Purpose | Remarks |
|--------|------|---------|---------|
| `TimeoutPolicies` | `TimeoutPolicies` | Gets or sets the collection of timeout‑related policies applied to saga steps and the overall saga. | The property never returns `null`; assigning `null` throws an `ArgumentNullException`. |
| `RetryPolicies` | `RetryPolicies` | Gets or sets the collection of retry policies that govern how failed steps are retried. | Assigning `null` throws an `ArgumentNullException`. |
| `CachePolicies` | `CachePolicies` | Gets or sets the collection of caching policies used for step results, saga definitions, and health‑check data. | Assigning `null` throws an `ArgumentNullException`. |
| `WorkerPolicies` | `WorkerPolicies` | Gets or sets the policies that control worker concurrency, queue depth, and execution limits. | Assigning `null` throws an `ArgumentNullException`. |
| `WebhookPolicies` | `WebhookPolicies` | Gets or sets the policies that dictate webhook delivery, verification, and failure handling. | Assigning `null` throws an `ArgumentNullException`. |
| `DefaultStepTimeoutSeconds` | `int` | Default timeout (in seconds) applied to any step that does not specify its own timeout. | Must be ≥ 1; setting a value ≤ 0 throws an `ArgumentOutOfRangeException`. |
| `DefaultSagaTimeoutSeconds` | `int` | Default timeout (in seconds) for the entire saga execution when no explicit saga timeout is defined. | Must be ≥ 1; setting a value ≤ 0 throws an `ArgumentOutOfRangeException`. |
| `MaxStepTimeoutSeconds` | `int` | Upper bound (in seconds) for any step timeout; values exceeding this limit are clamped to this maximum. | Must be ≥ `DefaultStepTimeoutSeconds`; otherwise throws an `ArgumentOutOfRangeException`. |
| `MaxSagaTimeoutSeconds` | `int` | Upper bound (in seconds) for any saga timeout; values exceeding this limit are clamped. | Must be ≥ `DefaultSagaTimeoutSeconds`; otherwise throws an `ArgumentOutOfRangeException`. |
| `CompensationTimeoutSeconds` | `int` | Timeout (in seconds) allowed for each compensation step during saga rollback. | Must be ≥ 1; setting a value ≤ 0 throws an `ArgumentOutOfRangeException`. |
| `DefaultMaxRetries` | `int` | Default number of retry attempts for a failed step when the step does not specify its own retry count. | Must be ≥ 0; negative values throw an `ArgumentOutOfRangeException`. |
| `DefaultRetryDelayMs` | `int` | Default delay (in milliseconds) between retry attempts when using fixed‑delay retry. | Must be ≥ 0; negative values throw an `ArgumentOutOfRangeException`. |
| `MaxRetries` | `int` | Global ceiling for retry attempts; any step requesting more retries than this value will be limited to `MaxRetries`. | Must be ≥ `DefaultMaxRetries`; otherwise throws an `ArgumentOutOfRangeException`. |
| `UseExponentialBackoff` | `bool` | When `true`, retry delays are calculated using an exponential backoff algorithm; otherwise fixed delay is used. | No validation required. |
| `BackoffMultiplier` | `double` | Factor by which the delay is multiplied on each retry attempt when `UseExponentialBackoff` is `true`. Must be > 0. | Setting a value ≤ 0 throws an `ArgumentOutOfRangeException`. |
| `MaxBackoffDelayMs` | `int` | Maximum delay (in milliseconds) allowed for exponential backoff; delays are capped at this value. | Must be ≥ `DefaultRetryDelayMs` when `UseExponentialBackoff` is `true`; otherwise throws an `ArgumentOutOfRangeException`. |
| `EnableCaching` | `bool` | Enables or disables caching of step results, saga definitions, and health‑check data across executions. | No validation required. |
| `SagaCacheExpirationMinutes` | `int` | Expiration time (in minutes) for cached saga state entries. | Must be ≥ 1 when `EnableCaching` is `true`; setting a value ≤ 0 throws an `ArgumentOutOfRangeException`. |
| `DefinitionCacheExpirationMinutes` | `int` | Expiration time (in minutes) for cached saga definition metadata. | Must be ≥ 1 when `EnableCaching` is `true`; setting a value ≤ 0 throws an `ArgumentOutOfRangeException`. |
| `HealthCheckCacheExpirationSeconds` | `int` | Expiration time (in seconds) for cached health‑check results. | Must be ≥ 1 when `EnableCaching` is `true`; setting a value ≤ 0 throws an `ArgumentOutOfRangeException`. |

## Usage

### Basic configuration

```csharp
using DotnetSagaOrchestrator.Configuration;

var options = new SagaOptions
{
    DefaultStepTimeoutSeconds = 30,
    DefaultSagaTimeoutSeconds = 300,
    DefaultMaxRetries = 3,
    DefaultRetryDelayMs = 500,
    UseExponentialBackoff = true,
    BackoffMultiplier = 2.0,
    MaxBackoffDelayMs = 5000,
    EnableCaching = true,
    SagaCacheExpirationMinutes = 10,
    DefinitionCacheExpirationMinutes = 60,
    HealthCheckCacheExpirationSeconds = 30
};

// Pass options to the orchestrator builder
var orchestrator = SagaOrchestratorBuilder.Create()
                                          .UseOptions(options)
                                          .Build();
```

### Advanced tuning with policy objects

```csharp
using DotnetSagaOrchestrator.Configuration;
using DotnetSagaOrchestrator.Policies;

var timeoutPolicies = new TimeoutPolicies
{
    // custom step‑level timeout overrides can be added here
};

var retryPolicies = new RetryPolicies
{
    // define specific retry strategies for certain step types
};

var cachePolicies = new CachePolicies
{
    // configure distributed cache providers, serialization, etc.
};

var options = new SagaOptions
{
    TimeoutPolicies   = timeoutPolicies,
    RetryPolicies     = retryPolicies,
    CachePolicies     = cachePolicies,
    WorkerPolicies    = new WorkerPolicies { MaxDegreeOfParallelism = 8 },
    WebhookPolicies   = new WebhookPolicies { MaxDeliveryAttempts = 5 },
    DefaultStepTimeoutSeconds = 45,
    MaxStepTimeoutSeconds     = 120,
    CompensationTimeoutSeconds = 60,
    MaxRetries                = 10,
    UseExponentialBackoff     = false,
    DefaultRetryDelayMsRetryDelayMs       = 1000,
    EnableCaching             = false
};

var orchestrator = SagaOrchestratorBuilder.Create()
                                          .UseOptions(options)
                                          .Build();
```

## Notes

- All property setters perform basic range validation and will throw `ArgumentOutOfRangeException` for values outside the allowed domain (e.g., negative timeouts or retry counts). Assigning `null` to any of the policy properties results in an `ArgumentNullException`.
- The class is **mutable**; however, once an instance has been supplied to the saga orchestrator and the orchestrator has started, changing the properties may lead to undefined behavior. It is recommended to treat `SagaOptions` as immutable after registration.
- The timeout and retry limits are **independent**: a step may be subject to both a timeout and a retry policy; if a step times out, the retry logic is not invoked unless the timeout itself is treated as a failure by the step implementation.
- When `EnableCaching` is `false`, the three cache‑expiration properties are ignored; setting them still requires valid positive values because the validation runs regardless of the flag.
- The `WorkerPolicies` and `WebhookPolicies` objects follow the same validation rules as defined in their respective types; invalid configurations will surface as exceptions when the orchestrator attempts to read them.
- Thread safety: reading the properties after construction is safe concurrent with multiple threads. Concurrent writes to the same `SagaOptions` instance are not synchronized; external synchronization is required if the object is modified after publication.
