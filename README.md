# dotnet-saga-orchestrator

A .NET library for managing distributed sagas with compensating transactions, retries and timeout handling.

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the layer breakdown (Core / Application / Data / Infrastructure), the saga execution and compensation data flow, extension points, and the reasoning behind the bigger design decisions.

## SagaIdGenerator

The `SagaIdGenerator` class provides a set of utility methods for generating and validating unique identifiers used in saga workflows. These identifiers are essential for tracking sagas, steps, correlations, and requests.

## ISagaResponseMapper

The `ISagaResponseMapper` interface provides methods for converting saga domain models to response DTOs, enabling consistent API response formatting. It supports mapping individual sagas, collections of sagas, and individual saga steps to their corresponding response types.

### Usage Example

```csharp
using SagaOrchestrator.Application.Mappers;
using SagaOrchestrator.Application.DTOs;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Create a mapper instance
var mapper = new SagaResponseMapper();

// Example saga domain model
var saga = new Saga
{
    Id = "saga_abc123",
    CorrelationId = "corr_xyz789",
    Status = SagaStatus.Completed,
    Definition = new SagaDefinition { Id = "order_processing", Name = "Order Processing Saga" },
    StartedAt = DateTime.UtcNow.AddMinutes(-5),
    CompletedAt = DateTime.UtcNow,
    RetryCount = 0,
    Steps = new List<SagaStep>
    {
        new SagaStep
        {
            Id = "step_001",
            Name = "Validate Order",
            Order = 1,
            Status = SagaStepStatus.Completed,
            ServiceUrl = "https://order-service/api/validate",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow.AddMinutes(-4),
            RetryCount = 0,
            ErrorMessage = null
        },
        new SagaStep
        {
            Id = "step_002",
            Name = "Process Payment",
            Order = 2,
            Status = SagaStepStatus.Completed,
            ServiceUrl = "https://payment-service/api/charge",
            StartedAt = DateTime.UtcNow.AddMinutes(-4),
            CompletedAt = DateTime.UtcNow.AddMinutes(-3),
            RetryCount = 0,
            ErrorMessage = null
        }
    }
};

// Map single saga to response
SagaResponse response = mapper.MapToResponse(saga);
Console.WriteLine($"Saga ID: {response.Id}");
Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Steps: {response.Steps.Count}");

// Map collection of sagas to responses
var sagas = new List<Saga> { saga };
List<SagaResponse> responses = mapper.MapToResponses(sagas);

// Map individual saga step to response
SagaStep step = saga.Steps[0];
SagaStepResponse stepResponse = mapper.MapStepToResponse(step);
Console.WriteLine($"Step Name: {stepResponse.Name}");
Console.WriteLine($"Service: {stepResponse.ServiceName}");
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
