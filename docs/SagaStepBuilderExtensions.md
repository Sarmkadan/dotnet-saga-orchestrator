# SagaStepBuilderExtensions

`SagaStepBuilderExtensions` provides a set of fluent extension methods for `SagaStepBuilder`.  
These methods enable the configuration of a saga step’s descriptive metadata, HTTP interaction details, compensation behavior, retry policies, and arbitrary key‑value metadata without modifying the original builder class.

## API

### `WithDescription(this SagaStepBuilder builder, string description)`

*Purpose* – Sets a human‑readable description for the step.  
*Parameters*  
- `builder` – The `SagaStepBuilder` instance to configure. Must not be `null`.  
- `description` – The description text. Must not be `null` or empty.  
*Return value* – The same `SagaStepBuilder` instance, allowing further chaining.  
*Exceptions* – Throws `ArgumentNullException` if `builder` is `null`; throws `ArgumentException` if `description` is `null` or whitespace.

### `WithHttpMethod(this SagaStepBuilder builder, HttpMethod method)`

*Purpose* – Specifies the HTTP method that the step will use when invoking an external service.  
*Parameters*  
- `builder` – The target `SagaStepBuilder`. Must not be `null`.  
- `method` – An `HttpMethod` value (e.g., `HttpMethod.Get`). Must not be `null`.  
*Return value* – The configured `SagaStepBuilder`.  
*Exceptions* – Throws `ArgumentNullException` when either argument is `null`.

### `WithCompensable(this SagaStepBuilder builder, bool compensable = true)`

*Purpose* – Marks the step as compensable (or not) for rollback scenarios.  
*Parameters*  
- `builder` – The `SagaStepBuilder` to modify. Must not be `null`.  
- `compensable` – `true` to enable compensation; `false` to disable. Defaults to `true`.  
*Return value* – The same `SagaStepBuilder` instance.  
*Exceptions* – Throws `ArgumentNullException` if `builder` is `null`.

### `WithRetryPolicyFromDefinition(this SagaStepBuilder builder, RetryPolicy policy)`

*Purpose* – Applies an existing `RetryPolicy` object to the step.  
*Parameters*  
- `builder` – The `SagaStepBuilder` being extended. Must not be `null`.  
- `policy` – A pre‑configured `RetryPolicy`. Must not be `null`.  
*Return value* – The modified `SagaStepBuilder`.  
*Exceptions* – Throws `ArgumentNullException` if either argument is `null`.

### `WithExponentialRetryPolicy(this SagaStepBuilder builder, int maxRetries, TimeSpan baseDelay)`

*Purpose* – Configures an exponential back‑off retry strategy.  
*Parameters*  
- `builder` – The target `SagaStepBuilder`. Must not be `null`.  
- `maxRetries` – Maximum number of retry attempts; must be non‑negative.  
- `baseDelay` – The initial delay before the first retry; must be non‑negative.  
*Return value* – The same `SagaStepBuilder` for further chaining.  
*Exceptions* – Throws `ArgumentNullException` if `builder` is `null`; throws `ArgumentOutOfRangeException` if `maxRetries` < 0 or `baseDelay` < `TimeSpan.Zero`.

### `WithLinearRetryPolicy(this SagaStepBuilder builder, int maxRetries, TimeSpan delay)`

*Purpose* – Configures a linear retry strategy with a fixed delay between attempts.  
*Parameters*  
- `builder` – The `SagaStepBuilder` to configure. Must not be `null`.  
- `maxRetries` – Number of retry attempts; must be non‑negative.  
- `delay` – Fixed delay between retries; must be non‑negative.  
*Return value* – The same builder instance.  
*Exceptions* – Throws `ArgumentNullException` if `builder` is `null`; throws `ArgumentOutOfRangeException` for negative `maxRetries` or `delay`.

### `WithNoRetryPolicy(this SagaStepBuilder builder)`

*Purpose* – Disables any retry behavior for the step.  
*Parameters*  
- `builder` – The `SagaStepBuilder` to modify. Must not be `null`.  
*Return value* – The builder instance, now configured with a no‑retry policy.  
*Exceptions* – Throws `ArgumentNullException` if `builder` is `null`.

### `WithMetadata(this SagaStepBuilder builder, string key, string value)`

*Purpose* – Adds a single metadata entry to the step.  
*Parameters*  
- `builder` – The `SagaStepBuilder` being extended. Must not be `null`.  
- `key` – Metadata key; must not be `null` or empty.  
- `value` – Metadata value; may be `null`.  
*Return value* – The same `SagaStepBuilder` instance.  
*Exceptions* – Throws `ArgumentNullException` if `builder` or `key` is `null`; throws `ArgumentException` if `key` is empty or whitespace.

## Usage

