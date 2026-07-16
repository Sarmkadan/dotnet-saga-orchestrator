# dotnet-saga-orchestrator

A .NET library for managing distributed sagas with compensating transactions, retries and timeout handling.

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the layer breakdown (Core / Application / Data / Infrastructure), the saga execution and compensation data flow, extension points, and the reasoning behind the bigger design decisions.

## SagaIdGenerator

The `SagaIdGenerator` class provides a set of utility methods for generating and validating unique identifiers used in saga workflows. These identifiers are essential for tracking sagas, steps, correlations, and requests.

## ISagaResponseMapper

The `ISagaResponseMapper` interface provides methods for converting saga domain models to response DTOs, enabling consistent API response formatting. It supports mapping individual sagas, collections of sagas, and individual saga steps to their corresponding response types.

## SagaResponse

The `SagaResponse` class is a response model that represents the state and metadata of a saga execution. It provides a structured view of saga operations including identification, timing, status tracking, and step-level details for API consumers. The response includes aggregated metrics like step counts, retry counts, and timing information to give a complete picture of saga progress and completion status.

### Usage Example

```csharp
using SagaOrchestrator.Application.DTOs;

// Create a completed saga response
var response = new SagaResponse
{
    Id = "saga_abc123",
    CorrelationId = "corr_xyz789",
    Status = "Completed",
    DefinitionId = "order_processing",
    DefinitionName = "Order Processing Saga",
    StartedAt = DateTime.UtcNow.AddMinutes(-5),
    CompletedAt = DateTime.UtcNow,
    FailureReason = null,
    StepCount = 3,
    CompletedSteps = 3,
    FailedSteps = 0,
    RetryCount = 0,
    Steps = new List<SagaStepResponse>
    {
        new SagaStepResponse
        {
            Id = "step_001",
            Name = "Validate Order",
            Order = 1,
            Status = "Completed",
            ServiceName = "https://order-service/api/validate",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow.AddMinutes(-4),
            ErrorMessage = null,
            RetryCount = 0
        },
        new SagaStepResponse
        {
            Id = "step_002",
            Name = "Process Payment",
            Order = 2,
            Status = "Completed",
            ServiceName = "https://payment-service/api/charge",
            StartedAt = DateTime.UtcNow.AddMinutes(-4),
            CompletedAt = DateTime.UtcNow.AddMinutes(-3),
            ErrorMessage = null,
            RetryCount = 0
        },
        new SagaStepResponse
        {
            Id = "step_003",
            Name = "Ship Order",
            Order = 3,
            Status = "Completed",
            ServiceName = "https://shipping-service/api/ship",
            StartedAt = DateTime.UtcNow.AddMinutes(-3),
            CompletedAt = DateTime.UtcNow.AddMinutes(-2),
            ErrorMessage = null,
            RetryCount = 0
        }
    }
};

Console.WriteLine($"Saga ID: {response.Id}");
Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Definition: {response.DefinitionName}");
Console.WriteLine($"Progress: {response.CompletedSteps}/{response.StepCount} steps completed");
Console.WriteLine($"Duration: {(response.CompletedAt - response.StartedAt)?.TotalSeconds ?? 0} seconds");

// Create a failed saga response with retry information
var failedResponse = new SagaResponse
{
    Id = "saga_def456",
    CorrelationId = "corr_uvw123",
    Status = "Failed",
    DefinitionId = "payment_processing",
    DefinitionName = "Payment Processing Saga",
    StartedAt = DateTime.UtcNow.AddMinutes(-10),
    CompletedAt = DateTime.UtcNow,
    FailureReason = "Payment gateway timeout",
    StepCount = 2,
    CompletedSteps = 1,
    FailedSteps = 1,
    RetryCount = 2,
    Steps = new List<SagaStepResponse>
    {
        new SagaStepResponse
        {
            Id = "step_001",
            Name = "Validate Payment Method",
            Order = 1,
            Status = "Completed",
            ServiceName = "https://payment-service/api/validate",
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow.AddMinutes(-9),
            ErrorMessage = null,
            RetryCount = 0
        },
        new SagaStepResponse
        {
            Id = "step_002",
            Name = "Charge Payment",
            Order = 2,
            Status = "Failed",
            ServiceName = "https://payment-service/api/charge",
            StartedAt = DateTime.UtcNow.AddMinutes(-9),
            CompletedAt = null,
            ErrorMessage = "Payment gateway timeout after 30 seconds",
            RetryCount = 2
        }
    }
};
```

### Usage Example

```csharp
using SagaOrchestrator.Core.Utilities;

var sagaId = SagaIdGenerator.GenerateSagaId();
var correlationId = SagaIdGenerator.GenerateCorrelationId();
var stepId = SagaIdGenerator.GenerateStepId();
var traceId = SagaIdGenerator.GenerateTraceId();
var requestId = SagaIdGenerator.GenerateRequestId();

Console.WriteLine($"Saga ID: {sagaId}"); // e.g. saga_xxxxxxxxxxxx
Console.WriteLine($"Correlation ID: {correlationId}"); // e.g. corr_xxxxxxxxxxxx or xxxxxxxxxxxx
Console.WriteLine($"Step ID: {stepId}"); // e.g. step_xxxxxxxxxxxx
Console.WriteLine($"Trace ID: {traceId}"); // e.g. trace_xxxxxxxxxxxx
Console.WriteLine($"Request ID: {requestId}"); // e.g. req_xxxxxxxx_xxxx

bool isValidSaga = SagaIdGenerator.IsValidSagaId(sagaId);
bool isValidCorrelation = SagaIdGenerator.IsValidCorrelationId(correlationId);

Console.WriteLine($"Is valid saga ID: {isValidSaga}"); // True
Console.WriteLine($"Is valid correlation ID: {isValidCorrelation}"); // True
```

## TimeoutPolicy

The `TimeoutPolicy` class encapsulates timeout configuration for sagas and saga steps. It provides methods for checking timeout conditions, calculating remaining time, and determining if sufficient time remains for operations. Timeout policies can be created using predefined factory methods (`CreateLenient`, `CreateStandard`, `CreateStrict`) or customized with a specific timeout duration.

### Usage Example

```csharp
using SagaOrchestrator.Core.Utilities;

// Create a standard timeout policy (1 minute)
var standardTimeout = TimeoutPolicy.CreateStandard();
Console.WriteLine($"Standard timeout: {standardTimeout.TimeoutSeconds} seconds"); // 60
Console.WriteLine($"Is relaxed: {standardTimeout.IsRelaxed}"); // False

// Create a lenient timeout policy (5 minutes)
var lenientTimeout = TimeoutPolicy.CreateLenient();
Console.WriteLine($"Lenient timeout: {lenientTimeout.TimeoutSeconds} seconds"); // 300
Console.WriteLine($"Is relaxed: {lenientTimeout.IsRelaxed}"); // True

// Create a strict timeout policy (10 seconds)
var strictTimeout = TimeoutPolicy.CreateStrict();
Console.WriteLine($"Strict timeout: {strictTimeout.TimeoutSeconds} seconds"); // 10

// Create a custom timeout policy (2 minutes)
var customTimeout = TimeoutPolicy.Create(120);
Console.WriteLine($"Custom timeout: {customTimeout.TimeoutSeconds} seconds"); // 120

// Check if elapsed time has exceeded timeout
var startTime = DateTime.UtcNow;
var elapsed = TimeSpan.FromSeconds(45);
bool hasExceeded = customTimeout.HasExceeded(elapsed);
Console.WriteLine($"Has exceeded: {hasExceeded}"); // False

// Check if enough time remains with buffer
var buffer = TimeSpan.FromSeconds(10);
bool hasSufficientTime = customTimeout.HasSufficientTime(startTime, TimeSpan.FromSeconds(30));
Console.WriteLine($"Has sufficient time: {hasSufficientTime}"); // True

// Get remaining time
TimeSpan remaining = customTimeout.GetRemainingTime(startTime);
Console.WriteLine($"Remaining time: {remaining.TotalSeconds} seconds");

// Get elapsed percentage
double percentage = customTimeout.GetElapsedPercentage(startTime);
Console.WriteLine($"Elapsed percentage: {percentage:F2}%");
```
