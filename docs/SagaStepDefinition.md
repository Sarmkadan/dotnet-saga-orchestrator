# SagaStepDefinition

Represents a single step within a saga orchestration workflow. Each step defines the action to invoke on a target service, its compensation behaviour, timeout constraints, retry policy, and metadata. The type provides validation logic and builder-style methods for configuring timeout and retry behaviour.

## API

### Properties

#### `Id`
`public string Id`

A unique identifier for the step. Used internally to correlate step execution with saga state and compensation tracking.

#### `Name`
`public string Name`

A human-readable name for the step. Typically used in logs, dashboards, and error messages.

#### `Description`
`public string Description`

An optional longer description of what the step does. May be surfaced in operational tooling or documentation generation.

#### `Order`
`public int Order`

The zero-based execution position of this step within the saga. Steps are executed sequentially in ascending order. Duplicate or non-contiguous values may cause validation failures.

#### `ServiceName`
`public string ServiceName`

The logical name of the service that handles this step’s request. Used for routing, logging, and service-discovery lookups.

#### `ServiceUrl`
`public string ServiceUrl`

The absolute or relative URL endpoint on the target service where the forward action is invoked. Required when `HttpMethod` is set.

#### `CompensationUrl`
`public string CompensationUrl`

The endpoint to call when compensating this step. Only meaningful when `IsCompensable` is `true`. If `IsCompensable` is `true` and this value is null or empty, validation will fail.

#### `TimeoutSeconds`
`public int TimeoutSeconds`

The maximum number of seconds to wait for the forward action to complete before considering it timed out. A value of zero or less means no timeout is enforced.

#### `MaxRetries`
`public int MaxRetries`

The maximum number of retry attempts for the forward action before the step is marked as failed. A value of zero means no retries are performed.

#### `RetryDelayMilliseconds`
`public int RetryDelayMilliseconds`

The fixed delay in milliseconds between retry attempts. Ignored if `RetryPolicy` is explicitly set to a non-null value that defines its own delay strategy.

#### `IsCompensable`
`public bool IsCompensable`

Indicates whether this step can be compensated. When `true`, a valid `CompensationUrl` must be provided. Steps that are not compensable cannot be rolled back; the saga may still attempt compensation of prior steps depending on the saga configuration.

#### `IsAsync`
`public bool IsAsync`

When `true`, the step invocation returns immediately and the saga expects a callback or polling mechanism to determine completion. Timeout and retry behaviour still apply to the overall resolution window.

#### `HttpMethod`
`public string HttpMethod`

The HTTP verb used to invoke `ServiceUrl`. Typical values are `"GET"`, `"POST"`, `"PUT"`, `"DELETE"`, or `"PATCH"`. Case-insensitive validation is applied.

#### `RetryPolicy`
`public RetryPolicy? RetryPolicy`

An optional explicit retry policy object. When set, it overrides `MaxRetries` and `RetryDelayMilliseconds` with its own strategy (e.g. exponential back-off, jitter). If `null`, the step falls back to the fixed-delay retry behaviour defined by `MaxRetries` and `RetryDelayMilliseconds`.

#### `Metadata`
`public Dictionary<string, string> Metadata`

A dictionary of arbitrary key-value pairs attached to the step. Used to carry contextual information such as tracing identifiers, feature flags, or custom routing hints. May be empty but is never null after construction.

### Constructors

#### `SagaStepDefinition()`
`public SagaStepDefinition()`

Default parameterless constructor. Initialises `Metadata` to an empty dictionary. All string properties default to `null` or empty, numeric properties to zero, and `IsCompensable` / `IsAsync` to `false`.

#### `SagaStepDefinition(string id, string name, int order, string serviceName, string serviceUrl, string httpMethod)`
`public SagaStepDefinition(string id, string name, int order, string serviceName, string serviceUrl, string httpMethod)`

Creates a step with the core required fields pre-populated. `Metadata` is initialised to an empty dictionary. `TimeoutSeconds`, `MaxRetries`, and `RetryDelayMilliseconds` default to zero. `IsCompensable` and `IsAsync` default to `false`.

**Parameters:**
- `id`: Assigned to `Id`. Must not be null or empty.
- `name`: Assigned to `Name`. Must not be null or empty.
- `order`: Assigned to `Order`.
- `serviceName`: Assigned to `ServiceName`. Must not be null or empty.
- `serviceUrl`: Assigned to `ServiceUrl`. Must not be null or empty.
- `httpMethod`: Assigned to `HttpMethod`. Must not be null or empty.

**Throws:** `ArgumentException` if any required string parameter is null or empty.

### Methods

#### `Validate()`
`public bool Validate()`

Performs a logical consistency check on the step definition. Returns `true` if the step is valid; otherwise `false`.

**Validation rules applied:**
- `Id`, `Name`, `ServiceName`, `ServiceUrl`, and `HttpMethod` must not be null or empty.
- If `IsCompensable` is `true`, `CompensationUrl` must not be null or empty.
- `Order` must be non-negative.
- `TimeoutSeconds` must be non-negative.
- `MaxRetries` must be non-negative.
- `RetryDelayMilliseconds` must be non-negative.
- `HttpMethod` must be a recognised HTTP verb (case-insensitive check against a predefined set).

**Returns:** `true` when all rules pass; `false` otherwise. Does not throw.

#### `SetTimeout(int timeoutSeconds)`
`public void SetTimeout(int timeoutSeconds)`

Sets the `TimeoutSeconds` property and returns the same `SagaStepDefinition` instance for fluent chaining.

**Parameters:**
- `timeoutSeconds`: The timeout duration in seconds. Must be zero or greater.

**Throws:** `ArgumentOutOfRangeException` if `timeoutSeconds` is negative.

**Returns:** The current instance.

#### `SetRetryPolicy(RetryPolicy policy)`
`public void SetRetryPolicy(RetryPolicy policy)`

Assigns an explicit `RetryPolicy` to the step and returns the same instance for fluent chaining. When a non-null policy is set, `MaxRetries` and `RetryDelayMilliseconds` are effectively ignored during execution.

**Parameters:**
- `policy`: The retry policy to use, or `null` to clear a previously set policy and revert to fixed-delay retries.

**Returns:** The current instance.

## Usage

### Example 1: Defining a synchronous compensable step with fixed-delay retries

```csharp
var step = new SagaStepDefinition(
    id: "reserve-inventory",
    name: "Reserve Inventory",
    order: 0,
    serviceName: "InventoryService",
    serviceUrl: "https://inventory.example.com/api/reserve",
    httpMethod: "POST"
)
{
    Description = "Reserves items in the warehouse for the order",
    CompensationUrl = "https://inventory.example.com/api/release",
    IsCompensable = true,
    TimeoutSeconds = 30,
    MaxRetries = 3,
    RetryDelayMilliseconds = 1000
};

bool isValid = step.Validate(); // true if all fields are correctly populated
```

### Example 2: Defining an asynchronous step with an exponential back-off retry policy

```csharp
var retryPolicy = new RetryPolicy(
    maxRetries: 5,
    strategy: RetryStrategy.ExponentialBackoff,
    baseDelayMs: 500,
    maxDelayMs: 10000
);

var step = new SagaStepDefinition()
    .SetTimeout(60)
    .SetRetryPolicy(retryPolicy);

step.Id = "process-payment";
step.Name = "Process Payment";
step.Order = 1;
step.ServiceName = "PaymentService";
step.ServiceUrl = "https://payment.example.com/api/charge";
step.HttpMethod = "POST";
step.IsAsync = true;
step.IsCompensable = true;
step.CompensationUrl = "https://payment.example.com/api/refund";
step.Metadata["correlation-id"] = Guid.NewGuid().ToString();

bool isValid = step.Validate();
```

## Notes

- **Validation is explicit.** `Validate()` must be called to detect misconfigured steps; the constructor and property setters do not enforce cross-field consistency beyond basic null checks in the parameterised constructor.
- **Retry policy precedence.** When `RetryPolicy` is non-null, the values of `MaxRetries` and `RetryDelayMilliseconds` are ignored at execution time. Clearing the policy by calling `SetRetryPolicy(null)` restores the fixed-delay behaviour.
- **Timeout and retry interaction.** If `TimeoutSeconds` is less than the total possible retry window (`MaxRetries * RetryDelayMilliseconds`), the timeout will fire before all retries are exhausted. Callers should ensure these values are coherent.
- **Compensation URL requirement.** Setting `IsCompensable = true` without a valid `CompensationUrl` causes `Validate()` to return `false`. The reverse is not enforced: a `CompensationUrl` may be present even when `IsCompensable` is `false`, though it will be ignored at runtime.
- **Thread safety.** This type is not designed for concurrent mutation. Properties and `Metadata` are not synchronised. If a step definition is shared across threads, external synchronisation is required.
- **Metadata dictionary.** The `Metadata` dictionary is always non-null after any constructor, but its contents are entirely caller-managed. No deep copy is made when the step is passed to the saga engine; mutations after registration may affect running orchestrations.
- **`SetTimeout` and `SetRetryPolicy` return void.** Despite returning the instance for chaining, the signature is `public void`. This is an intentional design choice in the codebase; callers can still chain because the returned reference is the same object.
