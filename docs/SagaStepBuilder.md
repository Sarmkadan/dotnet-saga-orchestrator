# SagaStepBuilder
The `SagaStepBuilder` class is a crucial component of the dotnet-saga-orchestrator project, responsible for constructing and configuring individual steps within a saga. It provides a fluent API for defining the characteristics of a saga step, such as its order, compensation, timeout, retry policy, and metadata. By utilizing this builder, developers can create complex saga workflows with ease and precision.

## API
The `SagaStepBuilder` class offers the following public members:
* `Create`: A static method that initiates the construction of a new saga step.
* `WithOrder`: Specifies the order in which the saga step should be executed. Returns the `SagaStepBuilder` instance for chaining.
* `WithCompensation`: Configures the compensation behavior for the saga step. Returns the `SagaStepBuilder` instance for chaining.
* `WithTimeout`: Sets the timeout for the saga step. Returns the `SagaStepBuilder` instance for chaining.
* `WithRetryPolicy`: Defines the retry policy for the saga step. Returns the `SagaStepBuilder` instance for chaining. (Note: There are two overloads for this method, allowing for different retry policy configurations.)
* `WithMetadata`: Adds metadata to the saga step. Returns the `SagaStepBuilder` instance for chaining. (Note: There are two overloads for this method, allowing for different metadata configurations.)
* `WithCircuitBreakerThreshold`: Configures the circuit breaker threshold for the saga step. Returns the `SagaStepBuilder` instance for chaining.
* `Async`: Specifies that the saga step should be executed asynchronously. Returns the `SagaStepBuilder` instance for chaining.
* `Synchronous`: Specifies that the saga step should be executed synchronously. Returns the `SagaStepBuilder` instance for chaining.
* `Build`: Completes the construction of the saga step and returns a `SagaStepDefinition` instance.

## Usage
Here are two examples of using the `SagaStepBuilder` class to construct saga steps:
```csharp
// Example 1: Creating a simple saga step with a timeout
var step = SagaStepBuilder.Create()
    .WithOrder(1)
    .WithTimeout(TimeSpan.FromSeconds(30))
    .Build();

// Example 2: Creating a saga step with retry policy and metadata
var stepWithRetry = SagaStepBuilder.Create()
    .WithOrder(2)
    .WithRetryPolicy(3, TimeSpan.FromSeconds(1))
    .WithMetadata("Key", "Value")
    .Build();
```

## Notes
When using the `SagaStepBuilder` class, consider the following edge cases and thread-safety remarks:
* The `Build` method will throw an exception if the saga step is not properly configured (e.g., missing order or timeout).
* The `WithRetryPolicy` and `WithMetadata` methods have multiple overloads, allowing for different configurations. Be cautious when using these methods to avoid unintended behavior.
* The `SagaStepBuilder` class is designed to be thread-safe, allowing for concurrent construction of saga steps. However, the resulting `SagaStepDefinition` instances are not thread-safe and should be used carefully in multi-threaded environments.
* The `Async` and `Synchronous` methods are mutually exclusive, and using both will result in an exception being thrown. Choose the correct execution mode based on your saga workflow requirements.
