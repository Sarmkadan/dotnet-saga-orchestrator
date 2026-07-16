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

## SagaCommandResult

The `SagaCommandResult` class is a standardized response DTO for all saga operations. It provides a consistent response format with success status, message, data payload, error collection, and metadata. This type is used throughout the application to return operation results from command handlers, services, and API endpoints.

### Usage Example

```csharp
using SagaOrchestrator.Application.DTOs;

// Create a successful command result with data
var successResult = SagaCommandResult.SuccessResult(
    "Order processed successfully",
    new { OrderId = "ord_12345", Status = "Completed" }
);

Console.WriteLine($"Success: {successResult.Success}");
Console.WriteLine($"Message: {successResult.Message}");
Console.WriteLine($"Data: {successResult.Data}");
Console.WriteLine($"Request ID: {successResult.RequestId}");
Console.WriteLine($"Timestamp: {successResult.Timestamp}");
Console.WriteLine($"Errors: {string.Join(", ", successResult.Errors)}");

// Create a failed command result with multiple errors
var failureResult = SagaCommandResult.FailureResult(
    "Failed to process payment",
    "Insufficient funds",
    "Payment gateway unavailable"
);

Console.WriteLine($"Success: {failureResult.Success}");
Console.WriteLine($"Message: {failureResult.Message}");
Console.WriteLine($"Errors: {string.Join(", ", failureResult.Errors)}");

// Create an exception command result
try
{
    // Some operation that might throw
}
catch (Exception ex)
{
    var exceptionResult = SagaCommandResult.ExceptionResult(ex);
    Console.WriteLine($"Exception captured: {exceptionResult.Message}");
    Console.WriteLine($"Error details: {exceptionResult.Errors[0]}");
}

// Generic version example
var genericSuccess = SagaCommandResult<string>.SuccessResult(
    "payment_processing_complete",
    "Payment processed successfully"
);

Console.WriteLine($"Generic Success: {genericSuccess.Success}");
Console.WriteLine($"Generic Data: {genericSuccess.Data}");
```

## CreateSagaRequest

The `CreateSagaRequest` class is a request model for creating a new saga instance. It allows you to specify the saga definition, configure retry behavior, set timeout constraints, and attach metadata or initial payload data. The request can be validated using the `IsValid()` method to ensure required fields are present and values are within acceptable ranges.

### Usage Example

```csharp
using SagaOrchestrator.Application.DTOs;

// Create a request to start a new order processing saga
var request = new CreateSagaRequest
{
    DefinitionId = "order_processing_v2",
    DefinitionName = "Order Processing Saga",
    MaxRetries = 3,
    TimeoutSeconds = 300,
    Metadata = new Dictionary<string, object>
    {
        { "customerId", "cust_12345" },
        { "orderId", "ord_67890" },
        { "priority", "high" }
    },
    Data = "{\"orderTotal\": 99.99, \"items\": [{\"sku\": \"WIDGET-001\", \"quantity\": 2}]}"
};

// Validate the request before sending
if (request.IsValid())
{
    Console.WriteLine("Saga request is valid");
    Console.WriteLine($"Definition: {request.DefinitionId}");
    Console.WriteLine($"Max Retries: {request.MaxRetries ?? 0}");
    Console.WriteLine($"Timeout: {request.TimeoutSeconds ?? 0} seconds");
    Console.WriteLine($"Metadata Count: {request.Metadata?.Count ?? 0}");
}

// Create a minimal request using only DefinitionId
var minimalRequest = new CreateSagaRequest
{
    DefinitionId = "simple_saga"
};

// Create a request with just DefinitionName (alternative to DefinitionId)
var namedRequest = new CreateSagaRequest
{
    DefinitionName = "Payment Processing",
    MaxRetries = 5,
    TimeoutSeconds = 600
};

// Create a request without optional fields
var basicRequest = new CreateSagaRequest
{
    DefinitionId = "inventory_update"
};
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

## CompensationService

The `CompensationService` handles the execution of compensating transactions when a saga fails. It manages the entire compensation workflow including initiating compensation, executing compensation steps in the appropriate order, retrying failed compensations, and checking for timeouts. The service supports different compensation strategies (reverse order, forward order, parallel) and automatically handles saga state transitions throughout the compensation process.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the compensation service with required dependencies
var compensationService = new CompensationService(
    compensationRepository,
    sagaRepository,
    stepRepository
);

// Example 1: Begin compensation for a failed saga
var failedSaga = await sagaRepository.GetByIdAsync("saga_abc123");
if (failedSaga.Status == SagaStatus.Failed)
{
    await compensationService.BeginCompensationAsync(failedSaga);
    
    // This creates compensation transactions for all completed steps
    var compensations = await compensationService.GetCompensationsAsync("saga_abc123");
    Console.WriteLine($"Created {compensations.Count} compensation transactions");
}

// Example 2: Execute the next compensation step
var nextCompensation = await compensationService.ExecuteNextCompensationAsync("saga_abc123");
if (nextCompensation != null)
{
    Console.WriteLine($"Executing compensation for step {nextCompensation.StepName} (order {nextCompensation.Order})");
    Console.WriteLine($"Status: {nextCompensation.Status}");
}
else
{
    Console.WriteLine("All compensations completed successfully");
}

// Example 3: Retry a failed compensation
var retrySuccess = await compensationService.RetryCompensationAsync("comp_789xyz");
if (retrySuccess)
{
    Console.WriteLine("Compensation marked for retry");
}

// Example 4: Check for timed out compensations
var timedOut = await compensationService.CheckTimeoutsAsync("saga_abc123");
if (timedOut.Count > 0)
{
    Console.WriteLine($"Found {timedOut.Count} timed out compensations");
    foreach (var timeout in timedOut)
    {
        Console.WriteLine($"- Compensation {timeout.Id} timed out: {timeout.ErrorMessage}");
    }
}

// Example 5: Get all compensations for a saga
var allCompensations = await compensationService.GetCompensationsAsync("saga_abc123");
foreach (var compensation in allCompensations)
{
    Console.WriteLine($"Compensation {compensation.Id}: {compensation.StepName} - {compensation.Status}");
}
```

## ISagaVisualizationService

The `ISagaVisualizationService` provides real-time visualization snapshots and streaming state updates for saga execution monitoring. It enables tracking saga progress, visualizing step execution graphs, and monitoring live state changes through polling-based streaming.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;
using System.Text.Json;

// Initialize the visualization service with required dependencies
var visualizationService = new SagaVisualizationService(
    sagaRepository,
    logger
);

// Example 1: Get a single saga snapshot
var snapshot = await visualizationService.GetSnapshotAsync("saga_abc123");
Console.WriteLine($"Saga: {snapshot.SagaName} ({snapshot.SagaId})");
Console.WriteLine($"Status: {snapshot.Status} ({snapshot.ProgressPercent:F1}% complete)");
Console.WriteLine($"Duration: {snapshot.ElapsedMs:F0}ms");
Console.WriteLine($"Steps: {snapshot.CompletedSteps}/{snapshot.TotalSteps}");
if (snapshot.FailureReason != null)
{
    Console.WriteLine($"Failed: {snapshot.FailureReason}");
}

// Example 2: Get all saga snapshots for dashboard display
var allSnapshots = await visualizationService.GetAllSnapshotsAsync();
Console.WriteLine($"Total sagas: {allSnapshots.Count}");
foreach (var s in allSnapshots.OrderByDescending(s => s.ElapsedMs))
{
    Console.WriteLine($"- {s.SagaName}: {s.Status} ({s.ProgressPercent:F1}%)");
}

// Example 3: Stream live state updates with custom polling interval
var cts = new CancellationTokenSource();
_ = Task.Run(async () =>
{
    await visualizationService.StreamLiveStateAsync(
        "saga_abc123",
        async snapshot =>
        {
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"Live Update: {DateTime.UtcNow:HH:mm:ss.fff}");
            Console.WriteLine(json);
            Console.WriteLine(new string('-', 60));
            
            if (snapshot.IsTerminal)
            {
                Console.WriteLine("Saga reached terminal state!");
                cts.Cancel();
            }
        },
        TimeSpan.FromSeconds(1),
        cts.Token
    );
});

// Run for 10 seconds then stop
await Task.Delay(TimeSpan.FromSeconds(10));
cts.Cancel();

// Example 4: Access step details from snapshot
var stepSnapshot = await visualizationService.GetSnapshotAsync("saga_xyz789");
foreach (var node in stepSnapshot.Nodes)
{
    Console.WriteLine($"Step {node.Index}: {node.Name} - {node.Status}");
    if (node.DurationMs.HasValue)
    {
        Console.WriteLine($"  Duration: {node.DurationMs.Value:F0}ms");
    }
    if (node.ErrorMessage != null)
    {
        Console.WriteLine($"  Error: {node.ErrorMessage}");
    }
}

// Example 5: Monitor progress over time
var progressSnapshots = new List<SagaVisualizationSnapshot>();
await visualizationService.StreamLiveStateAsync(
    "saga_progress_test",
    snapshot =>
    {
        progressSnapshots.Add(snapshot);
        return Task.CompletedTask;
    },
    TimeSpan.FromSeconds(2)
);

Console.WriteLine($"Captured {progressSnapshots.Count} snapshots");
Console.WriteLine($"Final status: {progressSnapshots.Last().Status}");
Console.WriteLine($"Final progress: {progressSnapshots.Last().ProgressPercent:F1}%");
```

## SagaDefinitionService

The `SagaDefinitionService` manages saga workflow definitions, enabling creation, modification, validation, and versioning of saga processes. It handles the lifecycle of saga definitions from creation through activation, including adding/removing steps, cloning for versioning, and comprehensive validation. The service ensures saga definitions are properly structured before being used to execute sagas.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the saga definition service
var sagaDefinitionService = new SagaDefinitionService(sagaDefinitionRepository);

// Example 1: Create a new saga definition
var definition = await sagaDefinitionService.CreateDefinitionAsync(
    "Order Processing Saga",
    "Handles the complete order processing workflow from validation to shipping"
);
Console.WriteLine($"Created definition: {definition.Name} (v{definition.Version})");

// Example 2: Add steps to the definition
var step1 = new SagaStepDefinition(
    "Validate Order",
    "https://order-service/api/validate",
    HttpMethod.Get,
    "Validate customer order details and inventory availability"
);
var step2 = new SagaStepDefinition(
    "Process Payment",
    "https://payment-service/api/charge",
    HttpMethod.Post,
    "Charge customer payment method"
);
var step3 = new SagaStepDefinition(
    "Ship Order",
    "https://shipping-service/api/ship",
    HttpMethod.Post,
    "Create shipping label and schedule delivery"
);

var updatedDefinition = await sagaDefinitionService.AddStepAsync(definition.Id, step1);
updatedDefinition = await sagaDefinitionService.AddStepAsync(definition.Id, step2);
updatedDefinition = await sagaDefinitionService.AddStepAsync(definition.Id, step3);

// Example 3: Validate the definition
var validationResult = sagaDefinitionService.ValidateDefinition(updatedDefinition);
if (!validationResult.IsValid)
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Definition is valid and ready for activation");
}

// Example 4: Activate the definition for use
var activatedDefinition = await sagaDefinitionService.ActivateDefinitionAsync(definition.Id);
Console.WriteLine($"Definition activated: {activatedDefinition.IsActive}");

// Example 5: List all active definitions
var activeDefinitions = await sagaDefinitionService.ListDefinitionsAsync(activeOnly: true);
Console.WriteLine($"Found {activeDefinitions.Count} active definitions");

// Example 6: Get a definition by name
var namedDefinition = await sagaDefinitionService.GetDefinitionByNameAsync("Order Processing Saga");
if (namedDefinition != null)
{
    Console.WriteLine($"Found definition: {namedDefinition.Name} (ID: {namedDefinition.Id})");
}

// Example 7: Clone a definition for versioning
var clonedDefinition = await sagaDefinitionService.CloneDefinitionAsync(definition.Id);
Console.WriteLine($"Cloned definition: {clonedDefinition.Name} v{clonedDefinition.Version}");

// Example 8: Remove a step
var definitionWithoutStep = await sagaDefinitionService.RemoveStepAsync(definition.Id, "Process Payment");
Console.WriteLine($"Removed step, definition now has {definitionWithoutStep.Steps.Count} steps");

// Example 9: Deactivate a definition
var deactivatedDefinition = await sagaDefinitionService.DeactivateDefinitionAsync(definition.Id);
Console.WriteLine($"Definition deactivated: {deactivatedDefinition.IsActive}");
```

## SagaOrchestrationService

The `SagaOrchestrationService` is the main service responsible for managing saga execution workflows. It handles creating sagas, executing saga steps in sequence, handling retries and timeouts, and managing compensation workflows when sagas fail. The service coordinates between the saga definition, individual steps, and the compensation service to ensure reliable distributed transaction execution.

### Usage Example

```csharp
using SagaOrchestrationService = SagaOrchestrator.Application.Services.SagaOrchestrationService;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the saga orchestration service with required dependencies
var sagaOrchestrationService = new SagaOrchestrationService(
    sagaRepository,
    sagaStepRepository,
    compensationService,
    sagaLogger
);

// Example 1: Create a new saga instance
var definition = new SagaDefinition(
    "Order Processing Saga",
    "Handles complete order processing workflow"
);

// Add steps to the definition
var step1 = new SagaStepDefinition(
    "Validate Order",
    "https://order-service/api/validate",
    HttpMethod.Get,
    "Validate customer order details"
);
var step2 = new SagaStepDefinition(
    "Process Payment",
    "https://payment-service/api/charge",
    HttpMethod.Post,
    "Charge customer payment method"
);
var step3 = new SagaStepDefinition(
    "Ship Order",
    "https://shipping-service/api/ship",
    HttpMethod.Post,
    "Create shipping label and schedule delivery"
);
definition.AddStep(step1);
definition.AddStep(step2);
definition.AddStep(step3);

// Create the saga
var saga = await sagaOrchestrationService.CreateSagaAsync(
    definition,
    maxRetries: 3,
    timeoutSeconds: 300
);
Console.WriteLine($"Created saga: {saga.Id} with {saga.Steps.Count} steps");

// Example 2: Start the saga execution
var startedSaga = await sagaOrchestrationService.StartSagaAsync(saga.Id);
Console.WriteLine($"Saga started: {startedSaga.Status}");

// Example 3: Execute the next step (typically called by a background worker)
var executedStep = await sagaOrchestrationService.ExecuteNextStepAsync(saga.Id);
if (executedStep != null)
{
    Console.WriteLine($"Executed step {executedStep.Name} (order {executedStep.Order})");
    Console.WriteLine($"Step status: {executedStep.Status}");
}
else
{
    Console.WriteLine("All steps completed successfully");
}

// Example 4: Handle step timeout
var timeoutHandled = await sagaOrchestrationService.HandleTimeoutAsync(saga.Id, stepId);
if (timeoutHandled)
{
    Console.WriteLine("Timeout handled, step marked for retry or compensation");
}

// Example 5: Compensate a failed saga
var compensatedSaga = await sagaOrchestrationService.CompensateSagaAsync(saga.Id);
Console.WriteLine($"Compensation completed: {compensatedSaga.Status}");

// Example 6: Compensate with explicit strategy
var compensatedWithStrategy = await sagaOrchestrationService.CompensateSagaAsync(
    saga.Id,
    CompensationStrategy.ReverseOrder
);

// Example 7: Abort a running saga
await sagaOrchestrationService.AbortSagaAsync(saga.Id, "User requested cancellation");

// Example 8: Retrieve saga details
var retrievedSaga = await sagaOrchestrationService.GetSagaAsync(saga.Id);
Console.WriteLine($"Saga status: {retrievedSaga.Status}");
Console.WriteLine($"Failed at: {retrievedSaga.FailedAt}");

// Example 9: List sagas with filtering
var runningSagas = await sagaOrchestrationService.ListSagasAsync(SagaStatus.Running);
Console.WriteLine($"Found {runningSagas.Count} running sagas");

var failedSagas = await sagaOrchestrationService.ListSagasAsync(SagaStatus.Failed);
Console.WriteLine($"Found {failedSagas.Count} failed sagas");
```

## IMetricsService

The `IMetricsService` interface provides methods for collecting and reporting metrics related to saga execution. It allows you to retrieve overall saga metrics, step-specific metrics, and performance statistics. The service can be used to monitor the health and efficiency of the saga system.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;

// Initialize the metrics service
var metricsService = new MetricsService(
    sagaRepository,
    stepRepository,
    logger
);

// Get overall saga metrics
var sagaMetrics = await metricsService.GetMetricsAsync();
Console.WriteLine($"Total Sagas: {sagaMetrics.TotalSagas}");
Console.WriteLine($"Completed Sagas: {sagaMetrics.CompletedSagas}");
Console.WriteLine($"Failed Sagas: {sagaMetrics.FailedSagas}");
Console.WriteLine($"Success Rate: {sagaMetrics.SuccessRate}%");
Console.WriteLine($"Average Duration: {sagaMetrics.AverageDurationSeconds} seconds");

// Get step-specific metrics
var stepMetrics = await metricsService.GetStepMetricsAsync("Validate Order");
Console.WriteLine($"Step Name: {stepMetrics.StepName}");
Console.WriteLine($"Total Executions: {stepMetrics.TotalExecutions}");
Console.WriteLine($"Successful Executions: {stepMetrics.SuccessfulExecutions}");
Console.WriteLine($"Failed Executions: {stepMetrics.FailedExecutions}");
Console.WriteLine($"Success Rate: {stepMetrics.SuccessRate}%");
Console.WriteLine($"Average Duration: {stepMetrics.AverageDurationMs} ms");

// Get performance statistics
var performanceStats = await metricsService.GetPerformanceStatsAsync();
Console.WriteLine($"Average Duration: {performanceStats.AverageDurationSeconds} seconds");
Console.WriteLine($"Min Duration: {performanceStats.MinDurationSeconds} seconds");
Console.WriteLine($"Max Duration: {performanceStats.MaxDurationSeconds} seconds");
Console.WriteLine($"Median Duration: {performanceStats.MedianDurationSeconds} seconds");
Console.WriteLine($"P95 Duration: {performanceStats.P95DurationSeconds} seconds");
Console.WriteLine($"P99 Duration: {performanceStats.P99DurationSeconds} seconds");
```